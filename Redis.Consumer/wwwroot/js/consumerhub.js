"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/redismessagehub").build();

connection.on("ReceiveMessage", function (time, message) {
    console.log(time + " - " + message);
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = time + " - " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function () {
    console.log("signalr connection established");
}).catch(function (err) {
    return console.error(err.toString());
});