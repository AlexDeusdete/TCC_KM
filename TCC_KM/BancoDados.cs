using System;
using System.Data;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Linq;

namespace TCC_KM
{
    class BancoDados
    {
        private string Caminho;
        public char DelimitadorColuna { get; private set; }
        public bool IdentificadorDoRegistro { get; private set; }
        public bool TemCabecalho{ get; private set; }
        private DataTable Banco;
        public int CasasDecimais { get; private set; }
        public BancoDados(string Caminho, int casasDecimais)
        {
            this.Caminho = Caminho;
            CasasDecimais = casasDecimais;
            Banco = new DataTable();
        }
        /// <summary>
        /// Vai retorna apenas as colunas que devem fazer parte do calculo
        /// EX: em um banco de estudantes a coluna com o numero de registro
        /// do alunos(RA) não deve entrar no calculo 
        /// </summary>
        /// <returns></returns>
        public DataTable GetBancoCalculo()
        {
            if (IdentificadorDoRegistro)
            {
                var aux = Banco.Copy();
                aux.Columns.RemoveAt(0);
                return aux;
            }
            return Banco.Copy();
        }
        public DataTable GetBanco() => Banco;
        public void ProcessaLeitura(char DelimitadorCol, bool IdentificadorRegistro, bool TemCabecalho)
        {
            DelimitadorColuna = DelimitadorCol;
            IdentificadorDoRegistro = IdentificadorRegistro;
            this.TemCabecalho = TemCabecalho;

            CsvToData();
        }
        private void CsvToData()
        {
            var csv = new CsvReader(new StreamReader(Caminho));
            csv.Configuration.Delimiter = DelimitadorColuna.ToString(); 
            while (csv.Read())
            {      
                var i = 0;
                string Coluna = "";
                if (Banco.Columns.Count == 0)
                {
                    while (csv.TryGetField<string>(i, out Coluna))
                    {
                        if (TemCabecalho)
                            Banco.Columns.Add(Coluna,typeof(double));
                        else
                            Banco.Columns.Add(i.ToString(), typeof(double));
                        i++;
                    }
                    if (TemCabecalho)
                        continue;
                }

                var row = Banco.NewRow();
                foreach (DataColumn column in Banco.Columns)
                {
                    row[column.ColumnName] = double.Parse(csv.GetField(Banco.Columns.IndexOf(column)), CultureInfo.InvariantCulture);
                }
                Banco.Rows.Add(row);
            }

            FormataDataTable();
        }
        /// <summary>
        /// formata campos do tipo inteiro para long
        /// pois no momento da leitura do csv não é possivel distinguir o tipo do dado
        /// </summary>
        private void FormataDataTable()
        {
            var temp = Banco.Clone();
            for(int i = 0; i <= Banco.Columns.Count - 1; i++)
            {
                bool allints = Banco.AsEnumerable().All(r => r.Field<Double>(i) == (long)r.Field<Double>(i));
                if (allints)
                    temp.Columns[i].DataType = typeof(long);
            }
            temp.Load(Banco.CreateDataReader());
            Banco = temp;
        }

    }
}
