namespace IBox.Modem.IRZ.Core
{
    public interface IModemRequestHandler
    {
        /// <summary>
        ///     Sets the next.
        /// </summary>
        /// <returns>The net.</returns>
        /// <param name="handler">Handler.</param>
        IModemRequestHandler SetNext(IModemRequestHandler handler);

        /// <summary>
        ///     Handle the specified request and param.
        /// </summary>
        /// <returns>The handel.</returns>
        /// <param name="request">Request.</param>
        /// <param name="param">Parameter.</param>
        ModemRequestContext Handle(ModemRequestContext request, string param);
    }
}