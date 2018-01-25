using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_KM
{
    class BancoDados
    {
        private string Path;
        public char DelimitadorCol { get; private set; }
        public bool IdentificadorRegistro { get; private set; }
        public bool Titulos { get; private set; }
        public DataTable _Banco { get; private set; }

        public BancoDados(string Path)
        {
            this.Path = Path;
            _Banco = new DataTable();
        }

        public void ProcessaLeitura(char DelimitadorCol, bool IdentificadorRegistro, bool Titulos)
        {
            this.DelimitadorCol = DelimitadorCol;
            this.IdentificadorRegistro = IdentificadorRegistro;
            this.Titulos = Titulos;

            CsvToData();
        }

        private void CsvToData()
        {
            using (StreamReader sr = new StreamReader(Path))
            {
                

                if (Titulos)
                {
                    string[] headers = sr.ReadLine().Split(DelimitadorCol);
                    foreach (string header in headers)
                    {
                        _Banco.Columns.Add(header);
                    }
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(DelimitadorCol);

                    if (!Titulos && _Banco.Columns.Count == 0)
                    {
                        for (int i = 0; i < rows.Length; i++)
                        {
                            _Banco.Columns.Add(i.ToString());
                        }
                    }

                    DataRow dr = _Banco.NewRow();
                    for (int i = 0; i < rows.Length; i++)
                    {
                        dr[i] = rows[i].Trim();
                    }
                    _Banco.Rows.Add(dr);
                }

            }
        }

    }
}
