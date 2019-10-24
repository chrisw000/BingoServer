"use strict";

var gameConnection = new signalR.HubConnectionBuilder().withUrl("/hub/lobby").build();

//Disable send button until connection is established
document.getElementById("lobbyNewGameButton").disabled = true;
document.getElementById("lobbyJoinGameButton").disabled = true;

gameConnection.on("LobbyNewGameHasStarted", function (gameData) {
    /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    console.log(gameData);

    addMessageToUIQueue("New Game Started: " + JSON.stringify(gameData));
});

gameConnection.on("LobbyUserJoinedGame", function (gameData, user, message) {
    /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    console.log(gameData);

    addMessageToUIQueue(user + " joined game " + gameData.gameId + " > " + message);
});

gameConnection.on("LobbyPlayerNumbers", function (gameData, playerNumbers) {
     /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    console.log(gameData);

    addMessageToUIQueue("My Numbers are " + playerNumbers.toString());
});

gameConnection.on("LobbyPlayerMessage", function (gameData, message) {
     /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    console.log(gameData);

    addMessageToUIQueue(message);
});

gameConnection.on("LobbyUpdateGame", function (gameData, message) {
     /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    console.log(gameData);
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
    var name = document.getElementById("lobbyNewGameName").value;
    var mode = document.getElementById("lobbyNewGameMode").value;
    var gameSize = document.getElementById("lobbyNewGameSize").value;   
    var cheeseCount = document.getElementById("lobbyNewGameCheeseCount").value;

    var newGame = {
            startedByUser: user,
            name: name,
            mode: mode,
            gameSize: parseInt(gameSize),
            cheeseCount: parseInt(cheeseCount)
        };

    console.log(newGame);

    gameConnection.invoke("ClientStartedNewGame", newGame).catch(function (err) {
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

function addMessageToUIQueue(message) {
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
}
