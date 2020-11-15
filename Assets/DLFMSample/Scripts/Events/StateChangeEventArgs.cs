using Level;
using System.Collections;
using System.Collections.Generic;

namespace Event
{
    public class StateChangeEventArgs
    {
        public readonly GameState lastState;
        public readonly GameState newState;
        public bool canceled = false;

        public StateChangeEventArgs(GameState lastState, GameState newState)
        {
            this.lastState = lastState;
            this.newState = newState;
        }
    }
}