"use strict";

var gameConnection = new signalR.HubConnectionBuilder().withUrl("/hub/lobby").build();

//Disable send button until connection is established
document.getElementById("lobbyNewGameButton").disabled = true;
document.getElementById("lobbyJoinGameButton").disabled = true;

gameConnection.on("LobbyNewGameHasStarted", function (gameData) {
    /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    showJson(gameData);
    document.getElementById("lobbyJoinGameId").value = gameData.gameId;
    addMessageToUIQueue("New Game Started: " + gameData.gameId);
});

gameConnection.on("LobbyUserJoinedGame", function (gameData, message) {
    /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    showJson(gameData);

    addMessageToUIQueue(user + " joined game " + gameData.gameId + " > " + message);
});

gameConnection.on("LobbyPlayerNumbers", function (gameData, player) {
     /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    showJson(gameData);
    console.log(player);

    addMessageToUIQueue("My Player: " + JSON.stringify(player));
});

gameConnection.on("LobbyPlayerMessage", function (gameData, message) {
     /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    showJson(gameData);

    addMessageToUIQueue(message);
});

gameConnection.on("LobbyUpdateGame", function (gameData, message) {
     /*
     * gameData is properties as per BlueCheese.HostedServices.Bingo.IGameData
     */
    showJson(gameData);
    document.getElementById("gamePulseMessage").innerHTML = message;
});

gameConnection.start().then(function () {
    document.getElementById("lobbyNewGameButton").disabled = false;
    document.getElementById("lobbyJoinGameButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("lobbyNewGameButton").addEventListener("click", function (event) {

    var user = document.getElementById("lobbyUsername").value;
    var playerId = document.getElementById("playerId").value;
    var name = document.getElementById("lobbyNewGameName").value;
    var mode = document.getElementById("lobbyNewGameMode").value;
    var size = document.getElementById("lobbyNewGameSize").value;   
    var cheeseCount = document.getElementById("lobbyNewGameCheeseCount").value;

    var newGame = {
            user: user,
            playerId: playerId,
            name: name,
            mode: parseInt(mode),
            size: parseInt(size),
            cheeseCount: parseInt(cheeseCount)
        };

    console.log(newGame);

    gameConnection.invoke("ClientStartedNewGame", newGame).then(function (game) {
        console.log('New game created', game);
    }).catch(function (err) {
        return console.error(err.toString());
    });
    
    event.preventDefault();
});

document.getElementById("lobbyJoinGameButton").addEventListener("click", function (event) {
    var user = document.getElementById("lobbyUsername").value;
    var playerId = document.getElementById("playerId").value;
    var gameId = document.getElementById("lobbyJoinGameId").value;

    var joinGame = {
        user: user,
        playerId: playerId,
        gameId: gameId
    };

    gameConnection.invoke("ClientJoinedGame", joinGame).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

function addMessageToUIQueue(message) {
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
}
function showJson(gameData) {
    console.log(gameData);
    document.getElementById("gameJson").innerHTML = JSON.stringify(gameData);
}

//------------------------------------------------------------------------------------

document.getElementById("softLogOnButton").addEventListener("click", function (event) {
    var user = document.getElementById("lobbyUsername").value;

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {
          document.getElementById("playerId").value = JSON.parse(this.responseText).playerId;
          document.getElementById("softLogOnButton").disabled = true;
        }
      };
    xhttp.open("POST", "/api/account", true);
    xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xhttp.send("lobbyUsername="+user).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});