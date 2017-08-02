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

        /// <summary>
        /// Debug information for the corner
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return CornerType.ToString() + " - Lat: " + Position.Latitude +
                " - Long: " + Position.Longitude + " - Angle: " + Angle;
        }

        /// <summary>
        /// Get the visual corner description
        /// </summary>
        /// <param name="corner">The corner </param>
        /// <returns>Textual representation of the corner</returns>
        public static string GetDescriptionForCorner(Corner corner) {
            switch (corner.CornerType) {
                case CornerType.LEFT_HAIRPIN:
                    return "hairpin left";
                case CornerType.LEFT_THREE:
                    return "left";
                case CornerType.LEFT_TWO:
                    return "square left";
                case CornerType.RIGHT_HAIRPIN:
                    return "hairpin right";
                case CornerType.RIGHT_THREE:
                    return "right";
                case CornerType.RIGHT_TWO:
                    return "square right";
            }
            return "Unknown";
        }

    }
}
