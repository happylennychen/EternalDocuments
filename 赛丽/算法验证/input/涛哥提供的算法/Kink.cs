using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTNCommon
{
    public class Kink
    {
        public double CalKink(List<double> xList, List<double> yList)
        {
            double kink = -9999;
            if (xList.Count != yList.Count)
                return kink;
            try
            {
                List<double> yListD1 = new List<double>();
                List<double> xListD1 = new List<double>();
                for (int i = 1; i < xList.Count - 1; i++)
                {
                    xListD1.Add(xList[i]);
                    double dv1 = (yList[i + 1] - yList[i - 1]) / (yList[i + 1] - yList[i - 1]);
                    yListD1.Add(dv1);
                }
                double[] array = Fit.Polynomial(xListD1.ToArray(), yListD1.ToArray(), 4);
                List<double> kinks = new List<double>();
                for (int i = 0; i < xListD1.Count; i++)
                {
                    try
                    {
                        double temp = 0;
                        for (int j = 0; j < array.Length; j++)
                        {
                            temp += array[j] * Math.Pow(xListD1[i], j);
                        }
                        if (temp != 0)
                        {
                            double kinkTemp = Math.Abs((yListD1[i] - temp) / temp * 100);
                            kinks.Add(kinkTemp);
                        }
                        else
                        {

                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                kink = kinks.Max();
                int index = kinks.IndexOf(kink);
                double cur = xListD1[index];
            }
            catch
            {
                kink = -9999;
            }
            return kink;
            
        }
    }
}
