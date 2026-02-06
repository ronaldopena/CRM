function exportReportToExcel(tableid, nomearquivo) {
    nomearquivo = nomearquivo + ".xlsx";
    let table = document.getElementById(tableid);
    TableToExcel.convert(table[0], {
        name: nomearquivo,
        sheet: {
            name: 'Planilha1'
        }
    });
}