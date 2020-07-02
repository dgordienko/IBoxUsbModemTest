using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    /// Imsi handler.(AT+CIMI)
    /// </summary>
    public sealed class RequestImsiHandler : AbstractModemCommandHandle
    {
        private const string ATCIMI = "AT+CIMI";
        private const string ISOPERATOR = "Есть оператор";
        private const string NOTOPERATOR = "Нет оператора";

        public override ModemRequestContext Handel(ModemRequestContext request, string @param)
        {
            var result = request.Response;
            if (param == ATCIMI)
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
                    var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        result.Imsi = matches[0].Value;
                        //оператор
                        var @operator = GprsProviderEx.ParseSimIMSI(result.Imsi);//Преобразует оператор+IMSI в значение оператора
                        result.OperatorName = GprsProviderEx.ProviderName(@operator);
                        result.IsSuccess &= @operator != GprsProvider.Undefined;
                        var desc = @operator != GprsProvider.Undefined ? ISOPERATOR : NOTOPERATOR;
                        result.State = string.Join(",", result.State, desc);
                    }
                    result.IsSuccess = !string.IsNullOrEmpty(result.Imsi);
                    var description = !string.IsNullOrEmpty(result.Imsi) ? "Есть IMSI" : "Нет IMSI";
                    result.State = string.Join(",", result.State, description);
                    recived = false;
                    (sender as ModemManager)?.Close();
                };
                if (!helper.IsOpen)
                {
                    helper.Open(request.Connection.PortName);
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
