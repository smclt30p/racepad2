using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Racepad2.UI.StatusTiles {

    class TileContainer : UserControl {

        private BasicStatusTile _innerTile;

        public string Tile {

            get {
                return _innerTile.GetType().ToString();
            } set {

                Type requestedType = Type.GetType(String.Format("Racepad2.UI.StatusTiles.{0}", value));

                if (requestedType == null) {
                    throw new Exception("Requested tile not found!");
                }

                _innerTile = (BasicStatusTile) Activator.CreateInstance(requestedType);

                if (_innerTile == null) {
                    throw new Exception("Tile instance is null!");
                }

                base.Content = _innerTile;
            }
        }
        public object Value {
            get {

                if (base.Content != null && base.Content is BasicStatusTile) {

                    BasicStatusTile tile = (BasicStatusTile)base.Content;

                    return tile.Value;

                } else return null;

            }
            set {

                if (base.Content != null && base.Content is BasicStatusTile) {
                    BasicStatusTile tile = (BasicStatusTile)base.Content;
                    tile.Value = value;
                }

            }
        }

    }
}
