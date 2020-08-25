using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
    public class SDPMDKReport
    {
        protected static Dictionary<string, EDSPointInfo> PointsRef;
        protected static SortedList<String, EDSPointInfo> AllPoints;
        public static void init(SortedList<String, EDSPointInfo> allPoints)
        {
            AllPoints = allPoints;
            PointsRef = new Dictionary<string, EDSPointInfo>();

            PointsRef.Add("DKMan", AllPoints["11VT_SPDG00D_1948-002.MCR@GRARM"]);//+?
            PointsRef.Add("DKAuto", AllPoints["11VT_SPDG00D_1948-021.MCR@GRARM"]);//+

            PointsRef.Add("DKDay", AllPoints["11VT_SPDG00A_1948-105.MCR@GRARM"]);//+
            PointsRef.Add("DKMonth", AllPoints["11VT_SPDG00A_1948-106.MCR@GRARM"]);//+
            PointsRef.Add("DKYear", AllPoints["11VT_SPDG00A_1948-107.MCR@GRARM"]);//+
            PointsRef.Add("DKStartDay", AllPoints["11VT_SPDG00A_1948-108.MCR@GRARM"]);//+
            PointsRef.Add("DKStartMonth", AllPoints["11VT_SPDG00A_1948-109.MCR@GRARM"]);//+
            PointsRef.Add("DKStartYear", AllPoints["11VT_SPDG00A_1948-110.MCR@GRARM"]);//+
            PointsRef.Add("DKEndDay", AllPoints["11VT_SPDG00A_1948-111.MCR@GRARM"]);//+
            PointsRef.Add("DKEndMonth", AllPoints["11VT_SPDG00A_1948-112.MCR@GRARM"]);//+
            PointsRef.Add("DKEndYear", AllPoints["11VT_SPDG00A_1948-113.MCR@GRARM"]);//+

            PointsRef.Add("DKHour", AllPoints["11VT_SPDG00A_1948-079.MCR@GRARM"]);//+
            PointsRef.Add("DKMin", AllPoints["11VT_SPDG00A_1948-080.MCR@GRARM"]);//+
            PointsRef.Add("DKStartHour", AllPoints["11VT_SPDG00A_1948-083.MCR@GRARM"]);//+
            PointsRef.Add("DKStartMin", AllPoints["11VT_SPDG00A_1948-084.MCR@GRARM"]);//+
            PointsRef.Add("DKEndHour", AllPoints["11VT_SPDG00A_1948-087.MCR@GRARM"]);//+
            PointsRef.Add("DKEndMin", AllPoints["11VT_SPDG00A_1948-089.MCR@GRARM"]);//+




        }
    }
}
