using System;
using System.Collections.Generic;

namespace TCC_KM
{
    public static class Ponto
    {
        public static double Distancia(List<float> X1, List<float> X2)
        {
            double Soma = 0.0;
            for(int i = 0; i < X1.Count; i++)
            {
                Soma += Math.Pow((X1[i] - X2[1]), 2.0);
            }

            return Math.Sqrt(Soma);
        }
    }
}
