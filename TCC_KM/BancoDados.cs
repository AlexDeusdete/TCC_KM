using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Globalization;
using System.Data.OleDb;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;

namespace TCC_KM
{
    class BancoDados
    {
        private string Caminho;
        public char _delimitadorColuna { get; private set; }
        public bool _identificadorDoRegistro { get; private set; }
        public bool _temCabecalho{ get; private set; }
        private DataTable Banco;
        public int _casasDecimais { get; private set; }

        public BancoDados(string Path, int casasDecimais)
        {
            this.Caminho = Path;
            this._casasDecimais = casasDecimais;
            Banco = new DataTable();
        }

        public DataTable GetBancoCalculo()
        {
            if (_identificadorDoRegistro)
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
            this._delimitadorColuna = DelimitadorCol;
            this._identificadorDoRegistro = IdentificadorRegistro;
            this._temCabecalho = TemCabecalho;

            CsvToData();
        }

        private void CsvToData()
        {
            var csv = new CsvReader(new StreamReader(Caminho));
            csv.Configuration.Delimiter = _delimitadorColuna.ToString(); 
            while (csv.Read())
            {      
                var i = 0;
                string Coluna = "";
                if (Banco.Columns.Count == 0)
                {
                    while (csv.TryGetField<string>(i, out Coluna))
                    {
                        if (_temCabecalho)
                            Banco.Columns.Add(Coluna,typeof(double));
                        else
                            Banco.Columns.Add(i.ToString(), typeof(double));
                        i++;
                    }
                    if (_temCabecalho)
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
