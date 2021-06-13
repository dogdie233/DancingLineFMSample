using Level;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Event
{
    public class SkinChangeEventArgs
    {
        public readonly Line line;
        public readonly Type lastSkin;
        public Type newSkin;
        public bool canceled = false;

        public SkinChangeEventArgs(Line line, Type lastSkin, Type newSkin)
        {
            this.line = line;
            this.lastSkin = lastSkin;
            this.newSkin = newSkin;
        }
    }
}