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
                    result.IsSuccess = Regex.Matches(e, $"{param}", RegexOptions.IgnoreCase).Count > 0;
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


    public sealed class ImsiHandler: AbstractHandle
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
                helper.OnDataReceived += (sender, e) =>
                {
                    result.IsSuccess = Regex.Matches(e, $"{param}", RegexOptions.IgnoreCase).Count > 0;
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
}
