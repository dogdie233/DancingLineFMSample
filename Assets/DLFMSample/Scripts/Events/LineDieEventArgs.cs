using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
	public class LineDieEventArgs
	{
		public readonly Line line;
		public readonly DeathCause cause;
		public bool canceled = false;

		public LineDieEventArgs (Line line, DeathCause cause)
		{
			this.line = line;
			this.cause = cause;
		}
	}
}
