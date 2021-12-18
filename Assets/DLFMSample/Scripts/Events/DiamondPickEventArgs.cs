using Level;

namespace Event
{
	public class DiamondPickEventArgs : EventArgsBase
	{
		public readonly Line line;
		public readonly Diamond diamond;
		public readonly bool lineEat;

		public DiamondPickEventArgs(Line line, Diamond diamond, bool lineEat)
		{
			this.line = line;
			this.diamond = diamond;
			this.lineEat = lineEat;
		}
	}
}
