using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using static System.Math;
using System.Data;
using System.Globalization;

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

        public static DataTable PivotGrupoDataTable(DataTable table, string colunast,
                                                     string colunapivot)
        {
            //cria o DataTable
            DataTable returnTable = new DataTable();

            if (colunast == "")
                colunast = table.Columns[0].ColumnName;

            //adiciona a colunast
            returnTable.Columns.Add(colunast);

            foreach (DataRow drtable in table.Rows)
            {
                DataRow dr = returnTable.NewRow();
                dr[0] = double.Parse(drtable[colunast].ToString()).ToString(CultureInfo.InvariantCulture);
                returnTable.Rows.Add(dr);
            }
            //Lê todos os valores diferentes para a coluna X
            List<string> columnXValues = new List<string>();

            columnXValues = (table.AsEnumerable().Select(x => x["grupo"].ToString()).ToList()).Distinct().ToList();
            columnXValues.Sort();
            foreach (var col in columnXValues)
            {
                returnTable.Columns.Add(col);
            }

            //Completa o datatable com os valores
            foreach (DataRow dr in table.Rows)
            {
                var i = 0;
                DataRow[] linhaAtual = returnTable.Select(colunast + " = '" + double.Parse(dr[colunast].ToString()).ToString(CultureInfo.InvariantCulture) + "'");
                while (linhaAtual[i][dr["grupo"].ToString()].ToString() != "")
                    i++;
                linhaAtual[i][dr["grupo"].ToString()] = double.Parse(dr[colunapivot].ToString()).ToString(CultureInfo.InvariantCulture);
            }
            return returnTable;
        }
    }
}
