using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
	public class CrownPickedEventArgs
	{
		public readonly Line line;
		public readonly Level.Crown crown;
		public bool canceled = false;

		public CrownPickedEventArgs (Line line, Level.Crown crown)
		{
			this.line = line;
			this.crown = crown;
		}
	}
}