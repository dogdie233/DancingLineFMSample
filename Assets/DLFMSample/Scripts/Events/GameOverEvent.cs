using Level;
using System.Collections;
using System.Collections.Generic;

namespace Level.Event
{
    public class GameOverEvent
    {
        public readonly DeathCause deathCause;
        public bool canceled = false;

        public GameOverEvent(DeathCause deathCause)
		{
            this.deathCause = deathCause;
		}
    }
}