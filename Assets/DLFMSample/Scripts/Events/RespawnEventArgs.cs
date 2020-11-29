using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Event
{
	public class RespawnEventArgs
	{
		public readonly Crown crown;
		public bool canceled = false;

		public RespawnEventArgs(Crown crown)
		{
			this.crown = crown;
		}
	}
}
