namespace BlueCheese.Hubs
{
    public class NewGameStarted {
        public string StartedByUser {get;set;}
        public string Name {get;set;}
        public string Mode {get;set;}
        public int GameSize {get;set;}
        public int CheeseCount {get;set;}
    }
}