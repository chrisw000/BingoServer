"use strict";

var gameConnection = new signalR.HubConnectionBuilder().withUrl("/hub/lobby").build();

//Disable send button until connection is established
document.getElementById("lobbyNewGameButton").disabled = true;
document.getElementById("lobbyJoinGameButton").disabled = true;

gameConnection.on("LobbyNewGameHasStarted", function (user, cheeseCount, gameId) {
    var encodedMsg = user + " started game " + gameId + " with " + cheeseCount + " cheeses";
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

gameConnection.on("LobbyUserJoinedGame", function (user, message, gameId) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " joined game " + gameId + " > " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

gameConnection.on("LobbyPlayerNumbers", function (gameId, playerNumbers) {
    var li = document.createElement("li");
    li.textContent = "My Numbers are " + playerNumbers.toString();
    document.getElementById("messagesList").appendChild(li);
});

gameConnection.on("LobbyPlayerMessage", function (gameId, message) {
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
});

gameConnection.on("LobbyUpdateGame", function (gameId, message) {
    document.getElementById("gamePulseMessage").textContent = message;
});

gameConnection.start().then(function () {
    document.getElementById("lobbyNewGameButton").disabled = false;
    document.getElementById("lobbyJoinGameButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("lobbyNewGameButton").addEventListener("click", function (event) {
    var user = document.getElementById("lobbyUsername").value;
    var cheeseCount = parseInt(document.getElementById("lobbyNewGameCheeseCount").value);
    var numberOfPlayersRequired = 3;
    gameConnection.invoke("ClientStartedNewGame", user, cheeseCount, numberOfPlayersRequired).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("lobbyJoinGameButton").addEventListener("click", function (event) {
    var user = document.getElementById("lobbyUsername").value;
    var gameId = document.getElementById("lobbyJoinGameId").value;
    gameConnection.invoke("ClientJoinedGame", user, gameId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});