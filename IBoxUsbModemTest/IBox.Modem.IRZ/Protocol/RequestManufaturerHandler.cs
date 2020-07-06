using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    public sealed class RequestManufaturerHandler : AbstractModemCommandHandler
    {
        private const string ATGMI = "AT+GMI";
        private const string DETECTED = "Detected";
        private const string UNKNOWN = "Unknown";
        private const string OK = "OK";

        public override ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            if (param == ATGMI)
            {
                var result = request.Response;
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
                            result.State = exception.Message;
                            request.Response.IsSuccess = false;
                            request.Description.Add($"Error send {ATGMI} - {exception.Message}");
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
                    var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if (matches.Count >= 2 &&
                        OK.Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
                    {
                        request.Response.Manufacturer = matches[matches.Count - 2].Value.Trim();
                        var model = result.AsModel();
                        request.Response.ModelName = model.ShortName();
                        result.IsSuccess = model != ModelModem.Unknown;
                        var desc = model != ModelModem.Unknown ? DETECTED : UNKNOWN;
                        request.Description.Add($"{desc} {request.Response.ModelName}");
                    }
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