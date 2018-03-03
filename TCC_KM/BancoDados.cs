using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Globalization;
using System.Data.OleDb;
using CsvHelper;
using CsvHelper.Configuration;

namespace TCC_KM
{
    class BancoDados
    {
        private string Caminho;
        public char DelimitadorCol { get; private set; }
        public bool IdentificadorRegistro { get; private set; }
        public bool TemCabecalho{ get; private set; }
        private DataTable _Banco;
        public int casasDecimais;

        public BancoDados(string Path, int casasDecimais)
        {
            this.Caminho = Path;
            this.casasDecimais = casasDecimais;
            _Banco = new DataTable();
        }

        public DataTable GetBancoCalculo()
        {
            if (IdentificadorRegistro)
            {
                var aux = _Banco.Copy();
                aux.Columns.RemoveAt(0);
                return aux;
            }
            return _Banco.Copy();
        }

        public DataTable GetBanco() => _Banco;

        public void ProcessaLeitura(char DelimitadorCol, bool IdentificadorRegistro, bool TemCabecalho)
        {
            this.DelimitadorCol = DelimitadorCol;
            this.IdentificadorRegistro = IdentificadorRegistro;
            this.TemCabecalho = TemCabecalho;

            CsvToData();
        }

        private void CsvToData()
        {
            var csv = new CsvReader(new StreamReader(Caminho));
            csv.Configuration.Delimiter = DelimitadorCol.ToString(); 
            while (csv.Read())
            {      
                var i = 0;
                string Coluna = "";
                if (_Banco.Columns.Count == 0)
                {
                    while (csv.TryGetField<string>(i, out Coluna))
                    {
                        if (TemCabecalho)
                            _Banco.Columns.Add(Coluna,typeof(double));
                        else
                            _Banco.Columns.Add(i.ToString(), typeof(double));
                        i++;
                    }
                    if (TemCabecalho)
                        continue;
                }

                var row = _Banco.NewRow();
                foreach (DataColumn column in _Banco.Columns)
                {
                    row[column.ColumnName] = Math.Round(double.Parse(csv.GetField(_Banco.Columns.IndexOf(column)), CultureInfo.InvariantCulture), casasDecimais);
                }
                _Banco.Rows.Add(row);
            }
        }

    }
}
