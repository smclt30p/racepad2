using Racepad2.Geo.Navigation.Core;
using System.Threading.Tasks;

namespace Racepad2.Geo.Navigation {
    abstract class RouteParser
    {
        protected CornerType GetCornerType(double angle) {

            if (angle >= 0 && angle <= 45) return CornerType.RIGHT_HAIRPIN;
            if (angle >= 45 && angle <= 90) return CornerType.RIGHT_TWO;
            if (angle >= 90 && angle <= 145) return CornerType.RIGHT_THREE;

            if (angle >= 145 && angle <= 215) return CornerType.STRAIGHT;

            if (angle >= 215 && angle <= 270) return CornerType.LEFT_THREE;
            if (angle >= 270 && angle <= 315) return CornerType.LEFT_TWO;
            if (angle >= 315 && angle <= 360) return CornerType.LEFT_HAIRPIN;


            return CornerType.UNKNOWN;

        }

        public abstract Task<DriveRoute> ParseAsync();
    }
}
