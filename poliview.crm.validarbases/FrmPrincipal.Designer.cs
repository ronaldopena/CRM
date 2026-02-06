namespace poliview.crm.unidades
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
            this.button1 = new System.Windows.Forms.Button();
            this.gridCrm = new System.Windows.Forms.DataGridView();
            this.gridSiecon = new System.Windows.Forms.DataGridView();
            this.listInclusao = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listExclusao = new System.Windows.Forms.ListBox();
            this.listResumo = new System.Windows.Forms.ListBox();
            this.chkUnidades = new System.Windows.Forms.CheckBox();
            this.chkProponentes = new System.Windows.Forms.CheckBox();
            this.chkContratos = new System.Windows.Forms.CheckBox();
            this.chkEmpreendimentos = new System.Windows.Forms.CheckBox();
            this.chkBlocos = new System.Windows.Forms.CheckBox();
            this.listScript = new System.Windows.Forms.ListBox();
            this.btnExportarScript = new System.Windows.Forms.Button();
            this.chkClientes = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.gridCrm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridSiecon)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(214, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "VERIFICAR BASES";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // gridCrm
            // 
            this.gridCrm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridCrm.Location = new System.Drawing.Point(705, 195);
            this.gridCrm.Name = "gridCrm";
            this.gridCrm.RowTemplate.Height = 25;
            this.gridCrm.Size = new System.Drawing.Size(174, 31);
            this.gridCrm.TabIndex = 1;
            // 
            // gridSiecon
            // 
            this.gridSiecon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridSiecon.Location = new System.Drawing.Point(705, 356);
            this.gridSiecon.Name = "gridSiecon";
            this.gridSiecon.RowTemplate.Height = 25;
            this.gridSiecon.Size = new System.Drawing.Size(174, 58);
            this.gridSiecon.TabIndex = 2;
            // 
            // listInclusao
            // 
            this.listInclusao.FormattingEnabled = true;
            this.listInclusao.ItemHeight = 15;
            this.listInclusao.Location = new System.Drawing.Point(581, 63);
            this.listInclusao.Name = "listInclusao";
            this.listInclusao.Size = new System.Drawing.Size(672, 184);
            this.listInclusao.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(582, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(253, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "INCLUSÃO: registros não encontrados no CRM";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(582, 263);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "EXCLUSÃO: registros não encontrados no SIECON";
            // 
            // listExclusao
            // 
            this.listExclusao.FormattingEnabled = true;
            this.listExclusao.ItemHeight = 15;
            this.listExclusao.Location = new System.Drawing.Point(581, 283);
            this.listExclusao.Name = "listExclusao";
            this.listExclusao.Size = new System.Drawing.Size(672, 169);
            this.listExclusao.TabIndex = 5;
            // 
            // listResumo
            // 
            this.listResumo.FormattingEnabled = true;
            this.listResumo.ItemHeight = 15;
            this.listResumo.Location = new System.Drawing.Point(12, 73);
            this.listResumo.Name = "listResumo";
            this.listResumo.Size = new System.Drawing.Size(546, 379);
            this.listResumo.TabIndex = 7;
            // 
            // chkUnidades
            // 
            this.chkUnidades.AutoSize = true;
            this.chkUnidades.Location = new System.Drawing.Point(265, 2);
            this.chkUnidades.Name = "chkUnidades";
            this.chkUnidades.Size = new System.Drawing.Size(75, 19);
            this.chkUnidades.TabIndex = 8;
            this.chkUnidades.Text = "Unidades";
            this.chkUnidades.UseVisualStyleBackColor = true;
            // 
            // chkProponentes
            // 
            this.chkProponentes.AutoSize = true;
            this.chkProponentes.Location = new System.Drawing.Point(265, 22);
            this.chkProponentes.Name = "chkProponentes";
            this.chkProponentes.Size = new System.Drawing.Size(93, 19);
            this.chkProponentes.TabIndex = 9;
            this.chkProponentes.Text = "Proponentes";
            this.chkProponentes.UseVisualStyleBackColor = true;
            // 
            // chkContratos
            // 
            this.chkContratos.AutoSize = true;
            this.chkContratos.Location = new System.Drawing.Point(265, 42);
            this.chkContratos.Name = "chkContratos";
            this.chkContratos.Size = new System.Drawing.Size(78, 19);
            this.chkContratos.TabIndex = 10;
            this.chkContratos.Text = "Contratos";
            this.chkContratos.UseVisualStyleBackColor = true;
            // 
            // chkEmpreendimentos
            // 
            this.chkEmpreendimentos.AutoSize = true;
            this.chkEmpreendimentos.Location = new System.Drawing.Point(388, 2);
            this.chkEmpreendimentos.Name = "chkEmpreendimentos";
            this.chkEmpreendimentos.Size = new System.Drawing.Size(123, 19);
            this.chkEmpreendimentos.TabIndex = 11;
            this.chkEmpreendimentos.Text = "Empreendimentos";
            this.chkEmpreendimentos.UseVisualStyleBackColor = true;
            // 
            // chkBlocos
            // 
            this.chkBlocos.AutoSize = true;
            this.chkBlocos.Location = new System.Drawing.Point(388, 22);
            this.chkBlocos.Name = "chkBlocos";
            this.chkBlocos.Size = new System.Drawing.Size(61, 19);
            this.chkBlocos.TabIndex = 12;
            this.chkBlocos.Text = "Blocos";
            this.chkBlocos.UseVisualStyleBackColor = true;
            // 
            // listScript
            // 
            this.listScript.FormattingEnabled = true;
            this.listScript.ItemHeight = 15;
            this.listScript.Location = new System.Drawing.Point(12, 471);
            this.listScript.Name = "listScript";
            this.listScript.Size = new System.Drawing.Size(1241, 169);
            this.listScript.TabIndex = 13;
            // 
            // btnExportarScript
            // 
            this.btnExportarScript.Location = new System.Drawing.Point(14, 651);
            this.btnExportarScript.Name = "btnExportarScript";
            this.btnExportarScript.Size = new System.Drawing.Size(130, 23);
            this.btnExportarScript.TabIndex = 14;
            this.btnExportarScript.Text = "Exportar Script";
            this.btnExportarScript.UseVisualStyleBackColor = true;
            this.btnExportarScript.Click += new System.EventHandler(this.button2_Click);
            // 
            // chkClientes
            // 
            this.chkClientes.AutoSize = true;
            this.chkClientes.Location = new System.Drawing.Point(388, 44);
            this.chkClientes.Name = "chkClientes";
            this.chkClientes.Size = new System.Drawing.Size(68, 19);
            this.chkClientes.TabIndex = 15;
            this.chkClientes.Text = "Clientes";
            this.chkClientes.UseVisualStyleBackColor = true;
            // 
            // FrmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1316, 689);
            this.Controls.Add(this.chkClientes);
            this.Controls.Add(this.btnExportarScript);
            this.Controls.Add(this.listScript);
            this.Controls.Add(this.chkBlocos);
            this.Controls.Add(this.chkEmpreendimentos);
            this.Controls.Add(this.chkContratos);
            this.Controls.Add(this.chkProponentes);
            this.Controls.Add(this.chkUnidades);
            this.Controls.Add(this.listResumo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listExclusao);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listInclusao);
            this.Controls.Add(this.gridSiecon);
            this.Controls.Add(this.gridCrm);
            this.Controls.Add(this.button1);
            this.Name = "FrmPrincipal";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.gridCrm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridSiecon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button button1;
        private DataGridView gridCrm;
        private DataGridView gridSiecon;
        private ListBox listInclusao;
        private Label label1;
        private Label label2;
        private ListBox listExclusao;
        private ListBox listResumo;
        private CheckBox chkUnidades;
        private CheckBox chkProponentes;
        private CheckBox chkContratos;
        private CheckBox chkEmpreendimentos;
        private CheckBox chkBlocos;
        private ListBox listScript;
        private Button btnExportarScript;
        private CheckBox chkClientes;
    }
}