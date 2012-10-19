using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wandering.World
{
	class Image
	{
		public Vector2 pos;
		public float width;
		public float height;
		public float angle;

		public string textureName;
		public Texture2D texture;

		private VertexPositionTexture[] vertexs;
		public VertexPositionTexture[] Vertexs
		{
			get
			{
				if (vertexs == null)
				{
					var transform = Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(pos.X, pos.Y, 0);
					vertexs = new VertexPositionTexture[]
					{
						new VertexPositionTexture( Vector3.Transform(new Vector3(-width/2,  height/2, 0), transform), new Vector2(0,0) ),
						new VertexPositionTexture( Vector3.Transform(new Vector3( width/2,  height/2, 0), transform), new Vector2(1,0) ),
						new VertexPositionTexture( Vector3.Transform(new Vector3( width/2, -height/2, 0), transform), new Vector2(1,1) ),
						new VertexPositionTexture( Vector3.Transform(new Vector3(-width/2,  height/2, 0), transform), new Vector2(0,0) ),
						new VertexPositionTexture( Vector3.Transform(new Vector3(-width/2, -height/2, 0), transform), new Vector2(0,1) ),
						new VertexPositionTexture( Vector3.Transform(new Vector3( width/2, -height/2, 0), transform), new Vector2(1,1) ),
					};
				}
				return vertexs;
			}
		}
	}
}
