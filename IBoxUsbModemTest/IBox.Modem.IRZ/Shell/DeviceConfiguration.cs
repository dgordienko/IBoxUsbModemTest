﻿using Newtonsoft.Json;

namespace IBox.Modem.IRZ.Shell
{
    public class DeviceConfiguration : DbStorageItem
    {
        private int _baudRate = 19200;

        [JsonProperty("port")] public string PortName { get; set; }

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
            return other != null && string.Equals(PortName, other.PortName) && BaudRate == other.BaudRate;
        }
    }
}