using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racepad2.UI.StatusTiles {
    class SpeedTile : BasicStatusTile {

        public SpeedTile() : base() {
            base.BottomText = "Speed (km/h)";
        }

        public override object Value {
            get {
                return base.Value.ToString();
            } set {

                double speed = Double.Parse(value.ToString());
                base.Value = Convert.ToString(Math.Round(speed * 3.6, 2));
            }
        }

    }
}
