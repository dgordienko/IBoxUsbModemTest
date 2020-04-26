namespace IBoxUsbModemUnitTest.Modem
{
    public static class ModemSettingsExtentions
    {
        public static string ToNetworkStatus(this int value)
        {
            var result = string.Empty;

            switch (value)
            {
                case 0:
                    result = "не зарегистрирован"; break;
                case 1:
                    result = "зарегистрирован"; break;
                case 2:
                    result = "поиск"; break;
                case 3:
                    result = "доступ запрещен"; break;
                case 4:
                    result = "неизвестно"; break;
                case 5:
                    result = "в роуминге"; break;
                case 6:
                case 7:
                    result = "только SMS"; break;
                case 8:
                    result = "только экстренные службы"; break;
                case 9:
                case 10:
                    result = "CSFB not preferred"; break;
                default:
                    result = "неопознанное состояние"; break;
            }
            return result;
        }
    }
}
