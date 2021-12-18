using Level;

namespace Event
{
	public class LineDieEventArgs : EventArgsBase
	{
		public readonly Line line;
		public readonly DeathCause cause;

		public LineDieEventArgs (Line line, DeathCause cause)
		{
			this.line = line;
			this.cause = cause;
		}
	}
}
