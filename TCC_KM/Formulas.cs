using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using static System.Math;

namespace TCC_KM
{
    public static class Formulas
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
        public static double Mediana(this IEnumerable<double> source)
        {
            if (source.Count() == 0)
            {
                throw new InvalidOperationException("Cannot compute median for an empty set.");
            }

            var sortedList = from number in source
                             orderby number
                             select number;

            int itemIndex = (int)sortedList.Count() / 2;

            if (sortedList.Count() % 2 == 0)
            {
                // Even number of items.  
                return (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;
            }
            else
            {
                // Odd number of items.  
                return sortedList.ElementAt(itemIndex);
            }
        }
        public static double Mediana<T>(this IEnumerable<T> source,Func<T, double> selector)
        {
            return (from num in source select selector(num)).Mediana();
        }
        public static double Variancia(this IEnumerable<double> source)
        {
            int n = source.Count();
            double media = source.Average();
            double M2 = 0;

            foreach (double x in source)
            {
                M2 += Pow((x - media), 2);
            }
            return M2 / (n);
        }
        public static double Variancia<T>(this IEnumerable<T> source,Func<T, double> selector)
        {
            return (from num in source select selector(num)).Variancia();
        }
        public static double DesvioPadrao(this IEnumerable<double> source)
        {
            return Math.Sqrt(source.Variancia());
        }
        public static double DesvioPadrao<T>(this IEnumerable<T> source,Func<T, double> selector)
        {
            return (from num in source select selector(num)).DesvioPadrao();
        }
    }
}
