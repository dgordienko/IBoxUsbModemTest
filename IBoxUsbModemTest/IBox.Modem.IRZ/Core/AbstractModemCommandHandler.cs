namespace IBox.Modem.IRZ.Core
{
    public abstract class AbstractModemCommandHandler : IModemRequestHandler
    {
        private IModemRequestHandler _nextHandler;

        public IModemRequestHandler SetNext(IModemRequestHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            return _nextHandler?.Handle(request, param);
        }
    }
}