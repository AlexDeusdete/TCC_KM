﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace TCC_KM
{
    class Hopkins
    {
        private readonly DataTable banco;
        public DataTable RegAmostraBanco { get; private set; }
        public DataTable RegAleatorio{ get; private set; }
        private int casasDecimais;
        public double result { get; private set; }

    /*AReg São registro verdadeiros da minha amostra*/
    /*GReg São registro gerador Aleatoriamente*/
    public Hopkins(BancoDados banco)
        {
            this.banco = banco.GetBancoCalculo();
            casasDecimais = banco.casasDecimais;
            RegAmostraBanco = this.banco.Clone();
            RegAleatorio = this.banco.Clone();
            RegAmostraBanco.Columns.Add("indexOriginal", typeof(Double));
            RegAmostraBanco.Columns.Add("DistanciaMin", typeof(Double));
            RegAleatorio.Columns.Add("DistanciaMin", typeof(Double));
            RegAmostraBanco.Columns["DistanciaMin"].DefaultValue = 0;
            RegAleatorio.Columns["DistanciaMin"].DefaultValue = 0;

            PrencherDadosAReg();
            PrencherDadosGReg();
            CalculoMin(RegAmostraBanco);
            CalculoMin(RegAleatorio);
            result = CalculoFinal();
        }

        private void PrencherDadosAReg()
        {
            /*Preenche com registro aleatorios do banco 25%*/
            var rdn = new Random();
            var aux = new List<int>();
            var i = 0;

            for (int j = 0; j < Convert.ToInt32(banco.Rows.Count / 4.0); j++)
            {
                i = rdn.Next(banco.Rows.Count);
                while (aux.IndexOf(i) >= 0)
                    i = rdn.Next(banco.Rows.Count);
                aux.Add(i);

                RegAmostraBanco.ImportRow(banco.Rows[i]);
                RegAmostraBanco.Rows[RegAmostraBanco.Rows.Count - 1]["indexOriginal"] = i;
            }
        }

        private void PrencherDadosGReg()
        {
            /*Preenche com informações aleatorias 25% do banco*/
            var rdn = new Random();
            for (int j = 0; j < Convert.ToInt32(banco.Rows.Count / 4.0); j++)
            {
                DataRow dr = RegAleatorio.NewRow();
                int max, min;
                for (int i = 0; i < banco.Columns.Count; i++)
                {
                    if (banco.Columns[i].DataType == typeof(long))
                    {
                        min = Convert.ToInt32(banco.Compute("MIN([" + banco.Columns[i].ColumnName + "])", ""));
                        max = Convert.ToInt32(banco.Compute("MAX([" + banco.Columns[i].ColumnName + "])", ""));
                        dr[i] = rdn.Next(min, max);
                    }
                    else
                    {
                        min = Convert.ToInt32(Convert.ToDouble(banco.Compute("MIN([" + banco.Columns[i].ColumnName + "])", "")));
                        max = Convert.ToInt32(Convert.ToDouble(banco.Compute("MAX([" + banco.Columns[i].ColumnName + "])", "")));
                        dr[i] = Math.Round(rdn.Next(min, max) + rdn.NextDouble(), casasDecimais);
                    }
                }
                RegAleatorio.Rows.Add(dr);
            }
        }

        private void CalculoMin(DataTable table)
        {
            double distancia = -1;
            int indexOriginal = -1;

            foreach (DataRow dr in table.Rows)
            {
                List<double> listY = dr.ItemArray.Select(x => Convert.ToDouble(x)).Take(banco.Columns.Count).ToList();
                if (dr.Table.Columns.Contains("indexOriginal"))
                {
                    indexOriginal = Convert.ToInt32(dr["indexOriginal"]);
                }
                
                for (int i = 0; i < banco.Rows.Count; i++)
                {
                    if (dr.Table.Columns.Contains("indexOriginal"))
                    {
                        if (i == indexOriginal)
                            continue;
                    }

                    List<double> listX = banco.Rows[i].ItemArray.Select(x => Convert.ToDouble(x)).Take(banco.Columns.Count).ToList();
                    distancia = Math.Round(Ponto.Distancia(listX, listY), casasDecimais);

                    //i == 0 significa que estou na primeira execução e a distancia ainda está em branco.
                    //i == 1 e index original ==0 significa que estou na primeira execução pois se o index original. == 0 ele pula a primeira execução
                    if ( (i == 0 || (i==1 && indexOriginal == 0))
                         || Convert.ToDouble(dr["DistanciaMin"]) > distancia)
                    {
                        dr["DistanciaMin"] = distancia;
                    }
                }
            }
        }

        private double CalculoFinal()
        {
            double u = Convert.ToDouble(RegAleatorio.Compute("SUM([DistanciaMin])", ""));
            double w = Convert.ToDouble(RegAmostraBanco.Compute("SUM([DistanciaMin])", ""));

            return u / (u + w);
        }
    }
}
