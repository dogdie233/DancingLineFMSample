using Level;
using UnityEngine;

namespace Event
{
    public class LineTurnEventArgs : EventArgsBase
    {
        public readonly Line line;
        public readonly Vector3 lastWay;
        public Vector3 newWay;
        public bool foucs;

        public LineTurnEventArgs(Line line, Vector3 lastWay, Vector3 newWay, bool foucs)
		{
            this.line = line;
            this.lastWay = lastWay;
            this.newWay = newWay;
            this.foucs = foucs;
		}
    }
}
