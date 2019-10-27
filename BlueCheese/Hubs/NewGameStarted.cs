namespace BlueCheese.Hubs
{
    public class NewGameStarted {
        public string StartedByUser {get;set;}
        public string Name {get;set;}
        public int Mode {get;set;}
        public int Size {get;set;}
        public int CheeseCount {get;set;}
    }
}