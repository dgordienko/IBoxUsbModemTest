using System;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    public sealed class RequestNetworkStatusHandler : AbstractModemCommandHandler
    {
        private const string Command = @"AT+CREG?";
        private const string Default = "неизвестно";

        protected override string ModemCommand => Command;

        protected override void OnDataReceived(object sender, string response)
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

            modemStatus.IsSuccess = netstatus == 1;
            //request.Description.Add($"Сеть:{netStatusDescr}");
            received = false;
            (sender as ModemManager)?.Close();
        }
    }
}