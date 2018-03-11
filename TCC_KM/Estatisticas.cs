using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_KM
{
    class Estatisticas
    {
        public DataTable _EstatisticaGrupos { get; private set; }
        private enum TiposEstatisticasGrupo {Media,Mediana,DesvioPadrao};
        private int NumeroExecucao = 0;
        private List<string> ColunasDisponiveisCSV = new List<string> { "NumeroExecucao", "Grupo", "QuantidadeDeRegistros" };
        /// <summary>
        /// Cria a classe e as colunas pricipais dos relatrios
        /// </summary>
        public Estatisticas()
        {
            _EstatisticaGrupos = new DataTable();

            _EstatisticaGrupos.Columns.Add("NumeroExecucao", typeof(int));
            _EstatisticaGrupos.Columns.Add("Grupo", typeof(int));
            _EstatisticaGrupos.Columns.Add("QuantidadeDeRegistros", typeof(int));
        }
        /// <summary>
        /// Recebe os dados já processados pelo algoritmo das kmedias e calcula
        /// algumas estatisticas
        /// </summary>
        /// <param name="dados">retorno do algpritmo das kmedias</param>
        public void SetEstatisticaGrupos(DataTable dados)
        {
            if (_EstatisticaGrupos.Columns.Count == 3)
                CriaColunasEstatisticaGrupo(dados);

            //Controla o numero de vezes que o metodo é executado para gravar
            NumeroExecucao++;
            var numeroGrupos = dados.AsEnumerable().GroupBy(x => x.Field<int>("Grupo")).Count();

            for(int grupo = 0; grupo <= numeroGrupos - 1; grupo++)
            {
                var dr = _EstatisticaGrupos.NewRow();
                dr["NumeroExecucao"] = NumeroExecucao;
                dr["Grupo"] = grupo;
                dr["QuantidadeDeRegistros"] = dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == grupo).Count();

                /*Precorro a tabela original calculo as estatisticas e coloco nas colunas 
                 correspondentes na minha taela de estatisticas*/
                foreach (DataColumn column in dados.Columns)
                {
                    
                    foreach (var tipoestatistica in Enum.GetNames(typeof(TiposEstatisticasGrupo)))
                    {
                        //Verifica se a coluna existe antes de inserir a estatistica
                        if (_EstatisticaGrupos.Columns.Contains(column.ColumnName + '-' + tipoestatistica))                       
                            dr[column.ColumnName + '-' + tipoestatistica] = GetEstatistica((TiposEstatisticasGrupo)Enum.Parse(typeof(TiposEstatisticasGrupo), tipoestatistica),
                                                                                  grupo,
                                                                                  dados,
                                                                                  column.ColumnName);
                    }
                }
                _EstatisticaGrupos.Rows.Add(dr);
            }
        }
        /// <summary>
        /// Especifica as colunas que devem aparecer no relatorio CSV
        /// </summary>
        /// <param name="NomeColuna"></param>
        public void SetColunaCSV(String NomeColuna)
        {
            foreach (var tipoestatistica in Enum.GetNames(typeof(TiposEstatisticasGrupo)))
            {
                ColunasDisponiveisCSV.Add(NomeColuna + '-' + tipoestatistica);
            }
        }
        /// <summary>
        /// Cria as colunas com as estatisticas disponiveis
        /// </summary>
        /// <param name="dados"></param>
        private void CriaColunasEstatisticaGrupo(DataTable dados)
        {
            for (int i = 0; i <= dados.Columns.Count - 1; i++)
            {
                //Coluna que gruarda o grupo então ao chegar nele pula
                if ((dados.Columns[i].ColumnName == "Grupo") || 
                    (dados.Columns[i].ColumnName == "DistanciaMin"))
                    continue;

                /*itero na lista do tipos de estatistica para criar uma coluna para cada um deles no banco de dados*/
                foreach (var tipoestatistica in Enum.GetNames(typeof(TiposEstatisticasGrupo)))
                {
                    _EstatisticaGrupos.Columns.Add(dados.Columns[i].ColumnName +'-'+ tipoestatistica, typeof(double));
                }
            }
        }
        /// <summary>
        /// faz o calculo das estatisticas
        /// </summary>
        /// <param name="TipoEstatistica">ex (media, mediana, desviopadrão)</param>
        /// <param name="Grupo">De qual grupo deve ser calculado a estatistica acima</param>
        /// <param name="Dados">conjunto de dados</param>
        /// <param name="Coluna">qual coluna deve ser feito a estatistica</param>
        /// <returns></returns>
        private double GetEstatistica(TiposEstatisticasGrupo TipoEstatistica, int Grupo, DataTable Dados, string Coluna)
        {
            switch (TipoEstatistica)
            {
                case TiposEstatisticasGrupo.Media:
                    return Dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == Grupo).Average(x => Convert.ToDouble(x[Coluna]));
                case TiposEstatisticasGrupo.Mediana:
                    return Dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == Grupo).Mediana(x => Convert.ToDouble(x[Coluna]));
                case TiposEstatisticasGrupo.DesvioPadrao:
                    return Dados.AsEnumerable().Where(x => x.Field<int>("Grupo") == Grupo).DesvioPadrao(x => Convert.ToDouble(x[Coluna]));
                default:
                    break;
            }
            return 0;
        }
        /// <summary>
        /// Pega as estatisticas ja calculadas
        /// e de acordo com as colunas especificadas salva um csv
        /// </summary>
        /// <param name="path">caminho onde o arquivo deve ficar</param>
        public void SalvarCSVGrupos(String path)
        {
            using (var textWriter = File.CreateText(path))
            using (var csv = new CsvWriter(textWriter))
            {
                csv.Configuration.Delimiter = ";";
                // Criar colunas
                foreach (DataColumn column in _EstatisticaGrupos.Columns)
                {
                    //só crias as colunas já especificadas
                    if(ColunasDisponiveisCSV.Contains(column.ColumnName))
                        csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();

                // Cria as linhas
                foreach (DataRow row in _EstatisticaGrupos.Rows)
                {
                    for (var i = 0; i < _EstatisticaGrupos.Columns.Count; i++)
                    {
                        if (ColunasDisponiveisCSV.Contains(_EstatisticaGrupos.Columns[i].ColumnName))
                            csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }
    }
}
