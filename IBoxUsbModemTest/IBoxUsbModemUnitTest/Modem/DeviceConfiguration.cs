using Newtonsoft.Json;

namespace IBoxUsbModemUnitTest.Modem
{
    public class DeviceConfiguration : DbStorageItem
    {
        [JsonProperty("port")]
        public string PortName { get; set; }

        private int _baudRate = 19200;
        [JsonProperty("baudrate")]
        public int BaudRate
        {
            get => _baudRate;
            set
            {
                if (value < 9600)
                    return;
                _baudRate = value;
            }
        }

        public override string ToString()
        {
            return $"port={PortName}, baudrate={BaudRate}";
        }

        public bool Equals(DeviceConfiguration other)
        {
            //return base.Equals(other);
            return (other != null) && string.Equals(PortName, other.PortName) && (BaudRate == other.BaudRate);
        }
    }
}
