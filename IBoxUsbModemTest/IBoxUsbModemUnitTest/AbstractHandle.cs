using IBoxUsbModemUnitTest.Modem;


namespace IBoxUsbModemUnitTest
{
    public abstract class AbstractHandle : IHandler
    {
        private IHandler _nextHandler;
        public virtual ModemRequest Handel(ModemRequest request, string @param)
        {

            return _nextHandler != null ?
                _nextHandler.Handel(request, param) : null;
        }

        public IHandler SetNet(IHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }
    }
}
