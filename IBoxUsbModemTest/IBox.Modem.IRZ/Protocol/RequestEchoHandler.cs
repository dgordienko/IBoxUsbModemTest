﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    /// Команда Echo (AT)
    /// </summary>
    public sealed class RequestEchoHandler : AbstractModemCommandHandler
    {
        private const string NOECHO = @"Нет ответа ЭХО";
        private const string ECHO = @"ЭХО";
        private const string AT = @"AT";
        private const string OK = @"OK";

        /// <summary>
        ///     Send command AT
        /// </summary>
        /// <param name="request"></param>
        /// <param name="param">AT</param>
        /// <returns cref="ModemRequestContext"></returns>
        public override ModemRequestContext
            Handle(ModemRequestContext request, string param)
        {
            if (param == AT)
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
                            request.Description.Add($"{AT} - {exception.Message}");
                            received = false;
                            request.Response.IsSuccess = false;
                            Debug.WriteLine(exception.Message);
#if DEBUG
                            throw;
#endif
                        }
                    }
                };
                helper.OnStatusChanged += (sender, response) => { Debug.WriteLine(response); };
                helper.OnDataReceived += (sender, response) =>
                {
                    result.IsSuccess = Regex.Matches(response, $"{OK}",
                         RegexOptions.IgnoreCase).Count > 0;
                    request.Description.Add(result.IsSuccess ? ECHO : NOECHO);
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