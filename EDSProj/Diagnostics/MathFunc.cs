using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{
    public class MathFunc { 
        public static double[] getLinear(SortedList<double,double> Otrez)
        {
            double sumXY = 0;
            double sumX2 = 0;
            double sumX = 0;
            double sumY = 0;

            for (int i = 0; i < Otrez.Count; i++)
            {
                double x = Otrez.Keys[i];
                double y = Otrez.Values[i];
                sumXY += x * y;
                sumX2 += x * x;
                sumX += x;
                sumY += y;
            }

            double n = Otrez.Count;
            double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - a * sumX) / n;

            double sumSign = 0;
            for (int i = 0; i < Otrez.Count; i++)
            {
                double x = Otrez.Keys[i];
                double y = Otrez.Values[i];
                double y1 = a * x + b;
                sumSign += (y1 - y) * (y1 - y);

            }
            return new double[] { a, b, sumSign };
        }
    
    }
}
