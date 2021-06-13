using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
	public class RespawnEventArgs
	{
		public readonly Level.Crown crown;
		public bool canceled = false;

		public RespawnEventArgs(Level.Crown crown)
		{
			this.crown = crown;
		}
	}
}
