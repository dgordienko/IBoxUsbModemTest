using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    public sealed class RequestModelHandler : AbstractModemCommandHandler
    {
        private const string ATGMM = "AT+GMM";
        private const string OK = "OK";

        public override ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            var result = request.Response;
            if (param == ATGMM)
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
                            result.State = exception.Message;
                            request.Description.Add($"Error send {ATGMM} - {exception.Message}");
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
                    var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                    if (matches.Count >= 2 && OK.Equals(matches[matches.Count - 1].Value,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        result.IsSuccess = true;
                        result.ModelName = matches[matches.Count - 2].Value.Trim();
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