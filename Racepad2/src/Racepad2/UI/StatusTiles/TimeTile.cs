using System;

namespace Racepad2.UI.StatusTiles {
    class TimeTile : BasicStatusTile {

        public TimeTile() {
            BottomText = "Duration";
        }

        public override object Value {
            get {
                return null;
            } set {

                if (value is int) {
                    base.Value = value.ToString();
                    TimeSpan time = TimeSpan.FromSeconds((int)value);
                    base.Value = time.ToString(@"hh\:mm\:ss");
                }

            }
        }

    }
}
