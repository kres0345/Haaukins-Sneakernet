using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuacamoleCommonConnection.Guacamole
{
    public class Client
    {
        const int STATE_IDLE = 0;
        const int STATE_CONNECTING = 1;
        const int STATE_WAITING = 2;
        const int STATE_CONNECTED = 3;
        const int STATE_DISCONNECTING = 4;
        const int STATE_DISCONNECTED = 5;

        private readonly Tunnel tunnel;
        private readonly IntegerPool streamIndices;
        private readonly OutputStream[] outputStreams;
        private int pingInterval;
        private int currentState = STATE_IDLE;
        bool IsConnected => currentState == STATE_CONNECTED || currentState == STATE_WAITING;

        public Client(Tunnel tunnel)
        {
            this.tunnel = tunnel;

            tunnel.OnInstruction += Tunnel_OnInstruction;
        }

        public async Task SendSize(int width, int height)
        {
            if (!IsConnected)
            {
                return;
            }

            await tunnel.SendMessageAsync("size", width, height);
        }

        public async Task SendKeyEvent(bool pressed, int keySymbol)
        {
            if (!IsConnected)
            {
                return;
            }

            await tunnel.SendMessageAsync("key", keySymbol, pressed);
        }

        public async Task SendMouseState(int mouseState, bool applyDisplayScale)
        {
            throw new NotImplementedException("Client.js:334");
        }

        public async Task SendTouchState(int touchState, bool applyDisplayScale)
        {
            throw new NotImplementedException("Client.js:379");
        }

        public OutputStream CreateOutputStream()
        {
            // Allocate index
            int index = streamIndices.Next();

            // Return new stream
            var stream = outputStreams[index] = new OutputStream(this, index);
            return stream;
        }

        public async Task<OutputStream> CreateAudioStream(string mimeType)
        {
            var stream = CreateOutputStream();
            await tunnel.SendMessageAsync("audio", stream.Index, mimeType);
            return stream;
        }

        public async Task<OutputStream> CreateFileStream(string mimeType, string filename)
        {
            var stream = CreateOutputStream();
            await tunnel.SendMessageAsync("file", stream.Index, mimeType, filename);
            return stream;
        }

        public async Task<OutputStream> CreatePipeStream(string mimeType, string name)
        {
            var stream = CreateOutputStream();
            await tunnel.SendMessageAsync("pipe", stream.Index, mimeType, name);
            return stream;
        }

        public async Task<OutputStream> CreateClipboardStream(string mimeType)
        {
            var stream = CreateOutputStream();
            await tunnel.SendMessageAsync("clipboard", stream.Index, mimeType);
            return stream;
        }

        public async Task<OutputStream> CreateArgumentValueStream(string mimeType, string name)
        {
            var stream = CreateOutputStream();
            await tunnel.SendMessageAsync("argv", stream.Index, mimeType, name);
            return stream;
        }

        public async Task<OutputStream> CreateObjectOutputStream(int index, string mimeType, string name)
        {
            var stream = CreateOutputStream();
            await tunnel.SendMessageAsync("put", index, stream.Index, mimeType, name);
            return stream;
        }

        public async Task RequestObjectInputStream(int index, string name)
        {
            if (!IsConnected)
            {
                return;
            }

            await tunnel.SendMessageAsync("get", index, name);
        }

        public async Task SendAck(int index, string message, int code)
        {
            if (!IsConnected)
            {
                return;
            }

            await tunnel.SendMessageAsync("ack", index, message, code);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">Base64-encoded</param>
        /// <returns></returns>
        public async Task SendBlob(int index, string data)
        {
            if (!IsConnected)
            {
                return;
            }

            await tunnel.SendMessageAsync("blob", index, data);
        }

        public async Task EndStream(int index)
        {
            if (!IsConnected)
            {
                return;
            }

            await tunnel.SendMessageAsync("end", index);

            if (outputStreams.Length < index)
            {
                streamIndices.Free(index);
                outputStreams[index] = null;
            }
        }

        public event Action<int> OnStateChange;
        public event Action<string> OnName;
        public event Action<Status> OnError;
        /// <summary>
        /// stream, mimetype, name
        /// </summary>
        public event Action<InputStream, string, string> OnArgv;
        /// <summary>
        /// stream, mimetype
        /// </summary>
        public event Action<InputStream, string> OnClipboard;
        /// <summary>
        /// stream, mimetype, filename
        /// </summary>
        public event Action<InputStream, string, string> OnFile;
        public event Action<GuacamoleObject, string> OnFilesystem;
        /// <summary>
        /// stream, mimetype, name
        /// </summary>
        public event Action<InputStream, string, string> OnPipe;
        /// <summary>
        /// parameters
        /// </summary>
        public event Action<string[]> OnRequired;
        /// <summary>
        /// timestamp
        /// </summary>
        public event Action<long> OnSync;


        public void SetState(int state)
        {
            if (currentState == state)
            {
                return;
            }   

            currentState = state;

            OnStateChange?.Invoke(currentState);
        }

        private void GetParser(int index)
        {

        }

        private async void Tunnel_OnInstruction(string opcode, List<string> parameters) => GetInstructionHandlerAsync(opcode)?.Invoke(parameters);

        public Action<List<string>> GetInstructionHandlerAsync(string msgType)
        {
            switch (msgType)
            {
                case "ack":
                    return async (parameters) =>
                    {

                    };
                case "arc":
                    return (parameters) =>
                    {

                    };
                case "argv":
                    return (parameters) =>
                    {

                    };
                case "audio":
                    return (parameters) =>
                    {

                    };
                case "blob":
                    return (parameters) =>
                    {

                    };
                case "body":
                    return (parameters) =>
                    {

                    };
                case "cfill":
                    return (parameters) =>
                    {

                    };
                case "clip":
                    return (parameters) =>
                    {

                    };
                case "clipboard":
                    return (parameters) =>
                    {

                    };
                case "close":
                    return (parameters) =>
                    {

                    };
                case "copy":
                    return (parameters) =>
                    {

                    };
                case "cstroke":
                    return (parameters) =>
                    {

                    };
                case "cursor":
                    return (parameters) =>
                    {

                    };
                case "curve":
                    return (parameters) =>
                    {

                    };
                case "disconnect":
                    return (parameters) =>
                    {

                    };
                case "dispose":
                    return (parameters) =>
                    {

                    };
                case "error":
                    return (parameters) =>
                    {

                    };
                case "end":
                    return (parameters) =>
                    {
                        
                    };
                case "file":
                    return (parameters) =>
                    {

                    };
                case "filesystem":
                    return (parameters) =>
                    {

                    };
                case "identity":
                    return (parameters) =>
                    {

                    };
                case "img":
                    return (parameters) =>
                    {

                    };
                case "jpeg":
                    return (parameters) =>
                    {

                    };
                case "lfill":
                    return (parameters) =>
                    {

                    };
                case "line":
                    return (parameters) =>
                    {

                    };
                case "lstroke":
                    return (parameters) =>
                    {

                    };
                case "mouse":
                    return (parameters) =>
                    {

                    };
                case "move":
                    return (parameters) =>
                    {

                    };
                case "name":
                    return (parameters) =>
                    {

                    };
                case "nest":
                    return (parameters) =>
                    {

                    };
                case "pipe":
                    return (parameters) =>
                    {

                    };
                case "png":
                    return (parameters) =>
                    {

                    };
                case "pop":
                    return (parameters) =>
                    {

                    };
                case "push":
                    return (parameters) =>
                    {

                    };
                case "rect":
                    return (parameters) =>
                    {

                    };
                case "required":
                    return (parameters) =>
                    {

                    };
                case "reset":
                    return (parameters) =>
                    {

                    };
                case "set":
                    return (parameters) =>
                    {

                    };
                case "shade":
                    return (parameters) =>
                    {

                    };
                case "size":
                    return (parameters) =>
                    {

                    };
                case "start":
                    return (parameters) =>
                    {

                    };
                case "sync":
                    return (parameters) =>
                    {

                    };
                case "transfer":
                    return (parameters) =>
                    {

                    };
                case "transform":
                    return (parameters) =>
                    {

                    };
                case "undefine":
                    return (parameters) =>
                    {

                    };
                case "video":
                    return (parameters) =>
                    {

                    };
                default:
                    return null;
            }
        }
    }
}
