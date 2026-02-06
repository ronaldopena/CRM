using Microsoft.Extensions.Configuration;
using poliview.crm.services;
using System.IO;
using System.Windows.Forms;


namespace poliview.crm.unidades
{
    public partial class FrmPrincipal : Form
    {
        private readonly IConfiguration _configuration;

        public FrmPrincipal()
        {
            InitializeComponent();
            _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listResumo.Items.Add("INICIO DO PROCESSAMENTO");

            listResumo.Items.Clear();
            listInclusao.Items.Clear();
            listExclusao.Items.Clear();
            listScript.Items.Clear();


            var connSql = _configuration["conexaoSqlServer"] ?? string.Empty;
            var connFb = _configuration["conexaoFirebird"] ?? string.Empty;
            listResumo.Items.Add(connSql);
            listResumo.Items.Add(connFb);

            if (chkUnidades.Checked) UnidadesServices.comparar(connFb, connSql, listResumo, listInclusao, listExclusao, listScript);
            if (chkBlocos.Checked) BlocosServices.comparar(connFb, connSql, listResumo, listInclusao, listExclusao, listScript);
            if (chkEmpreendimentos.Checked) EmpreendimentosServices.comparar(connFb, connSql, listResumo, listInclusao, listExclusao, listScript);
            if (chkContratos.Checked) ContratosServices.comparar(connFb, connSql, listResumo, listInclusao, listExclusao, listScript);
            if (chkProponentes.Checked) ProponentesServices.comparar(connFb, connSql, listResumo, listInclusao, listExclusao, listScript);
            if (chkClientes.Checked) ClientesServices.comparar(connFb, connSql, listResumo, listInclusao, listExclusao, listScript);

            listResumo.Items.Add("FIM DO PROCESSAMENTO");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //listScript.Items.
            ExportarParaArquivoTexto();
        }


        private void ExportarParaArquivoTexto()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Arquivos SQL Server (*.sql)|*.sql";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        foreach (var item in listScript.Items)
                        {
                            sw.WriteLine(item.ToString());
                        }
                    }
                }
            }
        }

    }
}