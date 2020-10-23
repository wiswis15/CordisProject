"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (userMessages) {
    var list = userMessages;        

    var myTableDiv = document.getElementById("datatable")
    myTableDiv.innerHTML = "";
    var table = document.createElement('TABLE');
    var tablebody = document.createElement('TBODY');

    table.border = '1'
    table.appendChild(tablebody);

    var heading = new Array();
    heading[0] = "Path"
    heading[1] = "Value"
    heading[2] = "LastUpdated"

    var tr = document.createElement('TR');
    tablebody.appendChild(tr);
    for (i = 0; i < heading.length; i++) {
        var th = document.createElement('TH')
        th.width = '75';
        th.appendChild(document.createTextNode(heading[i]));
        tr.appendChild(th);
    }


    for (var i = 0; i < list.length; i++) {
        var tr = document.createElement('TR');        
        var namecol = document.createElement('TD');
        namecol.appendChild(document.createTextNode(list[i].variablePath));
        tr.appendChild(namecol);

        var testcol = document.createElement('TD')
        testcol.appendChild(document.createTextNode(list[i].currentValue));
        tr.appendChild(testcol)

        var timecol = document.createElement('TD')
        timecol.appendChild(document.createTextNode(list[i].lastUpdated));
        tr.appendChild(timecol)
        
        tablebody.appendChild(tr);
    }
    myTableDiv.appendChild(table);

});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    connection.invoke("SendMessage").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});