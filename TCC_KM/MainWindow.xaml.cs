using CsvHelper;
using Microsoft.Win32;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace TCC_KM
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        //Variaveis que vão controlar o banco de dados atual
        private BancoDados bancoDados;
        private DataTable hopkinsDT;
        private Estatisticas estatisticas;
        
        public MainWindow()
        {
            InitializeComponent();
            hopkinsDT = new DataTable();
            hopkinsDT.Columns.Add("Hopkins", typeof(double));
            dgHopkins.ItemsSource = hopkinsDT.DefaultView;
        }

        private void btnCaminho_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtCaminho.Text = openFileDialog.FileName;
                Processamento();
            }
        }

        private void Processamento()
        {
            bancoDados = new BancoDados(txtCaminho.Text, int.Parse(txtCasasDecimais.Text));
            bancoDados.ProcessaLeitura(char.Parse(txtDelimitador.Text), chbRegistro.IsChecked.Value, chbTitulo.IsChecked.Value);
            dgDados.ItemsSource = bancoDados.GetBanco().DefaultView;
        }

        private void btnHopkins_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Conteiner.IsEnabled = false;
            //Executa a estatistica de Hopkins o numero de vezes inserido pelo usuario
            for (int i = 0; i <= int.Parse(txtQtdHopkins.Text) - 1; i++)
            {
                var hopkins = new Hopkins(bancoDados, tbHopkins);
                hopkinsDT.Rows.Add(hopkins.Result);
                lbMediaHP.Content = "Media : " + hopkinsDT.AsEnumerable().Average(x => x.Field<double>(0));
            }
            Conteiner.IsEnabled = true;
            Mouse.OverrideCursor = null;
        }

        private void btnKMedia_Click(object sender, RoutedEventArgs e)
        {
            //Criar classe que vai armazenar dados estatisticos de cada execução do k-media
            estatisticas = new Estatisticas();

            Mouse.OverrideCursor = Cursors.Wait;
            Conteiner.IsEnabled = false;
            //Executa o metodo das K-medias o numero de vezes inserido pelo usuario
            for(int i = 0; i <= int.Parse(txtQtdKmedia.Text) - 1; i++)
            {
                var kmedias = new Kmedias(bancoDados, tbKmedias);
                kmedias.CentroidesIniciais();
                kmedias.CalculaCentroideGeral();
                dgkmedia.ItemsSource = kmedias._dados.DefaultView;
                kmedias.Processamento();
                //guarda estatisticas dessa execução
                estatisticas.SetEstatisticaGrupos(kmedias._dados);
            }
            Conteiner.IsEnabled = true;
            Mouse.OverrideCursor = null;
        }
    }
}
