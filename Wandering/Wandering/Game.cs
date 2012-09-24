using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Wandering.Helpers;

namespace Wandering
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		World.Level level;

		// описание формата вершин
		VertexDeclaration vertexDeclaration;
		// эффект BasicEffect
		BasicEffect effect;
		Effect mixEffect;

		RenderTarget2D gateView;
		RenderTarget2D gateShadow;
		RenderTarget2D[] mainView;

		SpriteBatch spriteBatch;


		int telepotDepth = 2;
		const int maxTelepotDepth = 5;
		Color[] LevelColor = new Color[maxTelepotDepth];

		bool debugMode = false;


		public Game()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here

			level = World.Scene.LoadLevel(1);

			vertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

			effect = new BasicEffect(graphics.GraphicsDevice);
			mixEffect = Content.Load<Effect>("MixEffect");

			//По максимальное ширине будует 100 единиц
			int w = graphics.PreferredBackBufferWidth;
			int h = graphics.PreferredBackBufferHeight;
			var prj = effect.Projection;
			if(w>h)
				prj.M22 *= ((float)w/(float)h);
			else
				prj.M11 *= ((float)h/(float)w);

			prj.M11/=50;
			prj.M22/=50;
			effect.Projection = prj;

			gateView = new RenderTarget2D(graphics.GraphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None);
			gateShadow = new RenderTarget2D(graphics.GraphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			mainView = new RenderTarget2D[maxTelepotDepth];
			for(int i=0; i<mainView.Length; ++i)
				mainView[i] = new RenderTarget2D(graphics.GraphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

			for(int i=0; i<LevelColor.Length; ++i)
			{
				var force = 255 - (128/LevelColor.Length) * i;
				LevelColor[i] = new Color(force, force, force);
			}

			spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here
			var kState = Keyboard.GetState();
			Vector2 direction = new Vector2();
			if (kState.IsKeyDown(Keys.Up))
				direction.Y += 1;
			if (kState.IsKeyDown(Keys.Down))
				direction.Y -= 1;
			if (kState.IsKeyDown(Keys.Right))
				direction.X += 1;
			if (kState.IsKeyDown(Keys.Left))
				direction.X -= 1;

			if(direction.Length() != 0)
			{
				direction.Normalize();
				direction = Vector2.Transform(direction, Matrix.CreateRotationZ(MathHelper.ToRadians(level.Player.direction)));

				var newPos = level.Player.Pos + (direction * gameTime.ElapsedGameTime.Milliseconds / 1000 * 10); //10 единиц в секунду
				
				//пересекли ли хоть один телепорт?
				var gate = level.Teleports.SelectMany(x => new World.Gate[]{x.GateA, x.GateB}).FirstOrDefault( g =>
				{
					//1. пересечение линии телепорта
					var g1 = g.Vertexes[0].Position.ToVector2();
					var g2 = g.Vertexes[1].Position.ToVector2();


					var p1 = level.Player.Pos - g1;
					var p2 = newPos - g1;
					var gv = g2 - g1;

					var ang1 = angleTest(gv, p1);
					var ang2 = angleTest(gv, p2);

					if ((ang1 <= 0 && ang2 <= 0) || (ang1 >= 0 && ang2 >= 0))
						return false;

					p1 = g1 - level.Player.Pos;
					p2 = g2 - level.Player.Pos;
					gv = newPos - level.Player.Pos;
					ang1 = angleTest(gv, p1);
					ang2 = angleTest(gv, p2);

					if ((ang1 <= 0 && ang2 <= 0) || (ang1 >= 0 && ang2 >= 0))
						return false;
					
					return true;
				});

				if (gate != null)
				{
					newPos = positionFromGate(gate, newPos);
					level.Player.direction = directionFromGate(gate, level.Player.direction);
				}
				level.Player.Pos = newPos;

			}

			if (kState.IsKeyDown(Keys.A))
				level.Player.direction += (float)gameTime.ElapsedGameTime.Milliseconds / 1000 * 50;
			if (kState.IsKeyDown(Keys.D))
				level.Player.direction -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000 * 50;


			//Меньше телепортов
			if (kState.IsKeyDown(Keys.Subtract))
				minusPresed = true;

			if (minusPresed && kState.IsKeyUp(Keys.Subtract) )
			{	
				minusPresed = false;
				if(telepotDepth > 1)
					--telepotDepth;
			}

			//Больше телепортов
			if (kState.IsKeyDown(Keys.Add))
				plusPresed = true;

			if (plusPresed && kState.IsKeyUp(Keys.Add))
			{
				plusPresed = false;
				if (telepotDepth < maxTelepotDepth)
					++telepotDepth;
			}


			base.Update(gameTime);
		}

		bool plusPresed = false;
		bool minusPresed = false;


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// отключить отсечение невидимых поверхностей
			GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			effect.VertexColorEnabled = true;

			DrawLevelRecursive(level.Player.Pos, level.Player.direction, 0, null);

			graphics.GraphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin();
			spriteBatch.Draw(mainView[0], graphics.GraphicsDevice.Viewport.Bounds, Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}

		int startDepth = 0;
		RenderTarget2D DrawLevelRecursive(Vector2 pos, float direction, int depth, World.Gate gate)
		{
			if (depth >= telepotDepth)
				return null;

			var oldRenderTargets = graphics.GraphicsDevice.GetRenderTargets();
			graphics.GraphicsDevice.SetRenderTarget( mainView[depth] );
			graphics.GraphicsDevice.Clear(debugMode ? LevelColor[depth] : Color.White);
			
			//Рисуем полигоны уровня
			if (depth >= startDepth)
				DrawLelel(pos, direction, gate);

			//Рисуем то, что за телепортами
			var gates = level.Teleports.SelectMany(x => new World.Gate[]{x.GateA, x.GateB}).OrderByDescending(x => (x.Center - pos).Length())
				.Where(x => Vector2.Dot(x.Direction, (pos - x.Center)) > 0).ToList();

			gates.ForEach( x => {
				var gPos = positionFromGate(x, pos);
				var gDir = directionFromGate(x, direction);

				var rt = DrawLevelRecursive(gPos, gDir, depth+1, x.Pair);
				if(rt==null)
					return;

				if (depth >= startDepth)
					drawGateShadow(x, pos, direction);
				else
					fillGateShadow();

				MixTexture(rt, gateShadow);
			});

			graphics.GraphicsDevice.SetRenderTargets(oldRenderTargets);

			return mainView[depth];
		}



		void DrawLelel(Vector2 pos, float direction, World.Gate gate)
		{
			var t = effect.View;
			effect.View = Matrix.CreateTranslation(new Vector3(-pos, 0)) * Matrix.CreateRotationZ( MathHelper.ToRadians(-direction) );

			effect.CurrentTechnique.Passes[0].Apply();
						
			level.Poligons.ForEach(x => {
				drawPoligonShadow(pos, x.Points, gate, debugMode ? Color.Red : Color.Black);
				graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, x.Vertexs, 0, x.TringleCount);
			});

			if(debugMode)
				level.Teleports.ForEach(x =>
				{
					graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, x.GateA.Vertexes, 0, 1);
					graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, x.GateB.Vertexes, 0, 1);
				});

			drawPlayer();

			effect.View = t;
		}

		void drawPlayer()
		{
			var vertexList = new VertexPositionColor[3];
			vertexList[1] = new VertexPositionColor(new Vector3(0 + level.Player.Pos.X, 0.5f + level.Player.Pos.Y, 0), Color.Gray);
			vertexList[0] = new VertexPositionColor(new Vector3(-0.5f + level.Player.Pos.X, -0.5f + level.Player.Pos.Y, 0), Color.Gray);
			vertexList[2] = new VertexPositionColor(new Vector3(0.5f + level.Player.Pos.X, -0.5f + level.Player.Pos.Y, 0), Color.Gray);

			//GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>
			   (PrimitiveType.TriangleList, vertexList, 0, 1);
		}

		void drawPoligonShadow(Vector2 pos, Vector2[] vertexs, World.Gate gate, Color color)
		{
			//всё просто, нужно определить крайние точки из vertexs

			toStartShadowLine(pos, vertexs, gate, (a, b) => drawShadow(new Vector3(pos, 0), new Vector3(a, 0), new Vector3(b, 0), color));
		}

		void toStartShadowLine(Vector2 pos, Vector2[] vertexs, World.Gate gate, Action<Vector2, Vector2> callback)
		{
			//var center = new Vector3(pos, 0);

			Vector2? a = null;
			Vector2? b = null;

			cutShape(pos, vertexs, gate, point => 
			{

				if (a == null)
				{
					a = point - pos; //минимальный градус
					b = point - pos; //максимальный градус
				}

				var sample = point - pos;

				if (a.Value.X * (-sample.Y) + a.Value.Y * sample.X > 0)
					a = sample;

				if (b.Value.X * (-sample.Y) + b.Value.Y * sample.X < 0)
					b = sample;

				
			});

			if(a!=null)
				callback(pos + a.Value, pos + b.Value);
		}

		void cutShape(Vector2 pos, Vector2[] vertexs, World.Gate gate, Action<Vector2> pointCallback)
		{

			if (gate == null)
			{
				for(int i=0; i<vertexs.Length; ++i)
					pointCallback(vertexs[i]);
				return;
			}

			float A = gate.Vertexes[0].Position.Y - gate.Vertexes[1].Position.Y;
			float B = gate.Vertexes[1].Position.X - gate.Vertexes[0].Position.X;
			float C = gate.Vertexes[0].Position.X * gate.Vertexes[1].Position.Y - gate.Vertexes[1].Position.X * gate.Vertexes[0].Position.Y;

			if (A * pos.X + B * pos.Y + C > 0)
			{
				A = -A;
				B = -B;
				C = -C;
			}

			Func<Vector2, float> pointPos = (p) => A * p.X + B * p.Y + C;
			Func<Vector2, Vector2, Vector2> pointIntersection = (a1, a2) =>
			{
				float A2 = a1.Y - a2.Y;
				float B2 = a2.X - a1.X;
				float C2 = a1.X * a2.Y - a2.X * a1.Y;

				return new Vector2((B*C2 - B2*C)/(A*B2 - A2*B), (A*C2 - A2*C)/(B*A2 - B2*A));
			};


			for (int i = 0; i < vertexs.Length; ++i)
			{
				var curPoint = vertexs[i];
				var nextPoint = vertexs[(i + 1) % vertexs.Length];
				if (pointPos(curPoint) >= 0)
				{
					//При переходе с положительной на отрицательную сторону, попадает точка перечения 
					if (pointPos(nextPoint) < 0)
					{
						pointCallback(pointIntersection(curPoint, nextPoint));
					}
					//При движении в положительной зоне, просто передаём следующую точку
					else
					{
						pointCallback(nextPoint);
					}
				}
				else
				{
					//при выходе из отрицательной зоны, нужно внести точку пересчения и следующую точку
					if (pointPos(nextPoint) >= 0)
					{
						pointCallback(pointIntersection(curPoint, nextPoint));
						pointCallback(nextPoint);
					}					
				}
			}
		}

		void MixTexture(RenderTarget2D src, RenderTarget2D mask = null)
		{
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			if(mask !=null)
			{
				mixEffect.Parameters["AlphaMap"].SetValue(gateShadow);
				mixEffect.CurrentTechnique.Passes[0].Apply();
			}
			spriteBatch.Draw(src, graphics.GraphicsDevice.Viewport.Bounds, Color.White);

			spriteBatch.End();
		}


		Vector2 positionFromGate(World.Gate gate, Vector2 pos)
		{
			//1. Совмещаем текущие врата с нулём
			var tg1 = Matrix.CreateTranslation(-gate.Center.X, -gate.Center.Y, 0);

			//2. Поворачиваем текущие врата, чтобы совместить их наклон с выходными
			var rg1 = Matrix.CreateRotationZ(MathHelper.ToRadians(gate.Pair.Angle - (gate.Angle + 180)));

			//3. Совмещаем положение текущих врат с выходными
			var tg2 = Matrix.CreateTranslation(new Vector3(gate.Pair.Center, 0));

			//4. Положение персонажа относительно выходных врат
			var altPlayer = Vector2.Transform(pos, tg1 * rg1 * tg2);

			return altPlayer;
		}

		float directionFromGate(World.Gate gate, float direction)
		{
			return direction + gate.Pair.Angle - (gate.Angle + 180);
		}

		/// <summary>
		/// Проверяет находится ли отрезок [b1,b2] в тени [a1,a2], если смотреть из pos
		/// </summary>
		/// <returns>true - если находится</returns>
		bool testShasow(Vector2 pos, Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			a1 = a1 - pos;
			a2 = a2 - pos;
			b1 = b1 - pos;
			b2 = b2 - pos;

			//1. разница между вторыми и первыми точками должна быть не меньше нуля и меньше 180 градусов
			if (angleTest(a1, a2) < 0)
				Swap(ref a1, ref a2);

			if (angleTest(b1, b2) < 0)
				Swap(ref a1, ref a2);

			//2. хотя бы одна точка должна лежать в полуплости куда падает тень
			var sh = a2 - a1;
			var b1InShadow = angleTest(sh, b1 - a1) < 0;
			var b2InShadow = angleTest(sh, b2 - a1) < 0;
			if ( !b1InShadow && !b2InShadow)
				return false;

			//3. если хотя бы одна точка находится в тени заданного отрезка, то тест положителен
			if( b1InShadow && (angleTest(a1, b1) > 0 && angleTest(a2, b1) < 0) )
				return true;

			if (b2InShadow && (angleTest(a1, b2) > 0 && angleTest(a2, b2) < 0))
				return true;

			//4. если точки в разных полуплостях относительно одного из векторов задающих отрезок тени, то тест положителен
			if (b1InShadow && angleTest(a1, b1) <= 0 && angleTest(a1, b2) > 0)
				return true;
			
			if(b2InShadow && angleTest(a2, b2) >= 0 && angleTest(a2, b1) < 0)
				return true;

			return false;
		}

		/// <summary>
		/// Опеределяет положение вектора b относительно a
		/// </summary>
		/// <returns>больше нуля, если +0...+180, меньше нуля, если -0...-180 градусо </returns>
		float angleTest(Vector2 a, Vector2 b)
		{
			return (-a.Y)*b.X + a.X*b.Y;
		}

		float angleTest(Vector3 a, Vector3 b)
		{
			return (-a.Y) * b.X + a.X * b.Y;
		}


		static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		float getAngle(Vector2 v)
		{
			return (float)(Math.Atan2(v.Y, v.X) - Math.PI/2);
		}

		VertexPositionColor[] shadowVertexList = new VertexPositionColor[12];
		void drawShadow(Vector3 pos, Vector3 a, Vector3 b, Color color)
		{
			var f = a - pos;
			var s = b - pos;
			f.Normalize();
			s.Normalize();
			f*=100;
			s*=100;

			//Нормаль к линии a, b
			var norm = b - a;
			if ((angleTest(a - pos, b - pos) > 0))
				norm = new Vector3(norm.Y, -norm.X, 0);
			else
				norm = new Vector3(-norm.Y, norm.X, 0);
			
			norm.Normalize();
			norm *= 100;
			

			shadowVertexList[0] = new VertexPositionColor(a, color);
			shadowVertexList[1] = new VertexPositionColor(b, color);
			shadowVertexList[2] = new VertexPositionColor(a + f, color);
			shadowVertexList[3] = new VertexPositionColor(a + f, color);
			shadowVertexList[4] = new VertexPositionColor(b, color);
			shadowVertexList[5] = new VertexPositionColor(b + s, color);

			shadowVertexList[6] = new VertexPositionColor(a + f, color);
			shadowVertexList[7] = new VertexPositionColor(b + s, color);
			shadowVertexList[8] = new VertexPositionColor(a + f + norm, color);
			shadowVertexList[9] = new VertexPositionColor(a + f + norm, color);
			shadowVertexList[10] = new VertexPositionColor(b + s, color);
			shadowVertexList[11] = new VertexPositionColor(b + s + norm, color);

			graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, shadowVertexList, 0, 4);
		}

		void fillGateShadow()
		{
			var oldRenderTargets = graphics.GraphicsDevice.GetRenderTargets();
			graphics.GraphicsDevice.SetRenderTarget(gateShadow);

			effect.CurrentTechnique.Passes[0].Apply();

			graphics.GraphicsDevice.Clear(Color.White);
			graphics.GraphicsDevice.SetRenderTargets(oldRenderTargets);
		}

		void drawGateShadow (World.Gate gate, Vector2 pos, float dir)
		{
			var a = gate.Vertexes[0].Position;
			var b = gate.Vertexes[1].Position;


			var oldRenderTargets = graphics.GraphicsDevice.GetRenderTargets();
			graphics.GraphicsDevice.SetRenderTarget(gateShadow);

			effect.View = Matrix.CreateTranslation( -pos.X, -pos.Y, 0) * Matrix.CreateRotationZ(MathHelper.ToRadians(-dir));
			effect.CurrentTechnique.Passes[0].Apply();

			graphics.GraphicsDevice.Clear(Color.Transparent);
			
			drawShadow(new Vector3(pos,0), a, b, Color.White);

			//затираем тенями
			var lA = a;
			var lB = b;
			level.Poligons.ForEach(x => 
				toStartShadowLine(pos, x.Points, null, (sh1, sh2) => 
				{
					if (testShasow(pos, new Vector2(sh1.X, sh1.Y), new Vector2(sh2.X, sh2.Y), new Vector2(lA.X, lA.Y), new Vector2(lB.X, lB.Y)))
						drawShadow(new Vector3(pos, 0), new Vector3(sh1,0), new Vector3(sh2,0), Color.Black);
				})
			);

			drawPlayer();

			graphics.GraphicsDevice.SetRenderTargets(oldRenderTargets);
		}


		void save(RenderTarget2D r, string n = "ololo")
		{
			var t = graphics.GraphicsDevice.GetRenderTargets();
			graphics.GraphicsDevice.SetRenderTarget(null);
			using (var fstr = File.OpenWrite(n + ".png"))
			{
				r.SaveAsPng(fstr, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
			}
			graphics.GraphicsDevice.SetRenderTargets(t);
		}

		void drawFullScreen()
		{			
			var a = new Vector2(-1, 1);
			var b = new Vector2(1, 1);
			var c = new Vector2(1, -1);
			var d = new Vector2(-1, -1);

			VertexPositionTextureTexture[] l = new VertexPositionTextureTexture[6];

			l[0] = new VertexPositionTextureTexture(new Vector3(a, 0), new Vector2(0, 0));
			l[1] = new VertexPositionTextureTexture(new Vector3(b, 0), new Vector2(1, 0));
			l[2] = new VertexPositionTextureTexture(new Vector3(d, 0), new Vector2(0, 1));
			l[3] = new VertexPositionTextureTexture(new Vector3(b, 0), new Vector2(1, 0));
			l[4] = new VertexPositionTextureTexture(new Vector3(c, 0), new Vector2(1, 1));
			l[5] = new VertexPositionTextureTexture(new Vector3(d, 0), new Vector2(0, 1));

			graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionTextureTexture>(PrimitiveType.TriangleList, l, 0, 2);
		}

	}
}
