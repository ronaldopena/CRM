using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Chamado
    {
        public int idchamado { get; set; }
        public int idcontrato { get; set; }
		public string? idcontratosp7 { get; set; }
        public string? cliente { get; set; }
        public string? empreendimento { get; set; }
        public string? bloco { get; set; }
        public string? unidade { get; set; }
        public string? dataabertura { get; set; }
        public string? DT_Baixa { get; set; }
        public string? atendimento { get; set; }
		public string? ocorrencia { get; set; }
        public string? arvoreocorrencia { get; set; }
        public string? conclusao { get; set; }
		public string? descricao { get; set; }
        public int idstatus { get; set; }
        public string? status { get; set; }
		public int concluido { get; set; }
        public int naoenviaremails { get; set; }
        public string origem { get; set; }
        public int NovasMensagens { get; set; }
    }
}


public class ChamadoDetalhe
{
	public int idchamado { get; set; }
    public int idocorrencia { get; set; }
	public string? nomecliente { get; set; }
	public string? emailcliente { get; set; }
	public string? empreendimento { get; set; }
	public string? bloco { get; set; }
	public string? unidade { get; set; }
	public string? descricao { get; set; }
	public string? tipoocorrencia { get; set; }
    public string? ddd { get; set; }
	public string? telefone { get; set; }
	public string? celular { get; set; }
    public int idgrupo { get; set; }
    public string? grupo { get; set; }
	public int idatendente { get; set; }
	public string? atendente { get; set; }
    public int encerrado { get; set; }
    public string? statusChamado { get; set; }
    public string? statusOcorrencia { get; set; }
    public string? nomeempresa { get; set; }
    public string? logoempresa { get; set; }
    public int naoenviaremails { get; set; }
}

/*
 "idchamado": 2164,
		"idcontrato": 10,
		"idcontratosp7": "12",
		"empreendimento": "UNIQUE PINHEIROS",
		"bloco": "UNIQUE PINHEIROS",
		"unidade": "102",
		"dataabertura": "21/12/2020 13:28",
		"DT_Baixa": "00000000",
		"atendimento": "",
		"ocorrencia": "Assistência Técnica",
		"conclusao": "",
		"descricao": "teste",
		"idstatus": 19,
		"status": "Aberto e não atendido"
 */