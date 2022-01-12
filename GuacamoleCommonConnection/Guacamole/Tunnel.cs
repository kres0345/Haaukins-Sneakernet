using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuacamoleCommonConnection.Guacamole
{
    public class Tunnel
    {
        public enum State
        {
            CONNECTING = 0,
            Open = 1,
            CLOSED = 2,
            Unstable = 3
        }

        public static readonly string INTERNAL_DATA_OPCODE = "";

        private State _state;
        protected State state
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public bool IsConnected => state == State.Open || state == State.Unstable;

        public const int Receive_Timeout = 15000;
        public const int Unstable_Threshold = 1500;

        public virtual event Action<string> OnUUID;
        public virtual event Action<string, List<string>> OnInstruction;

        private string _UUID;
        public string UUID
        {
            get
            {
                return _UUID;
            }
            set
            {
                _UUID = value;
                OnUUID?.Invoke(_UUID);
            }
        }

        public Tunnel()
        {
            state = State.CONNECTING;
        }

        public virtual async Task ConnectAsync(byte[] data) { }

        public virtual async Task DisconnectAsync() { }

        public virtual async Task SendMessageAsync(string message, params object[] parameters) { }
    }
}
