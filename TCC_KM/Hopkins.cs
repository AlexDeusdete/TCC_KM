using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace TCC_KM
{
    class Hopkins
    {
        private readonly DataTable _dados;
        public DataTable _regAmostraBanco{ get; private set; }
        public DataTable _regAleatorios{ get; private set; }
        public double Result { get; private set; }

    public Hopkins(BancoDados banco)
        {
            this._dados = banco.GetBancoCalculo();
            _regAmostraBanco = this._dados.Clone();
            _regAleatorios = this._dados.Clone();
            _regAmostraBanco.Columns.Add("indexOriginal", typeof(Double));
            _regAmostraBanco.Columns.Add("DistanciaMin", typeof(Double));
            _regAleatorios.Columns.Add("DistanciaMin", typeof(Double));
            _regAmostraBanco.Columns["DistanciaMin"].DefaultValue = 0;
            _regAleatorios.Columns["DistanciaMin"].DefaultValue = 0;

            PrencherDadosAReg();
            PrencherDadosGReg();
            CalculoMin(_regAmostraBanco);
            CalculoMin(_regAleatorios);
            Result = CalculoFinal();
        }

        private void PrencherDadosAReg()
        {
            /*Preenche com registro aleatorios do banco 25%*/
            var rdn = new Random();
            var aux = new List<int>();

            for (int j = 0; j < Convert.ToInt32(_dados.Rows.Count / 4.0); j++)
            {
                var i = rdn.Next(_dados.Rows.Count);
                while (aux.IndexOf(i) >= 0)
                    i = rdn.Next(_dados.Rows.Count);
                aux.Add(i);

                _regAmostraBanco.ImportRow(_dados.Rows[i]);
                _regAmostraBanco.Rows[_regAmostraBanco.Rows.Count - 1]["indexOriginal"] = i;
            }
        }

        private void PrencherDadosGReg()
        {
            /*Preenche com informações aleatorias 25% do banco*/
            var rdn = new Random();
            for (int j = 0; j < Convert.ToInt32(_dados.Rows.Count / 4.0); j++)
            {
                DataRow dr = _regAleatorios.NewRow();
                int max, min;
                for (int i = 0; i < _dados.Columns.Count; i++)
                {
                    if (_dados.Columns[i].DataType == typeof(long))
                    {
                        min = Convert.ToInt32(_dados.AsEnumerable().Min(x => x.Field<long>(i)));
                        max = Convert.ToInt32(_dados.AsEnumerable().Max(x => x.Field<long>(i)));
                        dr[i] = rdn.Next(min, max);
                    }
                    else
                    {
                        min = Convert.ToInt32(_dados.AsEnumerable().Min(x => x.Field<double>(i)));
                        max = Convert.ToInt32(_dados.AsEnumerable().Max(x => x.Field<double>(i)));
                        dr[i] = rdn.Next(min, max) + rdn.NextDouble();
                    }
                }
                _regAleatorios.Rows.Add(dr);
            }
        }

        private void CalculoMin(DataTable table)
        {
            double distancia = -1;
            int indexOriginal = -1;

            foreach (DataRow dr in table.Rows)
            {
                List<double> listY = dr.ItemArray.Select(x => Convert.ToDouble(x)).Take(_dados.Columns.Count).ToList();
                if (dr.Table.Columns.Contains("indexOriginal"))
                {
                    indexOriginal = Convert.ToInt32(dr["indexOriginal"]);
                }
                
                for (int i = 0; i < _dados.Rows.Count; i++)
                {
                    if (dr.Table.Columns.Contains("indexOriginal"))
                    {
                        if (i == indexOriginal)
                            continue;
                    }

                    List<double> listX = _dados.Rows[i].ItemArray.Select(x => Convert.ToDouble(x)).Take(_dados.Columns.Count).ToList();
                    distancia = Ponto.Distancia(listX, listY);

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
            
            double u = Convert.ToDouble(_regAleatorios.AsEnumerable().Sum(x => x.Field<double>("DistanciaMin")));
            double w = Convert.ToDouble(_regAmostraBanco.AsEnumerable().Sum(x => x.Field<double>("DistanciaMin")));

            return u / (u + w);
        }
    }
}
