using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace TCC_KM
{
    class Kmedias
    {
        public DataTable Dados{ get; private set; }
        public List<List<double>> Centroides{ get; private set; } = new List<List<double>>();
        public List<double> CentroideGeral { get; private set; } = new List<double>();
        public List<double> Separacoes { get; private set; } = new List<double>();
        public List<double> Coesoes { get; private set; } = new List<double>();
        public int NumeroGrupos { get;private set; }
        private int numeroDeAtributos;
        private Impressao Tela;
        private int NumeroIteracoes= 0;
  
        public Kmedias(BancoDados dados, TextBlock saida, int numeroGrupos)
        {
            //Cria a classe de Saida de dados
            Tela = new Impressao(saida, dados.CasasDecimais);
            Dados = dados.GetBancoCalculo();
            numeroDeAtributos = dados.GetBancoCalculo().Columns.Count;
            Dados.Columns.Add("Grupo", typeof(int));
            Dados.Columns.Add("DistanciaMin", typeof(double));

            /*se não for especificado o numero de grupos
             * o numero de grupos é definido por Sqrt(n/2) onde n
                é o numero de registros da minha base de dados*/
            NumeroGrupos = numeroGrupos == 0 ? Convert.ToInt32(Math.Sqrt((Dados.Rows.Count / 2.0))) : numeroGrupos;

            Tela.Escrever("Número de Grupos : " + NumeroGrupos);
        }
        /// <summary>
        /// Responsavel por iniciar o processo de agrupamento 
        /// </summary>
        public void Processamento()
        {
            Agrupa(Centroides);

            //imprime os grupos e a quantidade de registro que tem em cada um deles
            Tela.Escrever("\n Grupo : Itens : Coesão : Separação");
            for (int i = 0; i <= NumeroGrupos - 1; i++)
            {
                Tela.Escrever("Grupo "+i+" : "+ Dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == i).Count()+
                                " : "+Coesoes[i] +" : "+ Separacoes[i] );
            }
        }
        /// <summary>
        /// Calcula o centróide geral do seus dados pegando a medias de todos os campos
        /// </summary>
        public void CalculaCentroideGeral()
        {
            for(int i = 0; i <= numeroDeAtributos - 1; i++)
            {
                CentroideGeral.Add(
                    Dados.AsEnumerable().Average(x => Convert.ToDouble(x[i]))
                                    );

            }
            Tela.Escrever("Centróide Geral : ");
            Tela.Escrever(CentroideGeral);
        }
        /// <summary>
        /// Calculo da separação - n * d(Ck,C)
        /// n = numero de registros do grupo
        /// d = calculo distancia 
        /// Ck = centroide do grupo
        /// C = centroide geral
        /// </summary>
        public void CalculoSeparacao()
        {
            Separacoes.Clear();
            for(int i = 0; i <= Centroides.Count - 1; i++)
            {
                //Pega o total de registro que tem de determinado grupo
                var reg = Dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == i).Count();


                Separacoes.Add(reg * Formulas.Distancia(Centroides[i], CentroideGeral));    
            }
        }
        /// <summary>
        /// Calculo da Coesao 
        /// soma da distancia de todos os itens do grupo para seu centroide
        /// </summary>
        public void CalculoCoesao()
        {
            Coesoes.Clear();
            for (int i = 0; i <= Centroides.Count - 1; i++)
            {
                Coesoes.Add(
                                Dados.AsEnumerable()
                                .Where(x => x.Field<int>("Grupo") == i)
                                .Sum(x => x.Field<double>("DistanciaMin"))
                            );
            }
        }
        /// <summary>
        /// captura aleatoriamente k centroides dos dados
        /// </summary>
        public void CentroidesIniciais()
        {
            var rdn = new Random();
            var aux = new List<int>();

            for(int i = 0; i <= NumeroGrupos-1 ; i++)
            {
                var reg = rdn.Next(Dados.Rows.Count);
                //operação para evitar dois centroides iguais
                while (aux.IndexOf(reg) >= 0)
                    reg = rdn.Next(Dados.Rows.Count);
                aux.Add(reg);

                Centroides.Add(
                    Dados.Rows[reg].ItemArray.Select(x => Convert.ToDouble(x)).Take(numeroDeAtributos).ToList()
                    );
            }
        }
        /// <summary>
        /// faz o agrupamento dos dados de forma iterativa
        /// </summary>
        /// <param name="Centroides"></param>
        public void Agrupa(List<List<double>> Centroides)
        {
            //lista para armazenar as distancias
            var distancias = new List<double>();
             
            /*percorro todos os registros da minha base de dados
             e para cada registro calculo a distancia entre ele e os centroides
             */
            foreach(DataRow row in Dados.Rows)
            {
                //separo um registro da minha base
                var reg = row.ItemArray.Select(x => Convert.ToDouble(x)).Take(numeroDeAtributos).ToList();
                distancias.Clear();

                /*adiciona a lista as distancia do registro para todos 
                  os centroides */
                foreach (List<double> centroide in Centroides)
                {
                    //calcula a distancia entre o registro e o centroide
                    distancias.Add(
                        Formulas.Distancia(reg, centroide)
                        );
                }

                /*Procuro na lista de distancias qual a menor
                 e coloco ela como "DistanciaMin" do registro*/
                row["DistanciaMin"] = distancias.Min();

                //coloco o grupo desse registro como aquele que teve a menor distancia
                row["Grupo"] = distancias.IndexOf(distancias.Min());

            }

            //guardo os valores de coesão e separação
            var CoesaoGeral = Coesoes.Sum();
            var SeparacaoGeral = Separacoes.Sum();

            //calculo a coesão e separações dos novos grupos gerados 
            CalculoCoesao();
            CalculoSeparacao();

            Tela.Escrever("Iteração :" + NumeroIteracoes);
            Tela.Escrever("\nCentroides:");
            Tela.Escrever(this.Centroides);
            Tela.Escrever("\nCoesão:");
            Tela.Escrever(Coesoes.Sum());
            Tela.Escrever("\nSeparação:");
            Tela.Escrever(Separacoes.Sum());

            RecalculaCentroides();

            //Criterio de parada do algoritmo
            if ((CoesaoGeral == Coesoes.Sum()) &&
                (SeparacaoGeral == Separacoes.Sum()))
            {
                return;
            }

            NumeroIteracoes++;
            Agrupa(this.Centroides);
        }
        /// <summary>
        /// recalcula o centroide
        /// com base na medias dos registros do seu grupo
        /// </summary>
        public void RecalculaCentroides()
        {
            Centroides.Clear();
            var centroide = new List<double>();
            for(int i = 0; i <= NumeroGrupos - 1; i++)
            {
                centroide.Clear();
                for(int j = 0; j<= numeroDeAtributos - 1; j++)
                {
                    var total = Dados.AsEnumerable()
                        .Where(x => x.Field<int>("Grupo") == i)
                        .Average(x => Convert.ToDouble(x[j]));

                    centroide.Add(total);
                }

                Centroides.Add(centroide.ToList());
            }
        }
    }
}
