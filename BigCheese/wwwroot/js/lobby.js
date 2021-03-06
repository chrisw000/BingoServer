﻿"use strict";

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

    lobby.updateGame(gameData);
    addMessageToUIQueue(message);

});

gameConnection.on("LobbyPlayerNumbers", function (gameData, player) {

    lobby.joinGame(gameData);
    
    if (lobby.game.mode == 2) {
        addGameMode2PushSelectionEvents();
    }

    document.getElementById("lobbyNewGameButton").disabled = true;
    document.getElementById("lobbyJoinGameButton").disabled = true;

    document.getElementById("playerJson").innerHTML = JSON.stringify(player);
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

    gameConnection.invoke("ClientJoinedGame", joinGame)
        .catch(function (err) {
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

function addGameMode2PushSelectionEvents() {

    document.getElementById("gameMode2DataEntry").style.display = "block";
    
    for (var i = 1; i <= 42; i++) {
        document.getElementById("game_mode2_" + i).addEventListener("click", clientPushSelectionEvent.bind(this, i), false);
    }
}

function clientPushSelectionEvent(index) {

    var pushSelection = {
        draw: index,
        gameId: lobby.game.gameId
    };

    console.log('clientPushSelectionEvent: ' + JSON.stringify(pushSelection));

    gameConnection.invoke("ClientPushSelection", pushSelection).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

//------------------------------------------------------------------------------------

document.getElementById("softLogOnButton").addEventListener("click", function (event) {
    var username = document.getElementById("lobbyUsername").value;
    if(username=="") return;

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {

          lobby.newPlayer(JSON.parse(this.responseText));

        if (lobby.player.user != username) {
            console.warn("restoring original player from session: " + lobby.player.user);
            document.getElementById("lobbyUsername").value = lobby.player.user;
        }

          document.getElementById("softLogOnButton").disabled = true;
          document.getElementById("lobbyUsername").disabled = true;
          document.getElementById("newGameDataEntry").style.display = "block";  
          document.getElementById("chatDataEntry").style.display = "block"; 
            
          document.getElementById("lobbyJoinGameButton").disabled = !(lobby.player && !lobby.game);
        }
      };
    xhttp.open("POST", "/api/account", true);
    xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xhttp.send("username="+username).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//------------------------------------------------------------------------------------
// Chat
//------------------------------------------------------------------------------------

gameConnection.on("ReceiveChatMessage", function (fromUserIdentity, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = fromUserIdentity.user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("chatList").appendChild(li);
});

document.getElementById("sendDirectButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;

    // Hardcoded to send to player slot [1] in game
    var toUser = lobby.game.players[1].info.user; // TODO this info object will be removed again...
    alert(toUser);

    gameConnection.invoke("SendDirectMessage", toUser, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("sendGameButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    gameConnection.invoke("SendGameMessage", lobby.game.gameId, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("sendGlobalButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    gameConnection.invoke("SendGlobalMessage", message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
