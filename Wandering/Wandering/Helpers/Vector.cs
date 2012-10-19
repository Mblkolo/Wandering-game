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

		/// <summary>
		/// Определяет пересечение отрезка a отрезком b
		/// Пересечение происходит, если концы отрезка b лежат по разные стороны прямой a или b1 лежит на прямой a, а b2 не лежит
		/// И точка пересечения отрезков а и b лежит на отрезке a включая крайние точки
		/// </summary>
		/// <param name="a1">Начало отрезка a</param>
		/// <param name="a2">Конец отрезка a</param>
		/// <param name="b1">Начало отрезка b</param>
		/// <param name="b2">Конец отрезка b</param>
		/// <returns>true, если отрезки пересекаются</returns>
		public static bool DetectCrossing(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			var p1 = b1 - a1;
			var p2 = b2 - a1;
			var p0 = a2 - a1;
			var ang1 = angleTest(p0, p1);
			var ang2 = angleTest(p0, p2);

			//Точка перечения отрезков лежит на отрезке b, исключая точку b2
			if ((ang1 < 0 && ang2 < 0) || (ang1 > 0 && ang2 > 0) || ang2 == 0)
				return false;


			p1 = a1 - b1;
			p2 = a2 - b1;
			p0 = b2 - b1;
			ang1 = angleTest(p0, p1);
			ang2 = angleTest(p0, p2);

			//Точка перечения отрезков лежит строго за пределеами отрезка a
			if ((ang1 < 0 && ang2 < 0) || (ang1 > 0 && ang2 > 0))
				return false;

			return true;
		}

		/// <summary>
		/// Опеределяет положение вектора b относительно a
		/// </summary>
		/// <returns>больше нуля, если +0...+180, меньше нуля, если -0...-180 градусов </returns>
		public static float angleTest(Vector2 a, Vector2 b)
		{
			return (-a.Y) * b.X + a.X * b.Y;
		}

		public static float angleTest(Vector3 a, Vector3 b)
		{
			return (-a.Y) * b.X + a.X * b.Y;
		}

		/// <summary>
		/// Проверяет находится ли отрезок [b1,b2] в тени [a1,a2], если смотреть из pos
		/// </summary>
		/// <returns>true - если находится</returns>
		public static bool testShasow(Vector2 pos, Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			a1 = a1 - pos;
			a2 = a2 - pos;
			b1 = b1 - pos;
			b2 = b2 - pos;

			//1. разница между вторыми и первыми точками должна быть не меньше нуля и меньше 180 градусов
			if (Vector.angleTest(a1, a2) < 0)
				Swap(ref a1, ref a2);

			if (Vector.angleTest(b1, b2) < 0)
				Swap(ref a1, ref a2);

			//2. хотя бы одна точка должна лежать в полуплости куда падает тень
			var sh = a2 - a1;
			var b1InShadow = Vector.angleTest(sh, b1 - a1) < 0;
			var b2InShadow = Vector.angleTest(sh, b2 - a1) < 0;
			if (!b1InShadow && !b2InShadow)
				return false;

			//3. если хотя бы одна точка находится в тени заданного отрезка, то тест положителен
			if (b1InShadow && (Vector.angleTest(a1, b1) > 0 && Vector.angleTest(a2, b1) < 0))
				return true;

			if (b2InShadow && (Vector.angleTest(a1, b2) > 0 && Vector.angleTest(a2, b2) < 0))
				return true;

			//4. если точки в разных полуплостях относительно одного из векторов задающих отрезок тени, то тест положителен
			if (b1InShadow && Vector.angleTest(a1, b1) <= 0 && Vector.angleTest(a1, b2) > 0)
				return true;

			if (b2InShadow && Vector.angleTest(a2, b2) >= 0 && Vector.angleTest(a2, b1) < 0)
				return true;

			return false;
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		/// <summary>
		/// Обрезает выпуклый многоугольник отрезком gate и возвращает точки геометрии лежащие внутри области отсечения
		/// </summary>
		/// <param name="pos">Точка взгляда, находится в той полуплоскости в которой не должно быть многоугольника</param>
		/// <param name="vertexs">Вершины выпуклого многоугольника</param>
		/// <param name="gate">Отрезок, задающий линию отсечения</param>
		/// <param name="pointCallback"></param>
		public static void cutShape(Vector2 pos, Vector2[] vertexs, Segment2 gate, Action<Vector2> pointCallback)
		{
			float A = gate.P1.Y - gate.P2.Y;
			float B = gate.P2.X - gate.P1.X;
			float C = gate.P1.X * gate.P2.Y - gate.P2.X * gate.P1.Y;

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

				return new Vector2((B * C2 - B2 * C) / (A * B2 - A2 * B), (A * C2 - A2 * C) / (B * A2 - B2 * A));
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

		/// <summary>
		/// Обноруживает пересечение окружности и отрезка
		/// </summary>
		/// <param name="c">Цент окружности</param>
		/// <param name="r">Радиус окружности</param>
		/// <param name="a1">Начало отрезка</param>
		/// <param name="a2">Конец отрезка</param>
		/// <returns></returns>
		public static bool DetectCollision(Vector2 center, float r, Vector2 a1, Vector2 a2)
		{
			var c1 = a1 - center;
			var c2 = a2 - center;

			var d = a2 - a1;

			float a = d.X * d.X + d.Y * d.Y;
			float b = 2.0f * (c1.X * d.X + c1.Y * d.Y);
			float c = c1.X * c1.X + c1.Y * c1.Y - r * r;

			if (-b < 0) return (c < 0);
			if (-b < (2.0f * a)) return (4.0f * a * c - b * b < 0);
			return (a + b + c < 0);



			////float ang = angleTest(a2-a1, c - a1);
			//var t = a1 - c;
			//if (t.X * t.X + t.Y * t.Y <= r * r)
			//    return true;

			//t = a2 - c;
			//if (t.X * t.X + t.Y * t.Y <= r * r)
			//    return true;


			//return false;
		}

		/// <summary>
		/// Поварачивает вектор на 90 градусов по часовой стрелке
		/// </summary>
		public static Vector2 Rotate90Right(Vector2 v)
		{
			return new Vector2(v.Y, -v.X);
		}

		/// <summary>
		/// Поварачивает вектор на 90 градусов против часовой стрелки
		/// </summary>
		public static Vector2 Rotate90Left(Vector2 v)
		{
			return new Vector2(v.Y, -v.X);
		}


	}
}
