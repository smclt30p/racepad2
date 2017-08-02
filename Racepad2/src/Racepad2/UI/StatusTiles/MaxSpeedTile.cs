using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racepad2.UI.StatusTiles {
    class MaxSpeedTile : BasicStatusTile {

        public MaxSpeedTile() {
            BottomText = "Max. Speed (km/h)";
        }

        public override object Value {
            get {
                return base.Value;
            } set {

                double maxSpeed = Double.Parse(value.ToString()) * 3.6;
                base.Value = Convert.ToString(Math.Round(maxSpeed, 2));

            }
        }

    }
}
