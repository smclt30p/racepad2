using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racepad2.UI.StatusTiles {
    class DistanceTile : BasicStatusTile {

        public DistanceTile() {
            BottomText = "Distance (km)";
        }

        public override object Value {
            get {
                return base.Value.ToString();
            } set {
                double distanceMeters = Double.Parse(value.ToString());
                base.Value = Convert.ToString(Math.Round((distanceMeters / 1000), 2));
            }
        }

    }
}
