using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBoxUsbModemUnitTest.Modem;


namespace IBoxUsbModemUnitTest
{
    /// <summary>
    /// Команда AT
    /// </summary>
    public sealed class EchoHandler : AbstractHandle
    {
        public override ModemRequest Handel(ModemRequest request, string @param)
        {
            var result = request.Response;
            if (param == "AT")
            {
                var helper = ModemSerialPort.Instance;
                var recived = false;
                helper.OnSerialPortOpened += (sender, e) =>
                {
                    if (e)
                    {
                        try
                        {
                            helper.SendString(param);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            result.State = exception.Message;
                        }
                        recived = true;
                    }
                };
                helper.OnStatusChanged += (sender, e) =>
                {
                    Debug.WriteLine(e);
                };
                helper.OnDataReceived += (sender, e) =>
                {
                    result.IsSuccess = Regex.Matches(e, $"{param}", RegexOptions.IgnoreCase).Count > 0;
                    result.State = string.Join(",", result.IsSuccess ? "ЭХО" : "Нет ответа ЭХО");
                    recived = false;
                    (sender as ModemSerialPort)?.Close();
                };
                if (!helper.IsOpen)
                {
                    helper.Open(portname: request.Connection.PortName);
                }
                while (recived)
                {
                    Thread.Sleep(300);
                }
                return request;
            }
            return base.Handel(request, "AT");
        }
    }

    /// <summary>
    /// Команда ATZ
    /// </summary>
    public sealed class ResetHandler : AbstractHandle
    {
        public override ModemRequest Handel(ModemRequest request, string @param)
        {
            var result = request.Response;
            if (param == "ATZ")
            {
                var helper = ModemSerialPort.Instance;
                var recived = false;
                helper.OnSerialPortOpened += (sender, e) =>
                {
                    if (e)
                    {
                        try
                        {
                            helper.SendString(param);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            result.State = exception.Message;
                        }
                        recived = true;
                    }
                };
                helper.OnStatusChanged += (sender, e) =>
                {
                    Debug.WriteLine(e);
                };
                helper.OnDataReceived += (sender, e) =>
                {
                    var @params = "OK";
                    result.IsSuccess = Regex.Matches(e, $"{@params}", RegexOptions.IgnoreCase).Count > 0;
                    result.State = string.Join(",", result.IsSuccess ? "Готов" : "Не готов");
                    recived = false;
                    (sender as ModemSerialPort)?.Close();
                };
                if (!helper.IsOpen)
                {
                    helper.Open(portname: request.Connection.PortName);
                }
                while (recived)
                {
                    Thread.Sleep(300);
                }
                return request;
            }
            return base.Handel(request, "AT");
        }
    }

    /// <summary>
    /// Imsi handler.(AT+CIMI)
    /// </summary>
    public sealed class ImsiHandler : AbstractHandle
    {
        public override ModemRequest Handel(ModemRequest request, string @param)
        {
            var result = request.Response;
            if (param == "AT+CIMI")
            {
                var helper = ModemSerialPort.Instance;
                var recived = false;
                helper.OnSerialPortOpened += (sender, e) =>
                {
                    if (e)
                    {
                        try
                        {
                            helper.SendString(param);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            result.State = exception.Message;
                        }
                        recived = true;
                    }
                };
                helper.OnStatusChanged += (sender, e) =>
                {
                    Debug.WriteLine(e);
                };
                helper.OnDataReceived += (sender, response) =>
                {
                    var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        result.Imsi = matches[0].Value;
                        //оператор
                        var @operator = GprsProviderEx.ParseSimIMSI(result.Imsi);//Преобразует оператор+IMSI в значение оператора
                        result.OperatorName = GprsProviderEx.ProviderName(@operator);
                        result.IsSuccess &= @operator != GprsProvider.Undefined;
                        var desc = @operator != GprsProvider.Undefined ? "Есть оператор" : "Нет оператора";
                        result.State = string.Join(",", result.State, desc);
                    }
                    result.IsSuccess = !string.IsNullOrEmpty(result.Imsi);
                    var description = !string.IsNullOrEmpty(result.Imsi) ? "Есть IMSI" : "Нет IMSI";
                    result.State = string.Join(",", result.State, description);
                    recived = false;
                    (sender as ModemSerialPort)?.Close();
                };
                if (!helper.IsOpen)
                {
                    helper.Open(portname: request.Connection.PortName);
                }
                while (recived)
                {
                    Thread.Sleep(300);
                }
                return request;
            }
            return base.Handel(request, @param);
        }
    }


    public sealed class RequestModelHandler : AbstractHandle
    {
        public override ModemRequest Handel(ModemRequest request, string param)
        {
            var result = request.Response;
            if (param == "AT+GMM")
            {
                var helper = ModemSerialPort.Instance;
                var recived = false;
                helper.OnSerialPortOpened += (sender, e) =>
                {
                    if (e)
                    {
                        try
                        {
                            helper.SendString(param);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            result.State = exception.Message;
                        }
                        recived = true;
                    }
                };
                helper.OnStatusChanged += (sender, e) =>
                {
                    Debug.WriteLine(e);
                };
                helper.OnDataReceived += (sender, response) =>
                {

                    var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if ((matches.Count >= 2) && ("OK".Equals(matches[matches.Count - 1].Value,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        result.IsSuccess = true;
                        result.ModelName = matches[matches.Count - 2].Value.Trim();
                    }
                    recived = false;
                    (sender as ModemSerialPort)?.Close();
                };
                if (!helper.IsOpen)
                {
                    helper.Open(portname: request.Connection.PortName);
                }
                while (recived)
                {
                    Thread.Sleep(300);
                }
                return request;
            }
            return base.Handel(request, @param);
        }
    }

    public sealed class RequestManufaturerHandler : AbstractHandle
    {
        public override ModemRequest Handel(ModemRequest request, string param)
        {
            var result = request.Response;
            if (param == "AT+GMI")
            {
                var helper = ModemSerialPort.Instance;
                var recived = false;
                helper.OnSerialPortOpened += (sender, e) =>
                {
                    if (e)
                    {
                        try
                        {
                            helper.SendString(param);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            result.State = exception.Message;
                        }
                        recived = true;
                    }
                };
                helper.OnStatusChanged += (sender, e) =>
                {
                    Debug.WriteLine(e);
                };
                helper.OnDataReceived += (sender, response) =>
                {

                    var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Manufacturer = matches[matches.Count - 2].Value.Trim();
                        var model = result.AsModel();
                        result.IsSuccess &= model != ModelModem.Unknown;
                        var desc = model != ModelModem.Unknown ? "Detected" : "Unknown";
                        result.State = string.Join(",", result.State, desc);
                    }
                    recived = false;
                    (sender as ModemSerialPort)?.Close();
                };
                if (!helper.IsOpen)
                {
                    helper.Open(portname: request.Connection.PortName);
                }
                while (recived)
                {
                    Thread.Sleep(300);
                }
                return request;
            }
            return base.Handel(request, param);
        }
    }
}
