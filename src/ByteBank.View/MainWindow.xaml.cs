using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            var contas = r_Repositorio.GetContaClientes();

            var partialContas = new IEnumerable<Core.Model.ContaCliente>[5];

            var count = contas.Count() % partialContas.Length;
            var take = contas.Count() / partialContas.Length;
            var skip = 0;
            for (int i = 0; i < partialContas.Length; i++)
            {
                var contasSkip = contas.Skip(skip);
                if (count > 0)
                {
                    partialContas[i] = contasSkip.Take(take + 1);
                    skip += take + 1;
                    count--;
                }
                else
                {
                    partialContas[i] = contasSkip.Take(take);
                    skip += take;
                }
            }

            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            var threads = new Thread[partialContas.Length];
            for (int i = 0; i < threads.Length; i++)
            {
                int localCount = i;
                threads[i] = new Thread(() =>
                {
                    foreach (var conta in partialContas[localCount])
                    {
                        var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                        resultado.Add(resultadoConta);
                    }
                });
                threads[i].Start();
            }

            //for (int i = 0; i < threads.Length; i++)
            //{
            //    threads[i].Start();
            //}

            while (threads.Where(x => x.IsAlive).Any())
            {
                Thread.Sleep(250);
            }

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);
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