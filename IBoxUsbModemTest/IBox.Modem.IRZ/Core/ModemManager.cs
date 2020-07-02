using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace IBox.Modem.IRZ.Core
{
    /// <summary>
    /// Serial-USB manager 
    /// </summary>
    public sealed class ModemManager
    {
        private static readonly Lazy<ModemManager> lazy = new Lazy<ModemManager>(() => new ModemManager());

        public static ModemManager Instance => lazy.Value;

        private readonly SerialPort serial;
        private Thread _readThread;
        private volatile bool _keepReading;

        private ModemManager()
        {
            serial = new SerialPort();
            _readThread = null;
            _keepReading = false;
        }

        /// <summary>
        /// Update the serial port status to the event subscriber
        /// </summary>
        public event EventHandler<string> OnStatusChanged;

        /// <summary>
        /// Update received data from the serial port to the event subscriber
        /// </summary>
        public event EventHandler<string> OnDataReceived;

        /// <summary>
        /// Update TRUE/FALSE for the serial port connection to the event subscriber
        /// </summary>
        public event EventHandler<bool> OnSerialPortOpened;

        /// <summary>
        /// Return TRUE if the serial port is currently connected
        /// </summary>
        public bool IsOpen { get { return serial.IsOpen; } }

        /// <summary>
        /// Open the serial port connection 
        /// </summary>
        /// <param name="portname">ttyUSB0 / ttyUSB1 / ttyUSB2 / etc.</param>
        /// <param name="baudrate">115200/param>
        /// <param name="parity">None / Odd / Even / Mark / Space</param>
        /// <param name="databits">5 / 6 / 7 / 8</param>
        /// <param name="stopbits">None / One / Two / OnePointFive</param>
        /// <param name="handshake">None / XOnXOff / RequestToSend / RequestToSendXOnXOff</param>
        public void Open(
            string portname = "/dev/ttyUSB0",
            int baudrate = 115200,
            Parity parity = Parity.None,
            int databits = 8,
            StopBits stopbits = StopBits.One,
            Handshake handshake = Handshake.None)
        {
            Close();

            try
            {
                serial.PortName = $"{portname}";
                serial.BaudRate = baudrate;
                serial.Parity = parity;
                serial.DataBits = databits;
                serial.StopBits = stopbits;
                serial.Handshake = handshake;

                serial.ReadTimeout = 300;
                serial.WriteTimeout = 300;

                serial.Open();

                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();

                StartReading();
            }
            catch (IOException exception)
            {
                OnStatusChanged?.Invoke(this, $"{portname} does not exist.{Environment.NewLine}{exception.StackTrace}");
            }
            catch (UnauthorizedAccessException exception)
            {
                OnStatusChanged?.Invoke(this, $"{portname} already in use.{Environment.NewLine}{exception.StackTrace}");
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, "Error: " + ex.Message);
            }

            if (serial.IsOpen)
            {
                string sb = StopBits.None.ToString().Substring(0, 1);
                switch (serial.StopBits)
                {
                    case StopBits.One:
                        sb = "1"; break;
                    case StopBits.OnePointFive:
                        sb = "1.5"; break;
                    case StopBits.Two:
                        sb = "2"; break;
                    default:
                        break;
                }

                string p = serial.Parity.ToString().Substring(0, 1);
                string hs = serial.Handshake == Handshake.None ? "No Handshake" : serial.Handshake.ToString();

                OnStatusChanged?.Invoke(this, string.Format("Connected to {0}: {1} bps, {2}{3}{4}, {5}.",
                    serial.PortName, serial.BaudRate, serial.DataBits,
                    p, sb, hs));

                OnSerialPortOpened?.Invoke(this, true);
            }
            else
            {
                OnStatusChanged?.Invoke(this, string.Format("{0} already in use.", portname));
                OnSerialPortOpened?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Close the serial port connection
        /// </summary>
        public void Close()
        {
            StopReading();
            serial.Close();
            OnStatusChanged?.Invoke(this, "Connection closed.");
            OnSerialPortOpened?.Invoke(this, false);
        }

        /// <summary>
        /// Send/write string to the serial port
        /// </summary>
        /// <param name="message"></param>
        public void SendString(string message)
        {
            if (serial.IsOpen)
            {
                try
                {
                    var data = Encoding.ASCII.GetBytes($"{message}\r\n");
                    serial.Write(data, 0, data.Length);
                    OnStatusChanged?.Invoke(this, $"Command sent: {message}");
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke(this, $"Failed to send command {message}: {ex.Message}");
                }
            }
        }

        private void StartReading()
        {
            if (!_keepReading)
            {
                _keepReading = true;
                _readThread = new Thread(ReadPort);
                _readThread.Start();
            }
        }

        private void StopReading()
        {
            if (_keepReading)
            {
                _keepReading = false;
                _readThread.Join();
                _readThread = null;
            }
        }

        private void ReadPort()
        {
            while (_keepReading)
            {
                if (serial.IsOpen)
                {
                    var readBuffer = new byte[serial.ReadBufferSize + 1];
                    try
                    {
                        var count = serial.Read(readBuffer, 0, serial.ReadBufferSize);
                        var data = Encoding.ASCII.GetString(readBuffer, 0, count);
                        OnDataReceived?.Invoke(this, data);
                    }
                    catch (TimeoutException exception)
                    {
                        //_keepReading = false;
                        Debug.WriteLine(exception);
                    }
                }
                else
                {
                    var waitTime = new TimeSpan(0, 0, 0, 0, 300);
                    Thread.Sleep(waitTime);
                }
            }
        }
    }
}
