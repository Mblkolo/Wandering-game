using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Wandering.Helpers
{
	static class Vector
	{
		public static Vector2 ToVector2(this Vector3 vector)
		{
			return new Vector2(vector.X, vector.Y);
		}
	}
}
