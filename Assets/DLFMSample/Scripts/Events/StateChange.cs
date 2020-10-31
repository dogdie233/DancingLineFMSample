using Level;
using System.Collections;
using System.Collections.Generic;

namespace Event
{
    public class StateChangeEvent
    {
        public readonly GameState lastState;
        public readonly GameState newState;
        public bool canceled = false;
        public string test;

        public StateChangeEvent(GameState lastState, GameState newState)
        {
            this.lastState = lastState;
            this.newState = newState;
        }
    }
}