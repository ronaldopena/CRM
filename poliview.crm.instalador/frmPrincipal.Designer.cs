namespace poliview.crm.instalador
{
    partial class FrmPrincipal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSalvarConfiguracoes = new Button();
            groupBox2 = new GroupBox();
            label7 = new Label();
            txtPortaAPI = new TextBox();
            label18 = new Label();
            txtNomeCliente = new TextBox();
            txtPastaInstalacaoCRM = new TextBox();
            label6 = new Label();
            label13 = new Label();
            txtAcessoExterno = new TextBox();
            txtLog = new ListBox();
            label35 = new Label();
            txtConnectionString = new TextBox();
            button2 = new Button();
            lblVersaoSQL = new Label();
            lblconexaoSQL = new Label();
            button1 = new Button();
            lblConexaoFB = new Label();
            txtConnectionStringFB = new TextBox();
            button3 = new Button();
            txtServidorSqlServer = new TextBox();
            txtBancoSqlServer = new TextBox();
            label1 = new Label();
            txtUsuarioSqlServer = new TextBox();
            label3 = new Label();
            txtSenhaSqlServer = new TextBox();
            label4 = new Label();
            txtSenhaFirebird = new TextBox();
            label5 = new Label();
            txtUsuarioFirebird = new TextBox();
            label8 = new Label();
            txtBancoFirebird = new TextBox();
            label9 = new Label();
            txtServidorFirebird = new TextBox();
            label10 = new Label();
            txtPortaFirebird = new TextBox();
            label2 = new Label();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnSalvarConfiguracoes
            // 
            btnSalvarConfiguracoes.Location = new Point(465, 697);
            btnSalvarConfiguracoes.Name = "btnSalvarConfiguracoes";
            btnSalvarConfiguracoes.Size = new Size(174, 23);
            btnSalvarConfiguracoes.TabIndex = 4;
            btnSalvarConfiguracoes.Text = "SALVAR CONFIGURAÇÕES";
            btnSalvarConfiguracoes.UseVisualStyleBackColor = true;
            btnSalvarConfiguracoes.Click += btnSalvarConfiguracoes_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(txtPortaAPI);
            groupBox2.Controls.Add(label18);
            groupBox2.Controls.Add(txtNomeCliente);
            groupBox2.Controls.Add(txtPastaInstalacaoCRM);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label13);
            groupBox2.Controls.Add(txtAcessoExterno);
            groupBox2.Location = new Point(14, 111);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1087, 155);
            groupBox2.TabIndex = 45;
            groupBox2.TabStop = false;
            groupBox2.Text = " Dados ";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(59, 81);
            label7.Name = "label7";
            label7.Size = new Size(56, 15);
            label7.TabIndex = 67;
            label7.Text = "Porta API";
            // 
            // txtPortaAPI
            // 
            txtPortaAPI.Location = new Point(124, 78);
            txtPortaAPI.Name = "txtPortaAPI";
            txtPortaAPI.Size = new Size(62, 23);
            txtPortaAPI.TabIndex = 66;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(29, 110);
            label18.Name = "label18";
            label18.Size = new Size(89, 15);
            label18.TabIndex = 63;
            label18.Text = "Instalação CRM";
            // 
            // txtNomeCliente
            // 
            txtNomeCliente.Location = new Point(124, 22);
            txtNomeCliente.Name = "txtNomeCliente";
            txtNomeCliente.Size = new Size(296, 23);
            txtNomeCliente.TabIndex = 58;
            // 
            // txtPastaInstalacaoCRM
            // 
            txtPastaInstalacaoCRM.Location = new Point(124, 108);
            txtPastaInstalacaoCRM.Name = "txtPastaInstalacaoCRM";
            txtPastaInstalacaoCRM.Size = new Size(296, 23);
            txtPastaInstalacaoCRM.TabIndex = 62;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(34, 26);
            label6.Name = "label6";
            label6.Size = new Size(80, 15);
            label6.TabIndex = 59;
            label6.Text = "Nome Cliente";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(31, 51);
            label13.Name = "label13";
            label13.Size = new Size(86, 15);
            label13.TabIndex = 61;
            label13.Text = "Acesso Externo";
            // 
            // txtAcessoExterno
            // 
            txtAcessoExterno.Location = new Point(124, 49);
            txtAcessoExterno.Name = "txtAcessoExterno";
            txtAcessoExterno.Size = new Size(296, 23);
            txtAcessoExterno.TabIndex = 60;
            // 
            // txtLog
            // 
            txtLog.AccessibleRole = AccessibleRole.None;
            txtLog.FormattingEnabled = true;
            txtLog.ItemHeight = 15;
            txtLog.Location = new Point(17, 380);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(1079, 304);
            txtLog.TabIndex = 46;
            txtLog.SelectedIndexChanged += txtLog_SelectedIndexChanged;
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Location = new Point(11, 9);
            label35.Name = "label35";
            label35.Size = new Size(109, 15);
            label35.TabIndex = 49;
            label35.Text = "Servidor SQL Server";
            // 
            // txtConnectionString
            // 
            txtConnectionString.Location = new Point(14, 81);
            txtConnectionString.Name = "txtConnectionString";
            txtConnectionString.ReadOnly = true;
            txtConnectionString.Size = new Size(999, 23);
            txtConnectionString.TabIndex = 48;
            txtConnectionString.Text = "Server=G15\\SQLEXPRESS;Database=ELEMENTO;User Id=sa;Password=master; Connect Timeout=600; TrustServerCertificate=True;";
            // 
            // button2
            // 
            button2.Location = new Point(1017, 27);
            button2.Name = "button2";
            button2.Size = new Size(82, 23);
            button2.TabIndex = 50;
            button2.Text = "Conectar";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // lblVersaoSQL
            // 
            lblVersaoSQL.AutoSize = true;
            lblVersaoSQL.BackColor = Color.Gray;
            lblVersaoSQL.ForeColor = Color.White;
            lblVersaoSQL.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Assertive;
            lblVersaoSQL.Location = new Point(112, 53);
            lblVersaoSQL.Name = "lblVersaoSQL";
            lblVersaoSQL.Padding = new Padding(5);
            lblVersaoSQL.Size = new Size(107, 25);
            lblVersaoSQL.TabIndex = 65;
            lblVersaoSQL.Text = "Sem informação ";
            // 
            // lblconexaoSQL
            // 
            lblconexaoSQL.AutoSize = true;
            lblconexaoSQL.BackColor = Color.Gray;
            lblconexaoSQL.ForeColor = Color.White;
            lblconexaoSQL.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Assertive;
            lblconexaoSQL.Location = new Point(15, 53);
            lblconexaoSQL.Name = "lblconexaoSQL";
            lblconexaoSQL.Padding = new Padding(5);
            lblconexaoSQL.Size = new Size(92, 25);
            lblconexaoSQL.TabIndex = 64;
            lblconexaoSQL.Text = "Desconectado";
            // 
            // button1
            // 
            button1.Location = new Point(226, 53);
            button1.Name = "button1";
            button1.Size = new Size(193, 28);
            button1.TabIndex = 66;
            button1.Text = "Atualizar Base de dados";
            button1.UseVisualStyleBackColor = true;
            button1.Visible = false;
            button1.Click += button1_Click;
            // 
            // lblConexaoFB
            // 
            lblConexaoFB.AutoSize = true;
            lblConexaoFB.BackColor = Color.Gray;
            lblConexaoFB.ForeColor = Color.White;
            lblConexaoFB.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Assertive;
            lblConexaoFB.Location = new Point(17, 323);
            lblConexaoFB.Name = "lblConexaoFB";
            lblConexaoFB.Padding = new Padding(5);
            lblConexaoFB.Size = new Size(92, 25);
            lblConexaoFB.TabIndex = 69;
            lblConexaoFB.Text = "Desconectado";
            // 
            // txtConnectionStringFB
            // 
            txtConnectionStringFB.Location = new Point(17, 351);
            txtConnectionStringFB.Name = "txtConnectionStringFB";
            txtConnectionStringFB.ReadOnly = true;
            txtConnectionStringFB.Size = new Size(999, 23);
            txtConnectionStringFB.TabIndex = 67;
            txtConnectionStringFB.Text = "User=SYSDBA;Password=masterkey;Database=C:\\Poliview\\BancoDeDados\\ELEMENTO.PLV;DataSource=localhost;Port=3050;Dialect=3;PacketSize=4096;Pooling=false;";
            // 
            // button3
            // 
            button3.Location = new Point(1021, 297);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 70;
            button3.Text = "Conectar";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click_1;
            // 
            // txtServidorSqlServer
            // 
            txtServidorSqlServer.Location = new Point(12, 27);
            txtServidorSqlServer.Name = "txtServidorSqlServer";
            txtServidorSqlServer.Size = new Size(207, 23);
            txtServidorSqlServer.TabIndex = 71;
            txtServidorSqlServer.Leave += txtServidorSqlServer_Leave;
            // 
            // txtBancoSqlServer
            // 
            txtBancoSqlServer.Location = new Point(225, 27);
            txtBancoSqlServer.Name = "txtBancoSqlServer";
            txtBancoSqlServer.Size = new Size(196, 23);
            txtBancoSqlServer.TabIndex = 73;
            txtBancoSqlServer.Leave += txtServidorSqlServer_Leave;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(224, 11);
            label1.Name = "label1";
            label1.Size = new Size(99, 15);
            label1.TabIndex = 72;
            label1.Text = "Banco SQL Server";
            // 
            // txtUsuarioSqlServer
            // 
            txtUsuarioSqlServer.Location = new Point(427, 27);
            txtUsuarioSqlServer.Name = "txtUsuarioSqlServer";
            txtUsuarioSqlServer.Size = new Size(196, 23);
            txtUsuarioSqlServer.TabIndex = 75;
            txtUsuarioSqlServer.Leave += txtServidorSqlServer_Leave;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(426, 10);
            label3.Name = "label3";
            label3.Size = new Size(106, 15);
            label3.TabIndex = 74;
            label3.Text = "Usuário SQL Server";
            // 
            // txtSenhaSqlServer
            // 
            txtSenhaSqlServer.Location = new Point(629, 27);
            txtSenhaSqlServer.Name = "txtSenhaSqlServer";
            txtSenhaSqlServer.Size = new Size(196, 23);
            txtSenhaSqlServer.TabIndex = 77;
            txtSenhaSqlServer.Leave += txtServidorSqlServer_Leave;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(628, 10);
            label4.Name = "label4";
            label4.Size = new Size(98, 15);
            label4.TabIndex = 76;
            label4.Text = "Senha SQL Server";
            // 
            // txtSenhaFirebird
            // 
            txtSenhaFirebird.Location = new Point(735, 296);
            txtSenhaFirebird.Name = "txtSenhaFirebird";
            txtSenhaFirebird.Size = new Size(196, 23);
            txtSenhaFirebird.TabIndex = 85;
            txtSenhaFirebird.Leave += txtServidorFirebird_Leave;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(734, 278);
            label5.Name = "label5";
            label5.Size = new Size(80, 15);
            label5.TabIndex = 84;
            label5.Text = "Senha firebird";
            // 
            // txtUsuarioFirebird
            // 
            txtUsuarioFirebird.Location = new Point(533, 296);
            txtUsuarioFirebird.Name = "txtUsuarioFirebird";
            txtUsuarioFirebird.Size = new Size(196, 23);
            txtUsuarioFirebird.TabIndex = 83;
            txtUsuarioFirebird.Leave += txtServidorFirebird_Leave;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(532, 278);
            label8.Name = "label8";
            label8.Size = new Size(90, 15);
            label8.TabIndex = 82;
            label8.Text = "Usuário Firebird";
            // 
            // txtBancoFirebird
            // 
            txtBancoFirebird.Location = new Point(227, 295);
            txtBancoFirebird.Name = "txtBancoFirebird";
            txtBancoFirebird.Size = new Size(300, 23);
            txtBancoFirebird.TabIndex = 81;
            txtBancoFirebird.Leave += txtServidorFirebird_Leave;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(226, 279);
            label9.Name = "label9";
            label9.Size = new Size(83, 15);
            label9.TabIndex = 80;
            label9.Text = "Banco Firebird";
            // 
            // txtServidorFirebird
            // 
            txtServidorFirebird.Location = new Point(14, 295);
            txtServidorFirebird.Name = "txtServidorFirebird";
            txtServidorFirebird.Size = new Size(207, 23);
            txtServidorFirebird.TabIndex = 79;
            txtServidorFirebird.Leave += txtServidorFirebird_Leave;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(13, 277);
            label10.Name = "label10";
            label10.Size = new Size(93, 15);
            label10.TabIndex = 78;
            label10.Text = "Servidor Firebird";
            // 
            // txtPortaFirebird
            // 
            txtPortaFirebird.Location = new Point(937, 295);
            txtPortaFirebird.Name = "txtPortaFirebird";
            txtPortaFirebird.Size = new Size(79, 23);
            txtPortaFirebird.TabIndex = 87;
            txtPortaFirebird.Leave += txtServidorFirebird_Leave;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(936, 277);
            label2.Name = "label2";
            label2.Size = new Size(76, 15);
            label2.TabIndex = 86;
            label2.Text = "Porta firebird";
            // 
            // FrmPrincipal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1110, 732);
            Controls.Add(txtPortaFirebird);
            Controls.Add(label2);
            Controls.Add(txtSenhaFirebird);
            Controls.Add(label5);
            Controls.Add(txtUsuarioFirebird);
            Controls.Add(label8);
            Controls.Add(txtBancoFirebird);
            Controls.Add(label9);
            Controls.Add(txtServidorFirebird);
            Controls.Add(label10);
            Controls.Add(txtSenhaSqlServer);
            Controls.Add(label4);
            Controls.Add(txtUsuarioSqlServer);
            Controls.Add(label3);
            Controls.Add(txtBancoSqlServer);
            Controls.Add(label1);
            Controls.Add(txtServidorSqlServer);
            Controls.Add(button3);
            Controls.Add(lblConexaoFB);
            Controls.Add(txtConnectionStringFB);
            Controls.Add(button1);
            Controls.Add(lblVersaoSQL);
            Controls.Add(lblconexaoSQL);
            Controls.Add(button2);
            Controls.Add(label35);
            Controls.Add(txtConnectionString);
            Controls.Add(txtLog);
            Controls.Add(groupBox2);
            Controls.Add(btnSalvarConfiguracoes);
            Name = "FrmPrincipal";
            Text = "Instalador Poliview CRM - versão 4.3.0";
            Load += FrmPrincipal_Load;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnSalvarConfiguracoes;
        private GroupBox groupBox2;
        private ListBox txtLog;
        private Label label18;
        private TextBox txtNomeCliente;
        private TextBox txtPastaInstalacaoCRM;
        private Label label6;
        private Label label13;
        private TextBox txtAcessoExterno;
        private Label label35;
        private TextBox txtConnectionString;
        private Button button2;
        private Label label7;
        private TextBox txtPortaAPI;
        private Label lblVersaoSQL;
        private Label lblconexaoSQL;
        private Button button1;
        private Label lblConexaoFB;
        private TextBox txtConnectionStringFB;
        private Button button3;
        private TextBox txtServidorSqlServer;
        private TextBox txtBancoSqlServer;
        private Label label1;
        private TextBox txtUsuarioSqlServer;
        private Label label3;
        private TextBox txtSenhaSqlServer;
        private Label label4;
        private TextBox txtSenhaFirebird;
        private Label label5;
        private TextBox txtUsuarioFirebird;
        private Label label8;
        private TextBox txtBancoFirebird;
        private Label label9;
        private TextBox txtServidorFirebird;
        private Label label10;
        private TextBox txtPortaFirebird;
        private Label label2;
    }
}