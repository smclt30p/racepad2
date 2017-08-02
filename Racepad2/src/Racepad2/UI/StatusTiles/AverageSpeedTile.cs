using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racepad2.UI.StatusTiles {
    class AverageSpeedTile : BasicStatusTile {

        public AverageSpeedTile() : base() {
            BottomText = "Avg. Speed (10s)";
        }

        public override object Value {
            get {
                return base.Value;
            }
            set {

                double avgSpeed = Double.Parse(value.ToString()) * 3.6;
                base.Value = Convert.ToString(Math.Round(avgSpeed, 2));

            }
        }
    }
}
