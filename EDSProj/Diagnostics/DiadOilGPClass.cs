using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDSProj.EDS;

namespace EDSProj.Diagnostics
{
    public class DiadOilGPClass : DiadOilClass
    {
        public override async Task<bool> ReadData(string GG)
        {
            report = new EDSReport(DateStart.AddSeconds(0), DateEnd, EDSReportPeriod.minute);

            recTUp = report.addRequestField(AllPoints[GG + "VT_BG01RI-25.MCR@GRARM"], EDSReportFunction.val);
            recTDn = report.addRequestField(AllPoints[GG + "VT_BG01RI-26.MCR@GRARM"], EDSReportFunction.val);
            recLvl1 = report.addRequestField(AllPoints[GG + "VT_BG01AI-01.MCR@GRARM"], EDSReportFunction.val);
            recLvl2 = report.addRequestField(AllPoints[GG + "VT_BG01AI-02.MCR@GRARM"], EDSReportFunction.val);
            recF = report.addRequestField(AllPoints[GG + "VT_GC01A-16.MCR@GRARM"], EDSReportFunction.val);

            bool ok = await report.ReadData();
            if (!ok)
                return false;


            return ok;

        }




    }
}
