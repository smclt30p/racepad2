using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racepad2.Core {

    class WalkingList<T> : List<T> {

        private int THRESHOLD = 10;

        public WalkingList() { }
        public WalkingList(int threshold) {
            this.THRESHOLD = threshold;
        }

        public new void Add(T element) {
            if (base.Count > THRESHOLD) {
                base.RemoveAt(0);
            }
            base.Add(element);
        }
    }
}
