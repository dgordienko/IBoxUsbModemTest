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
        private const int Sleep = 500;
        private readonly object _locker = new object();
        private static readonly Lazy<ModemManager> Lazy =
            new Lazy<ModemManager>(() => new ModemManager());

        private readonly SerialPort _serial;
        private volatile bool _keepReading;
        private Thread _readThread;

        private ModemManager()
        {
            _serial = new SerialPort();
            _readThread = null;
            _keepReading = false;
        }

        public static ModemManager Instance => Lazy.Value;

        /// <summary>
        ///     Return TRUE if the serial port is currently connected
        /// </summary>
        public bool IsOpen => _serial.IsOpen;

        /// <summary>
        ///     Update the serial port status to the event subscriber
        /// </summary>
        public event EventHandler<string> OnStatusChanged;

        /// <summary>
        ///     Update received data from the serial port to the event subscriber
        /// </summary>
        public event EventHandler<string> OnDataReceived;

        /// <summary>
        ///     Update TRUE/FALSE for the serial port connection to the event subscriber
        /// </summary>
        public event EventHandler<bool> OnSerialPortOpened;

        /// <summary>
        ///     Open the serial port connection
        /// </summary>
        /// <param name="portname">ttyUSB0 / ttyUSB1 / ttyUSB2 / etc.</param>
        /// <param name="baudrate">115200</param>
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
                _serial.PortName = $"{portname}";
                _serial.BaudRate = baudrate;
                _serial.Parity = parity;
                _serial.DataBits = databits;
                _serial.StopBits = stopbits;
                _serial.Handshake = handshake;

                _serial.ReadTimeout = 300;
                _serial.WriteTimeout = 300;

                _serial.Open();

                _serial.DiscardInBuffer();
                _serial.DiscardOutBuffer();

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

            if (_serial.IsOpen)
            {
                var sb = StopBits.None.ToString().Substring(0, 1);
                switch (_serial.StopBits)
                {
                    case StopBits.One:
                        sb = "1";
                        break;
                    case StopBits.OnePointFive:
                        sb = "1.5";
                        break;
                    case StopBits.Two:
                        sb = "2";
                        break;
                }

                var p = _serial.Parity.ToString().Substring(0, 1);
                var hs = _serial.Handshake == Handshake.None ? "No Handshake" :
                    _serial.Handshake.ToString();

                OnStatusChanged?.Invoke(this,
                    $"Connected to {_serial.PortName}: {_serial.BaudRate} bps, " +
                        "{_serial.DataBits}{p}{sb}, {hs}.");

                OnSerialPortOpened?.Invoke(this, true);
            }
            else
            {
                OnStatusChanged?.Invoke(this, $"{portname} already in use.");
                OnSerialPortOpened?.Invoke(this, false);
            }
        }

        /// <summary>
        ///     Close the serial port connection
        /// </summary>
        public void Close()
        {
            StopReading();
            _serial.Close();
            OnStatusChanged?.Invoke(this, "Connection closed.");
            OnSerialPortOpened?.Invoke(this, false);
        }

        /// <summary>
        ///     Send/write string to the serial port
        /// </summary>
        /// <param name="message"></param>
        public void SendString(string message)
        {

            {
                if (_serial.IsOpen)
                    try
                    {
                        var data = Encoding.ASCII.GetBytes($"{message}\r\n");
                        _serial.Write(data, 0, data.Length);
                        OnStatusChanged?.Invoke(this, $"Command sent: {message}");
                    }
                    catch (Exception exception)
                    {
                        OnStatusChanged?.Invoke(this, $"Failed to send command {message}: {exception.Message}");
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
                //_readThread.Abort(100);
               // _readThread.Join();
                _readThread = null;
                _keepReading = false;
            }
        }

        private void ReadPort()
        {
            var waitTime = new TimeSpan(0, 0, 0, 0, Sleep);
            lock (this)
            {
                while (_keepReading)
                {
                    if (_serial.IsOpen)
                    {
                        try
                        {
                            var readBuffer = new byte[_serial.ReadBufferSize + 1];
                            var count = _serial.Read(readBuffer, 0, _serial.ReadBufferSize);
                            var data = Encoding.ASCII.GetString(readBuffer, 0, count);
                            OnDataReceived?.Invoke(this, data);
                        }
                        catch (ThreadAbortException abort)
                        {
                            Debug.WriteLine(abort.Message);
                        }
                        catch (TimeoutException timeout)
                        {
                            Debug.WriteLine(timeout.Message);
                        }
                    }
                    else
                    {
                        //lock (_locker)
                        {
                            Monitor.Wait(this, waitTime);
                        }
                    }
                }
            }
        }

    }

}