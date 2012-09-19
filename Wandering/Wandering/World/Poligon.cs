using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wandering.World
{
	class Poligon
	{
		public Vector2[] Points;

		private VertexPositionColor[] vertexs;
		public VertexPositionColor[] Vertexs
		{
			get
			{
				if (vertexs == null)
				{
					vertexs = new VertexPositionColor[(Points.Length - 2) * 3];
					for (int i = 0; i < Points.Length - 2; ++i)
					{
						Vertexs[i * 3] = new VertexPositionColor(new Vector3(Points[0].X, Points[0].Y, 0), Color.Black);
						Vertexs[i * 3 + 1] = new VertexPositionColor(new Vector3(Points[i + 1].X, Points[i + 1].Y, 0), Color.Black);
						Vertexs[i * 3 + 2] = new VertexPositionColor(new Vector3(Points[i + 2].X, Points[i + 2].Y, 0), Color.Black);
					}
				}
				return vertexs;
			}
		}

		public int TringleCount
		{
			get
			{
				return Points.Length - 2;
			}
		}
	}
}
