using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     Команда ATZ
    /// </summary>
    public sealed class RequestResetHandler : AbstractModemCommandHandler
    {
        private const string ATZ = "ATZ";
        private const string OK = "OK";
        private const string REDY = "Готов";
        private const string NOTREDY = "Не готов";

        public override ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            var result = request.Response;
            if (param == ATZ)
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
                            request.Description.Add($"{ATZ} - {exception.Message}");
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
                    result.IsSuccess = Regex.Matches(response, $"{OK}", RegexOptions.IgnoreCase).Count > 0;
                    request.Description.Add(result.IsSuccess ? REDY : NOTREDY);
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