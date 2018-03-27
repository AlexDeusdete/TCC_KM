using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_KM
{
    class Grafico
    {
        public DataTable Dados { get; set; }
        public StringBuilder HtmlGrafico { get; private set; }
        private string X, Y;
        public Grafico(DataTable Dados, string X, string Y)
        {
            HtmlGrafico = new StringBuilder();
            this.X = X;
            this.Y = Y;
            this.Dados = Formulas.PivotGrupoDataTable(Dados, X, Y);
        }
        public void BindChart()
        {
            try
            {
                String path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
                HtmlGrafico.Append("<html><head>");
                HtmlGrafico.Append("<script type='text/javascript' src='file://C:\\Users\\Public\\Documents\\grafico.js'></script>");
                HtmlGrafico.Append("<script type = 'text/javascript' >");

                HtmlGrafico.Append(@" google.charts.load('current', {'packages':['corechart']});
                                      google.charts.setOnLoadCallback(drawChart);

                                      function drawChart() {
                                        var data = google.visualization.arrayToDataTable([[");
                foreach(DataColumn col in Dados.Columns)
                {
                    HtmlGrafico.Append("'"+col.ColumnName+"',");
                }
                HtmlGrafico.Remove(HtmlGrafico.Length - 1, 1);
                HtmlGrafico.Append("],");
                foreach (DataRow row in Dados.Rows)
                {
                    HtmlGrafico.Append("[");
                    for (int i = 0; i < Dados.Columns.Count; i++)
                    {
                        HtmlGrafico.Append(row[i]+",");
                    }
                    HtmlGrafico.Remove(HtmlGrafico.Length - 1, 1);
                    HtmlGrafico.Append("],");
                }
                HtmlGrafico.Remove(HtmlGrafico.Length - 1, 1);
                HtmlGrafico.Append("]);");

                HtmlGrafico.Append("var options = {");
                HtmlGrafico.Append("    title: '"+X+" vs. "+Y+" comparacao',");
                HtmlGrafico.Append("    hAxis: {title: '"+X+"'},");
                HtmlGrafico.Append("    vAxis: {title: '"+Y+"'},");
                HtmlGrafico.Append("    legend: 'grupos'");
                HtmlGrafico.Append(@"};
                                        var chart = new google.visualization.ScatterChart(document.getElementById('chart_div'));
                                        chart.draw(data, options);
                                      }");
                HtmlGrafico.Append(@"</script>
                                  </head>
                                  <body>
                                    <div id='chart_div' style='height: 700px; width: 100%;'></div>
                                   </body >
                                 </html >");
            }
            finally
            {

            }
        }
    }
}
