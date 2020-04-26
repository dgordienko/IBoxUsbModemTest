using System;

namespace IBoxUsbModemUnitTest.Modem
{
    public static class ModelModemEx
    {
        public static string ShortName(this ModelModem self)
        {
            switch (self)
            {
                //case ModelModem.None: return "none";
                case ModelModem.Unknown: return "Unknown";
                case ModelModem.SiemensMC35: return "MC35";
                case ModelModem.Wavecom: return "Wavecom";
                case ModelModem.PantechUM175: return "PantechUM175";
                case ModelModem.Cinterion: return "BGS2-E";//DEVSPACE-3943
                case ModelModem.Quectel: return "Quectel M95";
                case ModelModem.TU32: return "Huawei TU32";
            }
            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        public static string Description(this ModelModem self)
        {
            switch (self)
            {
                //case ModelModem.None: return "none";
                case ModelModem.Unknown: return "Unknown";
                case ModelModem.SiemensMC35: return "SiemensMC35"; //DEVSPACE-3943
                case ModelModem.Wavecom: return "Wavecom";
                case ModelModem.PantechUM175: return "Pantech UM175 (CDMA)";
                case ModelModem.Cinterion: return "BGS2-E";
                case ModelModem.Quectel: return "Quectel M95";
                case ModelModem.TU32: return "Huawei TU32";
            }
            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        public static ModelModem Parse(string val)
        {
            foreach (ModelModem e in Enum.GetValues(typeof(ModelModem)))
                if ((e.Description().Equals(val, StringComparison.OrdinalIgnoreCase)) ||
                    (e.ToString().Equals(val, StringComparison.OrdinalIgnoreCase)))
                    return e;
            throw new ArgumentException(string.Format("Unknown value '{0}'", val));
        }

        public static ModelModem ParseManufacturer(string atManufacturer)
        {
            if (string.IsNullOrEmpty(atManufacturer))
                return ModelModem.Unknown;

            if (atManufacturer.Equals("SIEMENS", StringComparison.OrdinalIgnoreCase) ||
                atManufacturer.Equals("Cinterion", StringComparison.OrdinalIgnoreCase)) //DEVSPACE-3943
                return ModelModem.SiemensMC35;
            if (atManufacturer.StartsWith("WAVECOM", StringComparison.OrdinalIgnoreCase)) // WAVECOM MODEM  MULTIBAND  900E  1800
                return ModelModem.Wavecom;
            if (atManufacturer.StartsWith("PANTECH", StringComparison.OrdinalIgnoreCase)) // PANTECH UM175      PANTECH UM175AL
                return ModelModem.PantechUM175;
            return ModelModem.Unknown;
        }

        public static ModelModem ParseModelName(string atModelName)
        {
            if (string.IsNullOrEmpty(atModelName))
                return ModelModem.Unknown;

            if (atModelName.Equals("MC35", StringComparison.OrdinalIgnoreCase) ||
                atModelName.Equals("MC35i", StringComparison.OrdinalIgnoreCase) ||
                atModelName.Equals("BGS2-E", StringComparison.OrdinalIgnoreCase)) //DEVSPACE-3943
                return ModelModem.SiemensMC35;
            if (atModelName.Equals("MULTIBAND  900E  1800", StringComparison.OrdinalIgnoreCase)) // WAVECOM MODEM  MULTIBAND  900E  1800
                return ModelModem.Wavecom;
            if (atModelName.StartsWith("UM175", StringComparison.OrdinalIgnoreCase)) // PANTECH UM175      PANTECH UM175AL
                return ModelModem.PantechUM175;

            return ModelModem.Unknown;
        }

    }
}
