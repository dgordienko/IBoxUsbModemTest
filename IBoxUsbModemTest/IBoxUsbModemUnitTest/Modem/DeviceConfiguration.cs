using Newtonsoft.Json;

namespace IBoxUsbModemUnitTest.Modem
{
    public class DeviceConfiguration : DbStorageItem
    {
        private string _PortName;
        [JsonProperty("port")]
        public string PortName
        {
            get { return _PortName; }
            set
            {
                /*
                if (string.IsNullOrEmpty(value))
                    return;
                */
                _PortName = value;
            }
        }

        private int _BaudRate = 19200;
        [JsonProperty("baudrate")]
        public int BaudRate
        {
            get { return _BaudRate; }
            set
            {
                if (value < 9600)
                    return;
                _BaudRate = value;
            }
        }

        public override string ToString()
        {
            return string.Format("port={0}, baudrate={1}", PortName, BaudRate);
        }

        public bool Equals(DeviceConfiguration other)
        {
            //return base.Equals(other);
            return (other != null) && string.Equals(PortName, other.PortName) && (BaudRate == other.BaudRate);
        }
    }
}
