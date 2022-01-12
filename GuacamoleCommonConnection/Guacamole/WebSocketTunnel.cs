using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GuacamoleCommonConnection.Guacamole
{
    public class WebSocketTunnel : Tunnel
    {
        private const int PING_FREQUENCY = 500;
        private System.Timers.Timer receiveTimeout, unstableTimeout, pingInterval;
        private ClientWebSocket socket;
        private string tunnelURL;
        public override event Action<string, List<string>> OnInstruction;

        public WebSocketTunnel(string tunnelURL)
        {
            this.tunnelURL = tunnelURL;
        }

        private void ResetTimeout()
        {
            if (state == State.Unstable)
            {
                state = State.Open;
            }

            receiveTimeout = new System.Timers.Timer();
            receiveTimeout.Elapsed += async (o, e) =>
            {
                await CloseTunnel(new Status(Status.StatusCode.UpstreamTimeout, "Server timeout."));
                receiveTimeout.Stop();
            };
            receiveTimeout.Interval = Receive_Timeout;

            unstableTimeout = new System.Timers.Timer();
            unstableTimeout.Elapsed += (o, e) =>
            {
                state = State.Unstable;
                unstableTimeout.Stop();
            };
            unstableTimeout.Interval = Unstable_Threshold;
        }

        private async Task CloseTunnel(Status status)
        {
            receiveTimeout?.Stop();
            unstableTimeout?.Stop();
            pingInterval?.Stop();

            if (state == State.CLOSED)
            {
                return;
            }

            // If connection closed abnormally, signal error.
            if (status.Code != (int)Status.StatusCode.Success)
            {
                throw new Exception("Idunno");
            }

            state = State.CLOSED;
            await socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed because I say so", System.Threading.CancellationToken.None);
        }

        private string GetElement(string value)
        {
            return value.Length + "." + value;
        }

        public override async Task SendMessageAsync(string message, params object[] parameters)
        {
            if (!IsConnected)
            {
                return;
            }

            if (parameters.Length == 0)
            {
                throw new Exception("No parameters");
            }

            StringBuilder msgSb = new StringBuilder(GetElement(message));

            for (int i = 0; i < parameters.Length; i++)
            {
                msgSb.Append(',');
                msgSb.Append(GetElement(parameters[i].ToString()));
            }

            msgSb.Append(';');

            byte[] bytes = Encoding.UTF8.GetBytes(msgSb.ToString());
            await socket.SendAsync(bytes, WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
        }

        public override async Task ConnectAsync(byte[] data)
        {
            ResetTimeout();

            state = State.CONNECTING;

            socket = new ClientWebSocket();
            socket.Options.AddSubProtocol("guacamole");
            await socket.ConnectAsync(new Uri(tunnelURL), System.Threading.CancellationToken.None);

            ResetTimeout();

            pingInterval = new Timer(PING_FREQUENCY);
            pingInterval.Elapsed += async (e, o) =>
            {
                await SendMessageAsync(INTERNAL_DATA_OPCODE, "ping", DateTimeOffset.Now.ToUnixTimeMilliseconds());
            };

            await ReceiveLoop();
        }

        public async Task ReceiveLoop()
        {
            while (true)
            {
                WebSocketReceiveResult r;
                byte[] messageBytes;
                do
                {
                    messageBytes = new byte[10];
                    r = await socket.ReceiveAsync(messageBytes, System.Threading.CancellationToken.None);
                } while (!r.EndOfMessage);

                messageBytes = null;

                ResetTimeout();

                int startIndex = 0;
                int elementEnd = 0;

                string message = Encoding.UTF8.GetString(messageBytes);
                List<string> elements = new List<string>();

                do
                {
                    // Search for end of length
                    var lengthEnd = message.IndexOf(".", startIndex);
                    if (lengthEnd != -1)
                    {
                        // Parse length
                        int length = int.Parse(message.Substring(elementEnd + 1, lengthEnd));

                        // Calculate start of element
                        startIndex = lengthEnd + 1;

                        // Calculate location of element terminator
                        elementEnd = startIndex + length;
                    }
                    // If no period, incomplete instruction.
                    else
                    {
                        await CloseTunnel(new Status(Status.StatusCode.ServerError, "Incomplete instruction."));
                    }

                    // We now have enough data for the element. Parse.
                    var element = message.Substring(startIndex, elementEnd);
                    var terminator = message.Substring(elementEnd, elementEnd + 1);

                    // Add element to array
                    elements.Add(element);

                    // If last element, handle instruction
                    if (terminator == ";")
                    {
                        // Get opcode
                        string opcode = elements[0];
                        elements.RemoveAt(0);

                        // Update state and UUID when first instruction received
                        if (UUID == null)
                        {

                            // Associate tunnel UUID if received
                            if (opcode == Tunnel.INTERNAL_DATA_OPCODE)
                            {
                                UUID = elements[0];
                            }

                            // Tunnel is now open and UUID is available
                            state = State.Open;
                        }

                        // Call instruction handler.
                        if (opcode != Tunnel.INTERNAL_DATA_OPCODE)
                        {
                            OnInstruction?.Invoke(opcode, elements);
                        }

                        // Clear elements
                        elements.Clear();
                    }

                    // Start searching for length at character after
                    // element terminator
                    startIndex = elementEnd + 1;

                } while (startIndex < message.Length);
            }
        }

        public override async Task DisconnectAsync()
        {
            await CloseTunnel(new Status(Status.StatusCode.Success, "Manually closed."));
        }
    }
}
