using System;
using System.Diagnostics;
using System.Threading;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    public sealed class RequestNetworkStatusHandler : AbstractModemCommandHandler
    {
        private const string Command = @"AT+CREG?";
        private const string Default = "неизвестно";

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
                            received = false;
                            request.Description.Add($"Error send {Command} - {exception.Message}");
                            request.Response.IsSuccess = false;
#if DEBUG
                            throw;
#endif
                        }
                    }
                };
                helper.OnStatusChanged += (sender, response) => { Debug.WriteLine(response); };
                helper.OnDataReceived += (sender, response) =>
                {
                    var netstatus = -1;
                    var netStatusDescr = Default;
                    var pos = response.IndexOf("+CREG:", StringComparison.Ordinal);
                    if (pos >= 0)
                    {
                        pos += "+CREG: 0,".Length;
                        if (int.TryParse(response.Substring(pos, 1), out netstatus))
                            netStatusDescr = netstatus.ToNetworkStatus();
                    }

                    result.IsSuccess = netstatus == 1;
                    request.Description.Add($"Сеть:{netStatusDescr}");
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