using System;
using System.Collections.Generic;
using System.Linq;

namespace TCC_KM
{
    public static class Ponto
    {
        public static double Distancia(List<double> X1, List<double> X2)
        {
            double Soma = 0.0;
            for(int i = 0; i < X1.Count; i++)
            {
                Soma += Math.Pow((X1[i] - X2[i]), 2.0);
            }

            return Math.Sqrt(Soma);
        }

        public static double Media(List<double> list)
        {
            return list.Average();
        }
    }
}
