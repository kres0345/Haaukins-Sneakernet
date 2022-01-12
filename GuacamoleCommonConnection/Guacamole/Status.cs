namespace GuacamoleCommonConnection.Guacamole
{
    public class Status
    {
        public enum StatusCode : int
        {
            Success = 0x0000,
            Unsupported = 0x0100,
            ServerError = 0x0200,
            ServerBusy = 0x0201,
            UpstreamTimeout = 0x0202,
            UpstreamError = 0x0203,
            ResourceNotFound = 0x0204,
            ResourceConflict = 0x0205,
            ResourceClosed = 0x0206,
            UpstreamNotFound = 0x0207,
            UpstreamUnavailable = 0x0208,
            SessionConflict = 0x0209,
            SessionTimeout = 0x020A,
            SessionClosed = 0x020B,
            ClientBadRequest = 0x0300,
            ClientUnauthorized = 0x0301,
            ClientForbidden = 0x0303,
            ClientTimeout = 0x0308,
            ClientOverrun = 0x030D,
            ClientBadType = 0x030F,
            ClientTooMany = 0x031D,
        }

        public readonly int Code;
        public readonly string Message;
        public bool IsError
        {
            get
            {
                return Code < 0 || Code > 0x00FF;
            }
        }

        public Status(StatusCode code, string message)
        {
            Code = (int)code;
            Message = message;
        }

        public Status(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
