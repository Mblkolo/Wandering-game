using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wandering.World
{
	class Teleport
	{
		public Gate GateA;
		public Gate GateB;

		public float lenght;
	}

	class Gate
	{
		public Vector2 Center;
		//Угол в градусах
		public float Angle;

		public Teleport Teleport;

		public Gate Pair;

		private VertexPositionColor[] vertexes;
		public VertexPositionColor[] Vertexes
		{
			get
			{
				if (vertexes == null)
				{
					vertexes = new VertexPositionColor[2];
					vertexes[0] = new VertexPositionColor(new Vector3(-Teleport.lenght / 2, 0, 0), Color.Red);
					vertexes[1] = new VertexPositionColor(new Vector3(Teleport.lenght / 2, 0, 0), Color.Red);

					vertexes[0].Position = Vector3.Transform(vertexes[0].Position, Matrix.CreateRotationZ(MathHelper.ToRadians(Angle))) + new Vector3(Center, 0);
					vertexes[1].Position = Vector3.Transform(vertexes[1].Position, Matrix.CreateRotationZ(MathHelper.ToRadians(Angle))) + new Vector3(Center, 0);
				}
				return vertexes;
			}
		}

		public Vector2? direction;
		public Vector2 Direction
		{
			get
			{
				if (direction == null)
				{
					var d = new Vector2(0, 1); //смотрит вверх
					direction = Vector2.Transform(d, Matrix.CreateRotationZ(MathHelper.ToRadians(Angle)));

				}
				return direction.Value;
			}
		}

	}
}
