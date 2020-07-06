﻿using System;
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

        public override ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            var result = request.Response;
            if (param == ATCIMI)
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
                            request.Response.IsSuccess = false;
                            request.Description.Add($"Error send {ATCIMI} - {exception.Message}");
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
                    if (matches.Count > 0)
                    {
                        result.Imsi = matches[0].Value;
                        //оператор
                        var @operator =
                            GprsProviderEx.ParseSimIMSI(result.Imsi); //Преобразует оператор+IMSI в значение оператора
                        result.OperatorName = @operator.ProviderName();
                        result.IsSuccess = @operator != GprsProvider.Undefined;
                        var desc = @operator != GprsProvider.Undefined ? ISOPERATOR : NOTOPERATOR;
                        request.Description.Add(desc);
                    }
                    result.IsSuccess = !string.IsNullOrEmpty(result.Imsi);
                    var description = !string.IsNullOrEmpty(result.Imsi) ? "Есть IMSI" : "Нет IMSI";
                    request.Description.Add(description);
                    //result.State = string.Join(",", result.State, description);
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