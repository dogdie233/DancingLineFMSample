using Level;
using System.Collections;
using System.Collections.Generic;

namespace Event
{
    public class GameOverEventArgs
    {
        public readonly DeathCause deathCause;
        public bool canceled = false;

        public GameOverEventArgs(DeathCause deathCause)
		{
            this.deathCause = deathCause;
		}
    }
}