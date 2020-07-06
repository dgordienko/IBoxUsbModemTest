using Newtonsoft.Json;

namespace IBox.Modem.IRZ.Shell
{
    public class ConnectConfiguration : DeviceConfiguration
    {
        private const int NetErrorLimitDefault = 15;

        private ConnectionInfo _lastInfo;

        private int _netErrorLimit = NetErrorLimitDefault;

        [JsonProperty("type")] public ConnectionType ConnectionType { get; set; } = ConnectionType.Gprs;

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

        public ConnectionInfo LastInfo
        {
            get => _lastInfo ?? (_lastInfo = new ConnectionInfo());
            set => _lastInfo = value;
        }

        public override string ToString()
        {
            return $" {base.ToString()}, connectType: {ConnectionType}";
        }

        public bool Equals(ConnectConfiguration other)
        {
            return base.Equals(other) && ConnectionType == other.ConnectionType;
        }
    }
}