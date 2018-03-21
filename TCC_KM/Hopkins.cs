using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace TCC_KM
{
    class Hopkins
    {
        private Impressao Tela;
        private readonly DataTable Dados;
        public DataTable RegAmostraBanco{ get; private set; }
        public DataTable RegAleatorios{ get; private set; }
        public double Result { get; private set; }

    public Hopkins(BancoDados dados, TextBlock Saida)
        {
            Tela = new Impressao(Saida,dados.CasasDecimais);
            Dados = dados.GetBancoCalculo();
            
            //cria dos bancos que vão armazenas amostra de dados para calculo do hopkins
            RegAmostraBanco = Dados.Clone();
            RegAleatorios = Dados.Clone();

            /*"indexOriginal" é o index do registro na base de dados 
             original*/
            RegAmostraBanco.Columns.Add("indexOriginal", typeof(Double));
            RegAmostraBanco.Columns.Add("DistanciaMin", typeof(Double));
            RegAmostraBanco.Columns["DistanciaMin"].DefaultValue = 0;

            RegAleatorios.Columns.Add("DistanciaMin", typeof(Double));
            RegAleatorios.Columns["DistanciaMin"].DefaultValue = 0;

            Tela.Escrever("Pegando amostra do banco.");
            PreencherAmostraBanco();

            Tela.Escrever("Gerando registros aleatorios");
            PreencherAleatorios();

            Tela.Escrever("Calculando distancias minimas");
            CalculoMin(RegAmostraBanco);
            CalculoMin(RegAleatorios);

            Tela.Escrever("Finalizando!");
            Result = CalculoFinal();
        }

        private void PreencherAmostraBanco()
        {
            /*Preenche com registro aleatorios do banco 25%*/
            var rdn = new Random();
            var aux = new List<int>();

            for (int j = 0; j < Convert.ToInt32(Dados.Rows.Count / 4.0); j++)
            {
                var i = rdn.Next(Dados.Rows.Count);
                while (aux.IndexOf(i) >= 0)
                    i = rdn.Next(Dados.Rows.Count);
                aux.Add(i);

                RegAmostraBanco.ImportRow(Dados.Rows[i]);
                RegAmostraBanco.Rows[RegAmostraBanco.Rows.Count - 1]["indexOriginal"] = i;
            }
        }
        /// <summary>
        /// preenche o  _regAleatorios com registros aleatorios com 
        /// quantidade igual a 25% da quantidade de dados da base original
        /// </summary>
        private void PreencherAleatorios()
        {
            var rdn = new Random();
            for (int j = 0; j < Convert.ToInt32(Dados.Rows.Count / 4.0); j++)
            {
                DataRow dr = RegAleatorios.NewRow();
                int max, min;
                for (int i = 0; i < Dados.Columns.Count; i++)
                {
                    //é verificado o tipo da coluna para gerar o dado aleatorio
                    if (Dados.Columns[i].DataType == typeof(long))
                    {
                        min = Convert.ToInt32(Dados.AsEnumerable().Min(x => x.Field<long>(i)));
                        max = Convert.ToInt32(Dados.AsEnumerable().Max(x => x.Field<long>(i)));
                        dr[i] = rdn.Next(min, max);
                    }
                    else
                    {
                        min = Convert.ToInt32(Dados.AsEnumerable().Min(x => x.Field<double>(i)));
                        max = Convert.ToInt32(Dados.AsEnumerable().Max(x => x.Field<double>(i)));
                        dr[i] = rdn.Next(min, max) + rdn.NextDouble();
                    }
                }
                RegAleatorios.Rows.Add(dr);
            }
        }
        /// <summary>
        /// faz o calculo da distancia minima dos registros de table
        /// para a minha base de dados original
        /// </summary>
        /// <param name="table"></param>
        private void CalculoMin(DataTable table)
        {
            double distancia = -1;
            int indexOriginal = -1;

            foreach (DataRow dr in table.Rows)
            {
                List<double> listY = dr.ItemArray.Select(x => Convert.ToDouble(x)).Take(Dados.Columns.Count).ToList();
                if (dr.Table.Columns.Contains("indexOriginal"))
                {
                    /*se tiver "indexOriginal" gravo esse 
                    valor para evitar calcular a distancia dele para ele mesmo*/
                    indexOriginal = Convert.ToInt32(dr["indexOriginal"]);
                }
                
                for (int i = 0; i < Dados.Rows.Count; i++)
                {
                    if (dr.Table.Columns.Contains("indexOriginal"))
                    {
                        if (i == indexOriginal)
                            continue;
                    }

                    List<double> listX = Dados.Rows[i].ItemArray.Select(x => Convert.ToDouble(x)).Take(Dados.Columns.Count).ToList();
                    distancia = Formulas.Distancia(listX, listY);

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
        /// <summary>
        /// soma as distancias minimas encontradas e aplica a 
        /// formula de estatistica de hopkins
        /// </summary>
        /// <returns>Hopkins</returns>
        private double CalculoFinal()
        {
            
            double u = Convert.ToDouble(RegAleatorios.AsEnumerable().Sum(x => x.Field<double>("DistanciaMin")));
            double w = Convert.ToDouble(RegAmostraBanco.AsEnumerable().Sum(x => x.Field<double>("DistanciaMin")));

            return u / (u + w);
        }
    }
}
