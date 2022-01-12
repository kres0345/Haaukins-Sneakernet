using System;
using System.Threading.Tasks;

namespace GuacamoleCommonConnection.Guacamole
{
    public class InputStream
    {
        public int Index;
        private Client client;

        /// <summary>
        /// Base64.
        /// </summary>
        public Action<string> OnBlob;
        public Action OnEnd;

        public InputStream(Client client, int index)
        {
            this.client = client;
            Index = index;
        }

        public async Task SendAck(string message, int code)
        {
            await client.SendAck(Index, message, code);
        }
    }
}
