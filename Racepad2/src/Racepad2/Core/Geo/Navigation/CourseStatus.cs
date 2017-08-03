using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racepad2.Geo.Navigation {
    enum CourseStatus {
        COURSE_STARTED,
        COURSE_PAUSED,
        //COURSE_PAUSED_FINISHED,
        // COURSE_PAUSED_NOT_STARTED,
        COURSE_LAST_STRAIGHT,
        COURSE_FINISHED,
        COURSE_NOT_STARTED,
        COURSE_IN_PROGRESS,
        COURSE_OFF_COURSE
    }
}
