function exportarRelatorio(nomearquivo) {
    // Seleciona a tabela e cria um objeto Workbook do SheetJS
    var tabela = document.getElementById("relatorio");
    var workbook = XLSX.utils.table_to_book(tabela);

    // Converte o objeto Workbook para o formato XLSX
    var arquivoXLSX = XLSX.write(workbook, { bookType: "xlsx", type: "array" });

    // Cria um Blob com o arquivo XLSX
    var blob = new Blob([arquivoXLSX], { type: "application/octet-stream" });

    // Cria um link para download do arquivo
    var url = URL.createObjectURL(blob);
    var link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", nomearquivo+".xlsx");
    link.style.visibility = "hidden";
    document.body.appendChild(link);

    // Clica no link para iniciar o download
    link.click();

    // Remove o link da página
    document.body.removeChild(link);
}