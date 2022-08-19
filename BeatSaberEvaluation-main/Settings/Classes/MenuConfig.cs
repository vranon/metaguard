namespace SaberTailor.Settings.Classes
{
    public enum PositionUnit { cm, mm }
    public enum PositionDisplayUnit { cm, inches, miles, nauticalmiles }

    public class MenuConfig
    {
        public int SaberPosIncrement;
        public int SaberPosIncValue;
        public int SaberRotIncrement;

        public PositionUnit SaberPosIncUnit;
        public PositionDisplayUnit SaberPosDisplayUnit;
    }
}
