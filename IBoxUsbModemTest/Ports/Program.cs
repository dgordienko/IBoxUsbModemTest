using System;
using System.IO.Ports;

namespace Ports
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var ports = SerialPort.GetPortNames();
        }
    }
}
