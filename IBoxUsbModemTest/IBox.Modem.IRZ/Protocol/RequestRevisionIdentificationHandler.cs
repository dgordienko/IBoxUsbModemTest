using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     AT+CGMR  Request revision identification of software status
    /// </summary>
    public sealed class RequestRevisionIdentificationHandler : AbstractModemCommandHandler
    {
        private const string Command = "AT+CGMR";
        private const string Ok = "OK";

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
                    if (matches.Count >= 2 && Ok.Equals(matches[matches.Count - 1].Value,
                        StringComparison.OrdinalIgnoreCase))
                        result.RevisionId = matches[matches.Count - 2].Value;
                    result.IsSuccess = true;
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