using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TCC_KM
{
    class Impressao
    {
        private TextBlock Saida;
        private int CasasDecimais;
        public Impressao(TextBlock Saida, int CasasDecimais)
        {
            this.Saida = Saida;
            this.CasasDecimais = CasasDecimais;
            Saida.Text = "";
        }

        public void Escrever(string value)
        {
            Saida.Text = Saida.Text + value + "\n";
        }

        public void Escrever(List<List<double>> value)
        {
            foreach (List<double> List in value)
            {
                Escrever(List);
            }
        }

        public void Escrever(List<double> value)
        {
            string aux = "";
            for (var i = 0; i <= value.Count - 1; i++)
            {
                
                aux = aux + String.Format("{0,20:F"+CasasDecimais+"}", value[i]) + " | ";
            }
            Escrever(aux);
        }

        public void Escrever(double value)
        {
            var aux = value.ToString("F" + CasasDecimais);
            Escrever(aux);
        }
        
    }
}
