using Level;
using UnityEngine;

namespace Event
{
	public class RespawnEventArgs : EventArgsBase
	{
		public readonly Crown crown;

		public RespawnEventArgs(Crown crown)
		{
			this.crown = crown;
		}
	}
}
