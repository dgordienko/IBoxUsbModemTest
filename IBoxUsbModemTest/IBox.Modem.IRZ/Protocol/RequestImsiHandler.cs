using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     Imsi handler.(AT+CIMI)
    /// </summary>
    public sealed class RequestImsiHandler : AbstractModemCommandHandler
    {
        private const string ATCIMI = "AT+CIMI";
        private const string ISOPERATOR = "Есть оператор";
        private const string NOTOPERATOR = "Нет оператора";
        private const int Sleep = 500;

        protected override string ModemCommand => ATCIMI;

        protected override void OnDataReceived(object sender, string response)
        {
            var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                modemStatus.Imsi = matches[0].Value;
                //оператор
                var @operator =
                    GprsProviderEx.ParseSimIMSI(modemStatus.Imsi); //Преобразует оператор+IMSI в значение оператора
                modemStatus.OperatorName = @operator.ProviderName();
                modemStatus.IsSuccess = @operator != GprsProvider.Undefined;
                var desc = @operator != GprsProvider.Undefined ? ISOPERATOR : NOTOPERATOR;
                //request.Description.Add(desc);
            }
            modemStatus.IsSuccess = !string.IsNullOrEmpty(modemStatus.Imsi);
            var description = !string.IsNullOrEmpty(modemStatus.Imsi) ? "Есть IMSI" : "Нет IMSI";
            //request.Description.Add(description);
            received = false;
            (sender as ModemManager)?.Close();
        }

    }
}