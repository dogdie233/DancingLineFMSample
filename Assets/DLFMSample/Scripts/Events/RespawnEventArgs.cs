using Level;
using UnityEngine;

namespace Event
{
	public class RespawnEventArgs : EventArgsBase
	{
		public readonly ICheckpoint checkpoint;

		public RespawnEventArgs(ICheckpoint checkpoint)
		{
			this.checkpoint = checkpoint;
		}
	}
}
