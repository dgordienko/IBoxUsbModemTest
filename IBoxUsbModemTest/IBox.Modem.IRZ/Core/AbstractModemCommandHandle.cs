namespace IBox.Modem.IRZ.Core
{
    public abstract class AbstractModemCommandHandle : IModemRequestHandler
    {
        private IModemRequestHandler _nextHandler;
        public virtual ModemRequestContext Handel(ModemRequestContext request, string @param)
        {

            return _nextHandler?.Handel(request, param);
        }

        public IModemRequestHandler SetNext(IModemRequestHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }
    }
}
