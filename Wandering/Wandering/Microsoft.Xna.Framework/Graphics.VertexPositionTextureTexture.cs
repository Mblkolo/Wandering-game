using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexPositionTextureTexture : IVertexType
	{
		Vector3 vertexPosition;
		Vector2 vertexTexture;
		Vector2 vertexTexture2;

		public static readonly VertexElement[] VertexElements = {  
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), // All examples I have seen before have 0 as the last argument in these lines.
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), // To distinguish between the two texture coordinates, 
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1) // <-THIS ONE has 1 as it's final argument
            };

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return new VertexDeclaration(VertexElements); }
		}

		public VertexPositionTextureTexture(Vector3 pos, Vector2 tex, Vector2 tex2)
		{
			vertexPosition = pos;
			vertexTexture = tex;
			vertexTexture2 = tex2;
		}

		public VertexPositionTextureTexture(Vector3 pos, Vector2 tex)
		{
			vertexPosition = pos;
			vertexTexture = tex;
			vertexTexture2 = tex;
		}
	}
}
