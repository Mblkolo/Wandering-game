using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Wandering.Helpers
{
	struct Segment2
	{
		public Vector2 P1;
		public Vector2 P2;

		public Segment2(Vector2 p1, Vector2 p2)
		{
			P1 = p1;
			P2 = p2;
		}

		public Vector2 GetVector()
		{
			return P2 - P1;
		}
	}
}
