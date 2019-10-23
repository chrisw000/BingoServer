## BingoServer

# Docker for Windows
- Install it

# Sql Server
- Run the SQL Script in ./Scripts/SQL/00_Create_Database.sql
- Edit the connection string in appsettings.json -> probably need to remove/correct the SQL instance name

# To See how to hookup to the React App
- Goto the Lobby page...
- Open 2nd Incognito window on Lobby page
- Window 1: add a username, Click "Create New Game"
- Window 2: add a username, copy/paste gameId, Click "Join Game"
- To hook up to React App...
- See BigCheese/Views/Lobby/Index.cshtml
- See BigCheese/wwwroot/js/lobby.js
- requires BigCheese/wwwroot/js/dist/browser/signalr.js
- There is a TypeScript version according to microsoft docs
