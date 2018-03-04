using Microsoft.Win32;
using System.Data;
using System.Windows;

namespace TCC_KM
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private BancoDados bancoDados;
        private DataTable hopkinsDT;
        public MainWindow()
        {
            InitializeComponent();
            hopkinsDT = new DataTable();
            hopkinsDT.Columns.Add("Hopkins");
            dgBanco1.ItemsSource = hopkinsDT.DefaultView;
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
            dgBanco.ItemsSource = bancoDados.GetBanco().DefaultView;
        }

        private void btnHopkins_Click(object sender, RoutedEventArgs e)
        {
            var hopkins = new Hopkins(bancoDados);
            hopkinsDT.Rows.Add(hopkins.result);
        }

        private void btnKMedia_Click(object sender, RoutedEventArgs e)
        {
            var kmedias = new Kmedias(bancoDados);
            kmedias.CentroidesIniciais();
            kmedias.CalculaCentroideGeral();
            dgkmedia.ItemsSource = kmedias._dados.DefaultView;
            kmedias.Processamento();
        }
    }
}
