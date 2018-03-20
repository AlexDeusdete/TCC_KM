using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace TCC_KM
{
    class Kmedias
    {
        public DataTable _dados{ get; private set; }
        public List<List<double>> _centroides{ get; private set; } = new List<List<double>>();
        public List<double> _centroideGeral { get; private set; } = new List<double>();
        public List<double> _separacoes { get; private set; } = new List<double>();
        public List<double> _coesoes { get; private set; } = new List<double>();
        public int _numeroGrupos { get;private set; }
        private int numeroDeAtributos;
        private Impressao Tela;
        private int NumeroIteracoes = 0;
  
        public Kmedias(BancoDados dados, TextBlock saida, int numeroGrupos)
        {
            //Cria a classe de Saida de dados
            Tela = new Impressao(saida, dados._casasDecimais);
            _dados = dados.GetBancoCalculo();
            numeroDeAtributos = dados.GetBancoCalculo().Columns.Count;
            _dados.Columns.Add("Grupo", typeof(int));
            _dados.Columns.Add("DistanciaMin", typeof(double));

            /*se não for especificado o numero de grupos
             * o numero de grupos é definido por Sqrt(n/2) onde n
                é o numero de registros da minha base de dados*/
            _numeroGrupos = numeroGrupos == 0 ? Convert.ToInt32(Math.Sqrt((_dados.Rows.Count / 2.0))) : numeroGrupos;

            Tela.Escrever("Número de Grupos : " + _numeroGrupos);
        }

        /// <summary>
        /// Responsavel por iniciar o processo de agrupamento 
        /// </summary>
        public void Processamento()
        {
            Agrupa(_centroides);

            //imprime os grupos e a quantidade de registro que tem em cada um deles
            Tela.Escrever("\n Grupo : Itens : Coesão : Separação");
            for (int i = 0; i <= _numeroGrupos - 1; i++)
            {
                Tela.Escrever("Grupo "+i+" : "+ _dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == i).Count()+
                                " : "+_coesoes[i] +" : "+ _separacoes[i] );
            }
        }
        /// <summary>
        /// Calcula o centróide geral do seus dados pegando a medias de todos os campos
        /// </summary>
        public void CalculaCentroideGeral()
        {
            for(int i = 0; i <= numeroDeAtributos - 1; i++)
            {
                _centroideGeral.Add(
                    _dados.AsEnumerable().Average(x => Convert.ToDouble(x[i]))
                                    );

            }
            Tela.Escrever("Centróide Geral : ");
            Tela.Escrever(_centroideGeral);
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
            _separacoes.Clear();
            for(int i = 0; i <= _centroides.Count - 1; i++)
            {
                //Pega o total de registro que tem de determinado grupo
                var reg = _dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == i).Count();


                _separacoes.Add(reg * Formulas.Distancia(_centroides[i], _centroideGeral));    
            }
        }

        /// <summary>
        /// Calculo da Coesao 
        /// soma da distancia de todos os itens do grupo para seu centroide
        /// </summary>
        public void CalculoCoesao()
        {
            _coesoes.Clear();
            for (int i = 0; i <= _centroides.Count - 1; i++)
            {
                _coesoes.Add(
                                _dados.AsEnumerable()
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

            for(int i = 0; i <= _numeroGrupos-1 ; i++)
            {
                var reg = rdn.Next(_dados.Rows.Count);
                //operação para evitar dois centroides iguais
                while (aux.IndexOf(reg) >= 0)
                    reg = rdn.Next(_dados.Rows.Count);
                aux.Add(reg);

                _centroides.Add(
                    _dados.Rows[reg].ItemArray.Select(x => Convert.ToDouble(x)).Take(numeroDeAtributos).ToList()
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
            foreach(DataRow row in _dados.Rows)
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
            var CoesaoGeral = _coesoes.Sum();
            var SeparacaoGeral = _separacoes.Sum();

            //calculo a coesão e separações dos novos grupos gerados 
            CalculoCoesao();
            CalculoSeparacao();

            Tela.Escrever("Iteração :" + NumeroIteracoes);
            Tela.Escrever("\nCentroides:");
            Tela.Escrever(_centroides);
            Tela.Escrever("\nCoesão:");
            Tela.Escrever(_coesoes.Sum());
            Tela.Escrever("\nSeparação:");
            Tela.Escrever(_separacoes.Sum());

            RecalculaCentroides();

            //Criterio de parada do algoritmo
            if ((CoesaoGeral == _coesoes.Sum()) &&
                (SeparacaoGeral == _separacoes.Sum()))
            {
                return;
            }

            NumeroIteracoes++;
            Agrupa(_centroides);
        }

        /// <summary>
        /// recalcula o centroide
        /// com base na medias dos registros do seu grupo
        /// </summary>
        public void RecalculaCentroides()
        {
            _centroides.Clear();
            var centroide = new List<double>();
            for(int i = 0; i <= _numeroGrupos - 1; i++)
            {
                centroide.Clear();
                for(int j = 0; j<= numeroDeAtributos - 1; j++)
                {
                    var total = _dados.AsEnumerable()
                        .Where(x => x.Field<int>("Grupo") == i)
                        .Average(x => Convert.ToDouble(x[j]));

                    centroide.Add(total);
                }

                _centroides.Add(centroide.ToList());
            }
        }
    }
}
