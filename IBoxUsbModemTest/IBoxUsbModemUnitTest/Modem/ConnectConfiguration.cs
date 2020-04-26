using Newtonsoft.Json;

namespace IBoxUsbModemUnitTest.Modem
{
    public class ConnectConfiguration : DeviceConfiguration
    {
        [JsonProperty("type")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.Gprs;

        public const int NetErrorLimitDefault = 15;
        private int _NetErrorLimit = NetErrorLimitDefault;
        public int NetErrorLimit
        {
            get { return _NetErrorLimit; }
            set
            {
                if (value < 5)
                    return;
                _NetErrorLimit = value;
            }
        }

        public override string ToString()
        {
            return string.Format(" {0}, connectType: {1}", base.ToString(), ConnectionType);
        }

        public bool Equals(ConnectConfiguration other)
        {
            return base.Equals(other) && (ConnectionType == other.ConnectionType);
        }

        private ConnectionInfo _lastInfo;
        public ConnectionInfo LastInfo
        {
            get { return _lastInfo ?? (_lastInfo = new ConnectionInfo()); }
            set { _lastInfo = value; }
        }
    }
}
