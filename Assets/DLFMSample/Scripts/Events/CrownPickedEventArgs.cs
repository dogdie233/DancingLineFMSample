using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
	public class CrownPickedEventArgs
	{
		public readonly Line line;
		public readonly Crown crown;
		public bool canceled = false;

		public CrownPickedEventArgs (Line line, Crown crown)
		{
			this.line = line;
			this.crown = crown;
		}
	}
}