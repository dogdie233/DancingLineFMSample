using Level;

namespace Event
{
    public class StateChangeEventArgs : EventArgsBase
    {
        public readonly GameState lastState;
        public readonly GameState newState;

        public StateChangeEventArgs(GameState lastState, GameState newState)
        {
            this.lastState = lastState;
            this.newState = newState;
        }
    }
}