function exportTableToExcel(filename) {
    var wb = XLSX.utils.table_to_book(document.getElementById("relatorio"), { sheet: "SheetJS" });
    var wbout = XLSX.write(wb, { bookType: "xlsx", type: "binary" });

    function s2ab(s) {
        var buf = new ArrayBuffer(s.length);
        var view = new Uint8Array(buf);
        for (var i = 0; i < s.length; i++) view[i] = s.charCodeAt(i) & 0xff;
        return buf;
    }

    var blob = new Blob([s2ab(wbout)], { type: "application/octet-stream" });
    
    var a = document.createElement("a");
    document.body.appendChild(a);
    a.style = "display: none";
    var url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);    
}