using IBoxUsbModemUnitTest.Modem;


namespace IBoxUsbModemUnitTest
{
    public interface IHandler
    {
        IHandler SetNet(IHandler handler);
        ModemRequest Handel(ModemRequest request, string @param);
    }
}
