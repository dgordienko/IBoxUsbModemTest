using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{

    public sealed class RequestManufaturerHandler : AbstractModemCommandHandle
    {
        private const string ATGMI = "AT+GMI";

        public override ModemRequestContext Handel(ModemRequestContext request, string param)
        {
            var result = request.Response;
            if (param == ATGMI)
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
            return base.Handel(request, param);
        }
    }
}
