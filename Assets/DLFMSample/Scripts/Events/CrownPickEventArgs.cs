using Level;

namespace Event
{
	public class CrownPickEventArgs : EventArgsBase
	{
		public readonly Line line;
		public readonly ICheckpoint checkpoint;

		public CrownPickEventArgs (Line line, ICheckpoint checkpoint)
		{
			this.line = line;
			this.checkpoint = checkpoint;
		}
	}
}