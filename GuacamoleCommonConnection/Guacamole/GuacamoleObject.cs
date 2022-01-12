using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuacamoleCommonConnection.Guacamole
{
    public class GuacamoleObject
    {
        public int Index;
        public Dictionary<string, Queue<Action>> BodyCallbacks;
        private Client client;

        public GuacamoleObject(Client client, int index)
        {
            Index = index;
            this.client = client;
        }

        public Action DequeueBodyCallback(string name)
        {
            if (BodyCallbacks.TryGetValue(name, out Queue<Action> callbackQueue))
            {
                return null;
            }

            // Otherwise, pull off first callback, deleting the queue if empty
            Action callback = callbackQueue.Dequeue();
            if (callbackQueue.Count == 0)
            {
                BodyCallbacks.Remove(name);
            }

            // Return found callback
            return callback;
        }

        public void EnqueueBodyCallback(string name, Action callback)
        {
            if (!BodyCallbacks.TryGetValue(name, out Queue<Action> callbackQueue))
            {
                callbackQueue = new Queue<Action>();
                BodyCallbacks[name] = callbackQueue;
            }

            callbackQueue.Enqueue(callback);
        }

        public async Task RequestInputStream(string name, Action bodyCallback)
        {
            if (!(bodyCallback is null))
            {
                EnqueueBodyCallback(name, bodyCallback);
            }

            await client.RequestObjectInputStream(Index, name);
        }

        public Task<OutputStream> CreateOutputStream(string mimeType, string name)
        {
            return client.CreateObjectOutputStream(Index, mimeType, name);
        }
    }
}
