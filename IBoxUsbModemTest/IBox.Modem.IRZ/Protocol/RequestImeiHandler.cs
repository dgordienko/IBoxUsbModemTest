using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     request IMEI (Imei is serial number AT+CGSN)
    /// </summary>
    public sealed class RequestImeiHandler : AbstractModemCommandHandler
    {
        private const string Command = "AT+CGSN";

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public override ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            var result = request.Response;
            if (param == Command)
            {
                var helper = ModemManager.Instance;
                var received = false;
                helper.OnSerialPortOpened += (sender, response) =>
                {
                    if (response)
                    {
                        try
                        {
                            received = true;
                            helper.SendString(param);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            request.Description.Add($"{Command} - {exception.Message}");
                            result.State = exception.Message;
                            request.Response.IsSuccess = false;
                            received = false;

#if DEBUG
                            throw;
#endif
                        }
                    }
                };
                helper.OnStatusChanged += (sender, response) => { Debug.WriteLine(response); };
                helper.OnDataReceived += (sender, response) =>
                {
                    var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
                    result.Imei = result.SerialNumber = matches[0].Value;
                    result.IsSuccess = result.SerialNumber == result.Imei;
                    received = false;
                    (sender as ModemManager)?.Close();
                };

                if (!helper.IsOpen) helper.Open(request.Connection.PortName);
                while (received) Thread.Sleep(300);
                return request;
            }

            return base.Handle(request, param);
        }
    }
}