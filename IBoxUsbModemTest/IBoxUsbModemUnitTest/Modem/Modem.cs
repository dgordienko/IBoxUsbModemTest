using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Polly;
using Polly.Retry;
using Serilog;

namespace IBoxUsbModemUnitTest.Modem
{

    public sealed class SerialPortHelper
    {
        private static readonly Lazy<SerialPortHelper> lazy = new Lazy<SerialPortHelper>(() => new SerialPortHelper());
        public static SerialPortHelper Instance { get { return lazy.Value; } }

        private readonly SerialPort serial;
        private Thread _readThread;
        private volatile bool _keepReading;

        private SerialPortHelper()
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
            string portname = "ttyUSB0",
            int baudrate = 115200,
            Parity parity = Parity.None,
            int databits = 8,
            StopBits stopbits = StopBits.One,
            Handshake handshake = Handshake.None)
        {
            Close();

            try
            {
                serial.PortName = portname;
                serial.BaudRate = baudrate;
                serial.Parity = parity;
                serial.DataBits = databits;
                serial.StopBits = stopbits;
                serial.Handshake = handshake;

                serial.ReadTimeout = 50;
                serial.WriteTimeout = 50;

                serial.Open();
                StartReading();
            }
            catch (IOException)
            {
                OnStatusChanged?.Invoke(this, string.Format("{0} does not exist.", portname));
            }
            catch (UnauthorizedAccessException)
            {
                OnStatusChanged?.Invoke(this, string.Format("{0} already in use.", portname));
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

                    serial.Write(message);

                    OnStatusChanged?.Invoke(this, string.Format("Message sent: {0}", message));
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke(this, string.Format("Failed to send string: {0}", ex.Message));
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
                    catch (TimeoutException) { }
                }
                else
                {
                    TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 300);
                    Thread.Sleep(waitTime);
                }
            }
        }
    }


    public static class FormatResponse
    {
        public static string ToResponseData(this string value)
        {
            return $"{ value.Replace('\r', '|').Replace('\n', '|')}";
        }
    }
    /// <summary>
    /// MC35    http://www.radiofid.ru/upload_data//at-commands/mc35at.pdf
    /// Wavecom http://nsk-embedded-downloads.googlecode.com/files/wavecom%202400a%20at%20commands.pdf
    /// </summary>
    public class Modem : Adapter, IModem
    {
        readonly ILogger logger = new LoggerConfiguration().
            WriteTo.Console().CreateLogger();

        private string _portName;
        private int _baudeRate;

        public List<int> AvailableBaudRates { get; } = new List<int> { 9600, 19200, 38400, 115200 };

        public enum RegStatus
        {
            Error = -1,
            NotRegistered = 0,
            Registered = 1,
            Searching = 2,
            Denied = 3,
            Unknown = 4,
            Roaming = 5
        }
        public ModemStatus FailedModemStatus { get; } = new ModemStatus
        {
            IsSuccess = false,
            State = "Not initialized",
            Manufacturer = "None",
            ModelName = "",
            SerialNumber = "SN xyz",
            SignalQuality = new SignalQuality { dBmW = 0, Percent = "0.0", IsValid = false },
            Imsi = string.Empty,
            Imei = string.Empty,
            OperatorName = ""
        };

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logService">Logging</param>
        /// <param name="configurationService">configuration</param>
        public Modem(ILogger logService, IManagerConfigurationService<ConnectConfiguration> configurationService)
        {
            logger = logService;
            var configuration = configurationService.LoadConfiguration();
            SetPort(configuration.PortName, configuration.BaudRate);
        }


        public Modem(ILogger logService, ConnectConfiguration configurationService)
        {
            logger = logService;
            var configuration = configurationService;
            SetPort(configuration.PortName, configuration.BaudRate);
        }

        /// <summary>
        /// Уставновка настроек порта
        /// </summary>
        public void SetPort(string portName, int baudeRate)
        {
            logger.Information(string.Format("Modem:SetPort Old = Port: {0}, Baud: {1}\nNew = Port: {2}, Baud: {3}",
                _portName, _baudeRate, portName, baudeRate));
            _portName = portName;
            _baudeRate = baudeRate;
        }

        /// <summary>
        /// Применение конфигурации 
        /// </summary>
        /// <param name="configuration">конфигурация</param>
        /// <returns></returns>
        public bool ApplyConfiguration(ConnectConfiguration configuration)
        {
            SetPort(configuration.PortName, configuration.BaudRate);
            return true;
        }

        /// <summary>
        /// Отправка АТ коммандыы  
        /// </summary>
        /// <param name="port">порт подлкючения устройства</param>
        /// <param name="command">Комманда</param>
        public string SendATCommand(SerialPort port, string command)
        {
            logger.Information(string.Format("Write: {0}", command));
            var buffer = new StringBuilder();
            var response = string.Empty;
            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }

                var data = Encoding.ASCII.GetBytes($"{command}\r\n");
                port.Write(data, 0, data.Length);

                Thread.Sleep(300);

                if (port.BytesToRead > 0)
                {
                    while (port.BytesToRead != 0) buffer.Append(port.ReadExisting());
                    response = buffer.ToString();
                    var readData = $"{ response.Replace('\r', '|').Replace('\n', '|')}";
                    logger.Information(string.Format($"Read: {readData}"));
                }
                else
                {
                    logger.Information($"Port {port.PortName} buffer is empty");
                }
            }
            catch (IOException exception)
            {
                logger.Error(exception, $"Modem.SendATCommand Exception: {exception.Message}");
                throw exception;
            }
            catch (Exception exception)
            {
                // TODO: debug logger??? для неизвестной ошибки пусть  падает в лог ее стектрейс (для продуктовой эксплуатации  скорее всего неприменимо) 
                logger.Error(exception, $"Modem.SendATCommand Exception: {exception.Message}{Environment.NewLine}{exception.StackTrace}");
                throw exception; // TODO: уточнить поведение на тесте 
            }
            finally
            {
                if (port.IsOpen)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    port.Close();
                }
            }
            return response;
        }

        public SerialPort ConfigurePort()
        {
            var result = new SerialPort(_portName, _baudeRate, Parity.None, 8, StopBits.One)
            {
                RtsEnable = true,
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII,
                WriteTimeout = 400,
                ReadTimeout = 400
            };
            return result;
        }


        private const int RetryCount = 3;
        private const int TimeOutRetry = 500;

        /// <summary>
        /// Политика переспроса при выполнении AT-команды
        /// если результат пустая строка или возникла ошибка доступа к порту,  или какая-то другая ошибка
        /// </summary>
        private readonly RetryPolicy<string> retry = Policy
            .HandleResult(string.Empty)
            .Or<IOException>()
            .Or<Exception>()
            .WaitAndRetry(RetryCount, attempt => TimeSpan.FromMilliseconds(TimeOutRetry));

        public ModemStatus GetModemStatus()
        {
            var result = FailedModemStatus.Clone();
            var description = new List<string>();
            var response = string.Empty;
            MatchCollection matches;
            using (var port = ConfigurePort())
            {
                try
                {
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "ATZ");
                    });
                    result.IsSuccess = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count > 0;
                    description.Add(result.IsSuccess ? "Готов" : "Не готов");
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT");
                    });
                    result.IsSuccess = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count > 0;
                    description.Add(result.IsSuccess ? "ЭХО" : "Нет ответа ЭХО");
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT+CIMI");
                    });
                    matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        result.Imsi = matches[0].Value;
                    }
                    result.IsSuccess = !string.IsNullOrEmpty(result.Imsi);
                    description.Add(!string.IsNullOrEmpty(result.Imsi) ? "Есть IMSI" : "Нет IMSI");
                    //оператор
                    var vOperator = GprsProviderEx.ParseSimIMSI(result.Imsi);//Преобразует оператор+IMSI в значение оператора
                    result.OperatorName = GprsProviderEx.ProviderName(vOperator);
                    result.IsSuccess &= vOperator != GprsProvider.Undefined;
                    description.Add(vOperator != GprsProvider.Undefined ? "Есть оператор" : "Нет оператора");
                    logger.Information(string.Format("Operator: {0}", vOperator));
                    //Model
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT+GMM");
                    });
                    matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if ((matches.Count >= 2) && ("OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.ModelName = matches[matches.Count - 2].Value.Trim();
                    }
                    //Manufacturer
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT+GMI");
                    });
                    matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Manufacturer = matches[matches.Count - 2].Value.Trim();
                    }
                    var vModel = result.AsModel();//преобразует производитель+модель в модель
                    result.IsSuccess &= vModel != ModelModem.Unknown; //!string.IsNullOrEmpty(vModemStatus.ModelName);
                    description.Add(vModel != ModelModem.Unknown ? "Detected" : "Unknown");
                    logger.Information(string.Format("Manufacturer: {0}", vModel));

                    //состояние сети
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT+CREG?");
                    });

                    var vNetStatus = -1;
                    var vNetStatusDescr = "неизвестно";

                    var vAnswPos = response.IndexOf("+CREG:");
                    if (vAnswPos >= 0)
                    {
                        vAnswPos += "+CREG: 0,".Length;
                        if (int.TryParse(response.Substring(vAnswPos, 1), out vNetStatus))
                            vNetStatusDescr = vNetStatus.ToNetworkStatus();
                    }
                    result.IsSuccess &= vNetStatus == 1;
                    description.Add("Сеть: " + vNetStatusDescr);
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT+CGSN");
                    });

                    matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                        // Imei is serial number
                        result.Imei = result.SerialNumber = matches[0].Value;
                    // AT+CGMR  Request revision identification of software status
                    response = retry.Execute(() =>
                    {
                        return SendATCommand(port, "AT+CGMR");
                    });

                    matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if ((matches.Count >= 2) && ("OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase)))
                        result.RevisionId = matches[matches.Count - 2].Value;
                    var quality = GetSignalQuality(port);
                    result.SignalQuality = quality;
                }
                catch (IOException ex)
                {
                    logger.Information(string.Format("Modem.GetModemStatus IOException: {0}", ex.Message));
                    result.State = string.Format("Ошибка: {0}", ex.Message);
                }
                catch (Exception ex)
                {
                    logger.Information(string.Format("Modem.GetModemStatus error: {0}", ex.Message));
                    result.State = string.Format("Ошибка: {0}", ex.Message);
                }
                finally
                {
                    logger.Information("Close");
                    result.State = string.Join(", ", description);
                }
            }
            return result;
        }


        /// <summary>
        /// проверка качества связи
        /// </summary>
        private SignalQuality GetSignalQuality(SerialPort serial)
        {
            var result = new SignalQuality()
            {
                dBmW = 0,
                IsValid = false,
                Percent = string.Empty
            };
            var response = retry.Execute(() =>
            {
                return SendATCommand(serial, "AT+CSQ");
            });
            if (string.IsNullOrEmpty(response)) return result;
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
            {
                matches = Regex.Matches(matches[matches.Count - 2].Value, @"\d+", RegexOptions.None);
                if (matches.Count > 0)
                {
                    var r = int.Parse(matches[0].Value);
                    if (r > 32)
                    {
                        result.dBmW = 0;
                        result.Percent = "0.0";
                        result.IsValid = false;
                    }

                    else if (r == 99) return result;

                    else
                    {
                        result.dBmW = -113 + (r << 1);
                        result.Percent = (r * 100.0 / 31.0).ToString("F2");
                        result.IsValid = true;
                    }
                }
            }
            return result;
        }
    }
}
