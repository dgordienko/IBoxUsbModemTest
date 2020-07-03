using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    /// Команда Echo (AT)
    /// </summary>
    public sealed class RequestEchoHandler : AbstractModemCommandHandle
    {
        private const string NOECHO = @"Нет ответа ЭХО";
        private const string ECHO = @"ЭХО";
        private const string AT = @"AT";
        private const string OK = @"OK";

        public override ModemRequestContext Handel(ModemRequestContext request, string @param)
        {
            var result = request.Response;
            if (param == AT)
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
                    result.IsSuccess = Regex.Matches(response, $"{OK}", RegexOptions.IgnoreCase).Count > 0;
                    request.Description.Add(result.IsSuccess ? ECHO : NOECHO);
                    //result.State = string.Join(",", result.IsSuccess ? ECHO : NOECHO);
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
            return base.Handel(request, @param);
        }
    }
}
