using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
    public class LineTurnEventArgs
    {
        public readonly Line line;
        public readonly Vector3 lastWay;
        public Vector3 newWay;
        public bool foucs;
        public bool canceled;

        public LineTurnEventArgs(Line line, Vector3 lastWay, Vector3 newWay, bool foucs)
		{
            this.line = line;
            this.lastWay = lastWay;
            this.newWay = newWay;
            this.foucs = foucs;
		}
    }
}
