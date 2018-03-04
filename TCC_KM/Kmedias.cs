using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private int numeroDeCampos;

        public Kmedias(BancoDados dados)
        {
            _dados = dados.GetBancoCalculo();
            numeroDeCampos = dados.GetBancoCalculo().Columns.Count;
            _dados.Columns.Add("Grupo", typeof(int));
            _dados.Columns.Add("DistanciaMin", typeof(double));

            _numeroGrupos = Convert.ToInt32(Math.Sqrt((_dados.Rows.Count / 2.0)));
        }

        public void Processamento()
        {
            Agrupa(_centroides);
        }

        public void CalculaCentroideGeral()
        {
            for(int i = 0; i <= numeroDeCampos - 1; i++)
            {
                _centroideGeral.Add(
                    _dados.AsEnumerable().Average(x => Convert.ToDouble(x[i]))
                                    );

            }
        }

        public void CalculoSeparacao()
        {
            _separacoes.Clear();
            for(int i = 0; i <= _centroides.Count - 1; i++)
            {
                //Pega o total de registro que tem de determinado grupo
                var reg = _dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == i).Count();


                _separacoes.Add(reg * Ponto.Distancia(_centroides[i], _centroideGeral));    
            }
        }

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
                    _dados.Rows[reg].ItemArray.Select(x => Convert.ToDouble(x)).Take(numeroDeCampos).ToList()
                    );
            }
        }

        public void Agrupa(List<List<double>> Centroides)
        {
            var distancias = new List<double>();
             
            foreach(DataRow row in _dados.Rows)
            {
                var reg = row.ItemArray.Select(x => Convert.ToDouble(x)).Take(numeroDeCampos).ToList();

                distancias.Clear();
                foreach (List<double> centroide in Centroides)
                {
                    distancias.Add(
                        Ponto.Distancia(reg, centroide)
                        );
                }

                row["DistanciaMin"] = distancias.Min();
                row["Grupo"] = distancias.IndexOf(distancias.Min());

            }

            var CoesaoGeral = _coesoes.Sum();
            var SeparacaoGeral = _separacoes.Sum();

            CalculoCoesao();
            CalculoSeparacao();

            RecalculaCentroides();

            if ((CoesaoGeral == _coesoes.Sum()) &&
                (SeparacaoGeral == _separacoes.Sum()))
            {
                return;
            }

            Agrupa(_centroides);
        }

        public void RecalculaCentroides()
        {
            _centroides.Clear();
            int n;
            var centroide = new List<double>();
            for(int i = 0; i <= _numeroGrupos - 1; i++)
            {
                centroide.Clear();

                n = _dados.AsEnumerable()
                                .Where(x => x.Field<int>("Grupo") == i).Count();
                for(int j = 0; j<= numeroDeCampos - 1; j++)
                {
                    var total = _dados.AsEnumerable()
                        .Where(x => x.Field<int>("Grupo") == i)
                        .Sum(x => Convert.ToDouble(x[j]));

                    centroide.Add(total/n);
                }

                _centroides.Add(centroide.ToList());
            }
        }
    }
}
