using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
	public struct Coordinate
	{
		public int X;
		public int Y;

		public Coordinate(int x, int y)
		{
			X = x;
			Y = y;
		}
		public bool IsNeighbourWith(Coordinate other)
		{
			if (Math.Abs(X - other.X) + Math.Abs(Y - other.Y) == 1)
				return true;
			return false;
		}
	}
}
