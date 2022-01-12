using System;
using System.Threading.Tasks;

namespace GuacamoleCommonConnection.Guacamole
{
    public class OutputStream
    {
        public readonly int Index;
        private readonly Client client;

        public event Action<Status> OnAck;

        public OutputStream(Client client, int index)
        {
            this.client = client;
            this.Index = index;
        }

        /// <summary>
        /// Base64, i think.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task SendBlob(string data) => await client.SendBlob(Index, data);

        public async Task SendEnd() => await client.EndStream(Index);
    }
}
