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

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnProcessar.IsEnabled = false;

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            var resultado = ConsolidarContas(r_Repositorio.GetContaClientes())
                .ContinueWith(task => AtualizarView(task.Result, DateTime.Now - inicio), taskSchedulerUI)
                .ContinueWith(task => BtnProcessar.IsEnabled = true, taskSchedulerUI);
        }

        private Task<List<string>> ConsolidarContas(IEnumerable<ContaCliente> contas)
        {
            var resultado = new List<string>();

            var contasTasks = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                });
            });

            return Task.WhenAll(contasTasks).ContinueWith(t => resultado);
        }

        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}