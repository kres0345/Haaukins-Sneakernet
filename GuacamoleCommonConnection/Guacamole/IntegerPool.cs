using System.Collections.Generic;

namespace GuacamoleCommonConnection.Guacamole
{
    public class IntegerPool
    {
        private readonly Queue<int> pool = new Queue<int>();
        private int nextInteger = 0;

        public int Next()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return nextInteger++;
        }

        public void Free(int integer)
        {
            pool.Enqueue(integer);
        }
    }
}
