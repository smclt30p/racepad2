using Windows.Devices.Geolocation;

namespace Racepad2.Geo.Navigation.Core
{
    class Corner
    {
        public CornerType CornerType { get; set; }
        public BasicGeoposition Position { get; set; }
        public double Angle { get; set; }
        public double EntranceBearing { get; set; }
        public double ExitEaring { get; set; }

        public override string ToString() {
            return CornerType.ToString() + " - Lat: " + Position.Latitude +
                " - Long: " + Position.Longitude + " - Angle: " + Angle;
        }

    }
}
