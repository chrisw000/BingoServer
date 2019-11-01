"use strict";

const gameConnection = new signalR.HubConnectionBuilder()
                            .withUrl("/hub/lobby")
                            .withAutomaticReconnect([0, 1000, 1000, 2000, 3000, 5000, 8000, 13000, 21000, 34000, null])
                            .configureLogging(signalR.LogLevel.Information)
                            .build();

function Lobby () {
    this.player =  null;
    this.game = null;

    this.newPlayer = function (playerData) {
        this.player = playerData;
        this.showJson("player", playerData);
    };
    this.joinGame = function (gameData) {
        this.game = gameData;
        this.showJson("game", gameData);
    };
    this.updateGame = function (gameData) {
        this.game = gameData;
        this.showJson("game", gameData);
    };
    this.showJson = function (name, obj) {
        console.log(obj);
        document.getElementById(name+"Json").innerHTML = JSON.stringify(obj);
    };
};

let lobby = new Lobby();

//Disable send button until connection is established
document.getElementById("lobbyNewGameButton").disabled = true;
document.getElementById("lobbyJoinGameButton").disabled = true;

gameConnection.on("LobbyNewGameHasStarted", function (gameData) {

    // TODO: maintain the full list of games

    document.getElementById("lobbyJoinGameId").value = gameData.gameId;
    document.getElementById("lobbyJoinGameButton").disabled = !(lobby.player && !lobby.game);

    addMessageToUIQueue("New Game Started: " + gameData.gameId);
});

gameConnection.on("LobbyUserJoinedGame", function (gameData, message) {

    lobby.joinGame(gameData);

    addMessageToUIQueue(user + " joined game " + gameData.gameId + " > " + message);
});

gameConnection.on("LobbyPlayerNumbers", function (gameData, player) {

    lobby.joinGame(gameData);

    document.getElementById("lobbyNewGameButton").disabled = true;
    document.getElementById("lobbyJoinGameButton").disabled = true;

    addMessageToUIQueue("My Player: " + JSON.stringify(player));
});

gameConnection.on("LobbyPlayerMessage", function (gameData, message) {

    lobby.updateGame(gameData);

    addMessageToUIQueue(message);
});

gameConnection.on("LobbyUpdateGame", function (gameData, message) {

    lobby.updateGame(gameData);

    document.getElementById("gamePulseMessage").innerHTML = message;
});

//------------------------------------------------------------------------------------

start().then(function () {
    if (document.getElementById("lobbyJoinGameId").value) {
        document.getElementById("lobbyJoinGameButton").disabled = false;
    } else {
        document.getElementById("lobbyNewGameButton").disabled = false;
    }
}).catch(function (err) {
    return console.error(err.toString());
});

async function start() {
    try {
        await gameConnection.start();
        console.assert(gameConnection.state === signalR.HubConnectionState.Connected);
        console.log("connected");
    } catch (err) {
        console.assert(gameConnection.state === signalR.HubConnectionState.Disconnected);
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};

gameConnection.onreconnecting((error) => {
    console.assert(gameConnection.state === signalR.HubConnectionState.Reconnecting);

    addMessageToUIQueue(`Connection lost due to error "${error}". Reconnecting.`);
});

gameConnection.onreconnected((connectionId) => {
    console.assert(gameConnection.state === signalR.HubConnectionState.Connected);

    addMessageToUIQueue(`Connection reestablished. Connected with connectionId "${connectionId}".`);

    gameConnection.invoke("ClientReconnected", lobby.player).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

});

gameConnection.onclose((error) => {
    console.assert(gameConnection.state === signalR.HubConnectionState.Disconnected);

    addMessageToUIQueue(`Connection closed due to error "${error}". Try refreshing this page to restart the connection.`);
});

//------------------------------------------------------------------------------------

document.getElementById("lobbyNewGameButton").addEventListener("click", function (event) {

    document.getElementById("lobbyNewGameButton").disabled = true;
    document.getElementById("lobbyJoinGameButton").disabled = true;

    var name = document.getElementById("lobbyNewGameName").value;
    var mode = document.getElementById("lobbyNewGameMode").value;
    var size = document.getElementById("lobbyNewGameSize").value;   
    var cheeseCount = document.getElementById("lobbyNewGameCheeseCount").value;

    var newGame = {
            user: lobby.player.user,
            playerId: lobby.player.playerId,
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

    document.getElementById("lobbyNewGameButton").disabled = true;
    document.getElementById("lobbyJoinGameButton").disabled = true;

    var gameId = document.getElementById("lobbyJoinGameId").value;

    var joinGame = {
        user: lobby.player.user,
        playerId: lobby.player.playerId,
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

//------------------------------------------------------------------------------------

document.getElementById("softLogOnButton").addEventListener("click", function (event) {
    var user = document.getElementById("lobbyUsername").value;

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {

          lobby.newPlayer(JSON.parse(this.responseText));

          document.getElementById("softLogOnButton").disabled = true;
          document.getElementById("lobbyUsername").disabled = true;
          document.getElementById("newGateDataEntry").style.display = "block";    
            
          document.getElementById("lobbyJoinGameButton").disabled = !(lobby.player && !lobby.game);
        }
      };
    xhttp.open("POST", "/api/account", true);
    xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xhttp.send("lobbyUsername="+user).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});