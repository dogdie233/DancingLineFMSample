using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
	public class DiamondPickedEventArgs
	{
		public readonly Line line;
		public readonly Diamond diamond;
		public readonly bool lineEat;
		public bool canceled = false;

		public DiamondPickedEventArgs(Line line, Diamond diamond, bool lineEat)
		{
			this.line = line;
			this.diamond = diamond;
			this.lineEat = lineEat;
		}
	}
}
