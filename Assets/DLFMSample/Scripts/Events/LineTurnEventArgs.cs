using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
    public class LineTurnEventArgs
    {
        public readonly Line line;
        public readonly Vector3 lastway;
        public Vector3 newway;
        public bool foucs;
        public bool canceled;

        public LineTurnEventArgs(Line line, Vector3 lastway, Vector3 newway, bool foucs)
		{
            this.line = line;
            this.lastway = lastway;
            this.newway = newway;
            this.foucs = foucs;
		}
    }
}
