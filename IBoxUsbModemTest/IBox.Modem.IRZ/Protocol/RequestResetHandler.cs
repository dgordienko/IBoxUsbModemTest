using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{

    /// <summary>
    /// Команда ATZ
    /// </summary>
    public sealed class RequestResetHandler : AbstractModemCommandHandle
    {
        private const string ATZ = "ATZ";

        public override ModemRequestContext Handel(ModemRequestContext request, string @param)
        {
            var result = request.Response;
            if (param == ATZ)
            {
                var helper = ModemManager.Instance;
                var recived = false;
                helper.OnSerialPortOpened += (sender, response) =>
                {
                    if (response)
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
                helper.OnStatusChanged += (sender, response) =>
                {
                    Debug.WriteLine(response);
                };
                helper.OnDataReceived += (sender, response) =>
                {
                    var @params = "OK";
                    result.IsSuccess = Regex.Matches(response, $"{@params}", RegexOptions.IgnoreCase).Count > 0;
                    result.State = string.Join(",", result.State, result.IsSuccess ? "Готов" : "Не готов");
                    recived = false;
                    (sender as ModemManager)?.Close();
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
