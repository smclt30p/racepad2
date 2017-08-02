using System;

namespace Racepad2.Geo.Navigation {
    class GPXException : Exception
    {
        public GPXException(string reason) : base(reason) {
            
        }
    }
}
