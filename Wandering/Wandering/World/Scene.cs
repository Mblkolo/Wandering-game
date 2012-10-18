using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Wandering.World
{
	static class Scene
	{
		public static Level LoadLevel(int levelNum)
		{
			var doc = XDocument.Load("Levels\\Level1.xml");

			var level = new Level();

			level.Poligons = doc.Root.Elements("Scene").Elements("Poligon").Select(x =>
				{
					var p = new Poligon();
					p.Points = x.Elements("Point").Select(y => new Vector2(float.Parse(y.Attribute("x").Value), float.Parse(y.Attribute("y").Value))).ToArray();
					return p;
				}
			).ToList();

			level.Images = doc.Root.Elements("Scene").Elements("Image").Select(x =>
				{
					var i = new Image();
					i.pos = new Vector2(float.Parse(x.Attribute("x").Value), float.Parse(x.Attribute("y").Value));
					i.textureName = x.Attribute("name").Value;
					i.width = float.Parse(x.Attribute("w").Value);
					i.height = float.Parse(x.Attribute("h").Value);
					return i;
				}
			).ToList();

			level.Teleports = doc.Root.Elements("Scene").Elements("Teleport").Select(x =>
			{
				var t = new Teleport();
				Gate g = new Gate();

				g.Center = new Vector2(float.Parse(x.Attribute("x1").Value), float.Parse(x.Attribute("y1").Value));
				g.Angle = float.Parse(x.Attribute("ang1").Value);
				g.Teleport = t;
				t.GateA = g;

				g = new Gate();
				g.Center = new Vector2(float.Parse(x.Attribute("x2").Value), float.Parse(x.Attribute("y2").Value));
				g.Angle = float.Parse(x.Attribute("ang2").Value);
				g.Teleport = t;
				t.GateB = g;

				t.GateA.Pair = t.GateB;
				t.GateB.Pair = t.GateA;

				t.lenght = float.Parse(x.Attribute("len").Value);
				return t;
			}
			).ToList();

			var player = new Player();
			player.Pos = new Vector2(float.Parse(doc.Root.Element("Player").Attribute("x").Value), float.Parse(doc.Root.Element("Player").Attribute("y").Value));
			player.direction = float.Parse(doc.Root.Element("Player").Attribute("direction").Value);
			level.Player = player;

			return level;
		}
	}
}
