using Newtonsoft.Json;

namespace IBoxUsbModemUnitTest.Modem
{
    public class ConnectConfiguration : DeviceConfiguration
    {
        [JsonProperty("type")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.Gprs;

        private const int NetErrorLimitDefault = 15;
        
        private int _netErrorLimit = NetErrorLimitDefault;
        public int NetErrorLimit
        {
            get => _netErrorLimit;
            set
            {
                if (value < 5)
                    return;
                _netErrorLimit = value;
            }
        }

        public override string ToString()
        {
            return $" {base.ToString()}, connectType: {ConnectionType}";
        }

        public bool Equals(ConnectConfiguration other)
        {
            return base.Equals(other) && (ConnectionType == other.ConnectionType);
        }

        private ConnectionInfo _lastInfo;
        public ConnectionInfo LastInfo
        {
            get => _lastInfo ?? (_lastInfo = new ConnectionInfo());
            set => _lastInfo = value;
        }
    }
}
