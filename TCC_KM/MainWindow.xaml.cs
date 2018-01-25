using Microsoft.Win32;
using System.Windows;

namespace TCC_KM
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private BancoDados bancoDados;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCaminho_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtCaminho.Text = openFileDialog.FileName;
            }
            Processamento();
        }

        private void Processamento()
        {
            bancoDados = new BancoDados(txtCaminho.Text);
            bancoDados.ProcessaLeitura(char.Parse(txtDelimitador.Text), chbRegistro.IsChecked.Value, chbTitulo.IsChecked.Value);
            dgBanco.ItemsSource = bancoDados._Banco.DefaultView;
        }
    }
}
