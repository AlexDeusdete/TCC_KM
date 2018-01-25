using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_KM
{
    class Hopkins
    {
        private readonly DataTable banco;
        public DataTable AReg { get; private set; }
        public DataTable GReg{ get; private set; }

    /*AReg São registro verdadeiros da minha amostra*/
    /*GReg São registro gerador Aleatoriamente*/
    public Hopkins(DataTable banco)
        {
            this.banco = banco;
            AReg = banco.Clone();
            GReg = banco.Clone();

            PrencherDadosAReg();
            PrencherDadosGReg();
        }

        private void PrencherDadosAReg()
        {
            /*Preenche com registro aleatorios do banco*/
            var rdn = new Random();
            var aux = new List<int>();
            var i = 0;

            for (int j = 0; j < Convert.ToInt32(banco.Rows.Count / 3.0); j++)
            {
                i = rdn.Next(banco.Rows.Count);
                aux.Add(i);
                while (aux.IndexOf(i) >= 0)
                    i++;
                AReg.ImportRow(banco.Rows[i]);
            }
        }

        private void PrencherDadosGReg()
        {
            /*Preenche com informações aleatorias*/
            var rdn = new Random();
            for (int j = 0; j < Convert.ToInt32(banco.Rows.Count / 3.0); j++)
            {
                DataRow dr = GReg.NewRow();
                for (int i = 0; i < GReg.Columns.Count; i++)
                {
                    dr[i] = rdn.Next();
                }
                GReg.Rows.Add(dr);
            }
        }
    }
}
