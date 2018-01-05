using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ByteBank.View.Utils;
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

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            var taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
            BtnProcessar.IsEnabled = false;

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            //var resultado = new List<string>();

            // AtualizarView(new List<string>(), TimeSpan.Zero);
            LimparView();

            var inicio = DateTime.Now;
            var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);
            var resultado = await ConsolidarContas(contas, byteBankProgress);
            var fim = DateTime.Now;
            AtualizarView(resultado, fim - inicio);
            BtnProcessar.IsEnabled = true;

            #region old way
            //var contasTarefas = contas.Select(conta =>
            //{
            //    return Task.Factory.StartNew(() =>
            //    {

            //        var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
            //        resultado.Add(resultadoConta);
            //    });
            //}).ToArray();

            //ConsolidarContas(contas).ContinueWith(task => {
            //        var fim = DateTime.Now;
            //    var resultado = task.Result;
            //        AtualizarView(resultado, fim - inicio);
            //    }, taskSchedulerUI)
            //    .ContinueWith(task =>
            //    {
            //        BtnProcessar.IsEnabled = true;
            //    }, taskSchedulerUI);
            #endregion
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas,IProgress<string>reportadorDeProgresso)
        {
          
            var tasks = contas.Select(conta => Task.Factory.StartNew(() =>
            {
                var resultadoConsolidação = r_Servico.ConsolidarMovimentacao(conta);
                reportadorDeProgresso.Report(resultadoConsolidação);
              
                return resultadoConsolidação;
             })
             
             );
                           
            var resultado = await Task.WhenAll(tasks);

            return resultado;

            #region old way
            //    var resultado = new List<string>();
            //    var tasks = contas.Select(conta => 
            //    {
            //        return Task.Factory.StartNew(()=>
            //        {
            //        var contaResultado = r_Servico.ConsolidarMovimentacao(conta);
            //        resultado.Add(contaResultado);
            //    });
            //});


            //    return Task.WhenAll(tasks).ContinueWith(t=>
            //        {
            //            return resultado;
            //        });
            #endregion
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
            PgsProgresso.Value = 0;
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
