using Level;

namespace Event
{
	public class CrownPickEventArgs : EventArgsBase
	{
		public readonly Line line;
		public readonly Crown crown;

		public CrownPickEventArgs (Line line, Crown crown)
		{
			this.line = line;
			this.crown = crown;
		}
	}
}