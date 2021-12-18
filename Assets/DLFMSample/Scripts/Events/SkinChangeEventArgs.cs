using Level;
using System;

namespace Event
{
    public class SkinChangeEventArgs : EventArgsBase
    {
        public readonly Line line;
        public readonly Type lastSkin;
        public Type newSkin;

        public SkinChangeEventArgs(Line line, Type lastSkin, Type newSkin)
        {
            this.line = line;
            this.lastSkin = lastSkin;
            this.newSkin = newSkin;
        }
    }
}