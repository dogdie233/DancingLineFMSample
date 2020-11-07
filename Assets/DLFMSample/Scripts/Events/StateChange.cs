using Level;
using System.Collections;
using System.Collections.Generic;

namespace Level.Event
{
    public class StateChangeEvent
    {
        public readonly GameState lastState;
        public readonly GameState newState;
        public bool canceled = false;

        public StateChangeEvent(GameState lastState, GameState newState)
        {
            this.lastState = lastState;
            this.newState = newState;
        }
    }
}