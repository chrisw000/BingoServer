﻿using BlueCheese.HostedServices.Bingo.Contracts;
using System;

namespace BlueCheese.Hubs
{
    public class NewGameStarted : IHoldUserIdentity
    {
        public string User {get;set;}
        public Guid PlayerId {get;set;}
        public string Name {get;set;}
        public int Mode {get;set;}
        public int Size {get;set;}
        public int CheeseCount {get;set;}

        internal string ConnectionId {get;set;}

        public NewGameStarted()
        {
            // Required for default serialization
        }
    }
}