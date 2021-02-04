using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;
        private readonly TaskScheduler taskSchedulerUI;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
            taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnProcessar.IsEnabled = false;

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            var resultado = await ConsolidarContas(r_Repositorio.GetContaClientes());

            AtualizarView(resultado, DateTime.Now - inicio);

            BtnProcessar.IsEnabled = !BtnProcessar.IsEnabled;
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas)
        {
            var tasks = contas.Select(conta => Task.Factory.StartNew(() => r_Servico.ConsolidarMovimentacao(conta)));

            return await Task.WhenAll(tasks);
        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}