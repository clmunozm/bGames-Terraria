using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ModLoader;

namespace bGamesMod
{
	internal static class ModUtils
	{
		private static MethodInfo _loadPlayersMethod;

		private static MethodInfo _mouseTextMethod;

		private static MethodInfo _invasionWarningMethod;
		private static FieldInfo _npcDefaultSpawnRate;
		private static FieldInfo _npcDefaultMaxSpawns;

		private static PropertyInfo _steamid;

		private static FieldInfo _hueTexture;

		private static Texture2D _dummyTexture;
		private static float _deltaTime;

		private static Texture2D _logoTexture;
		private static Texture2D _logoTexture2;
		private static Texture2D _testTubeTexture;

		internal static Item[] previousInventoryItems;

		public static event EventHandler InventoryChanged;

		public static bool InterfaceVisible { get; set; }

		/// <summary>
		/// A 1x1 pixel white texture.
		/// </summary>
		public static Texture2D DummyTexture
		{
			get
			{
				if (_dummyTexture == null)
				{
					_dummyTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
					_dummyTexture.SetData(new Color[] { Color.White });
				}
				return _dummyTexture;
			}
		}

		public static KeyboardState PreviousKeyboardState { get; set; }
		public static MouseState MouseState { get; set; }
		public static MouseState PreviousMouseState { get; set; }

		/// <summary>
		/// Time in seconds that has passed since the last update call.
		/// </summary>
		public static float DeltaTime
		{
			get { return _deltaTime; }
		}

		public static int NPCDefaultSpawnRate
		{
			get { return (int)_npcDefaultSpawnRate.GetValue(null); }
			set { _npcDefaultSpawnRate.SetValue(null, value); }
		}

		public static int NPCDefaultMaxSpawns
		{
			get { return (int)_npcDefaultMaxSpawns.GetValue(null); }
			set { _npcDefaultMaxSpawns.SetValue(null, value); }
		}

		public static string SteamID
		{
			get { return (string)_steamid.GetValue(null, null); }
		}
		public static Texture2D HueTexture
		{
			get
			{
				return TextureAssets.Hue.Value;
			}
		}

		/// <summary>
		/// Gets or Sets if the game camera is free to move from the players position
		/// </summary>
		public static bool FreeCamera { get; set; }

		public static NetworkMode NetworkMode
		{
			get
			{
				return (NetworkMode)Main.netMode;
			}
		}

		/// <summary>
		/// Server Side Characters Enabled
		/// </summary>
		public static bool SSC
		{
			get
			{
				return Main.ServerSideCharacter;
			}
		}

		public static void Init()
		{
			InitReflection();
			InterfaceVisible = true;

			if (NetworkMode != NetworkMode.Server)
			{
				FreeCamera = false;
				previousInventoryItems = new Item[Main.player[Main.myPlayer].inventory.Length];
				SetPreviousInventory();
				Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -10000, 10000);
			}
		}

		private static void InitReflection()
		{
			try
			{
				_loadPlayersMethod = typeof(Main).GetMethod("LoadPlayers", BindingFlags.NonPublic | BindingFlags.Static);
				_mouseTextMethod = typeof(Main).GetMethod("MouseText", BindingFlags.NonPublic | BindingFlags.Instance);
				_invasionWarningMethod = typeof(Main).GetMethod("InvasionWarning", BindingFlags.NonPublic | BindingFlags.Static);
				_npcDefaultSpawnRate = typeof(NPC).GetField("defaultSpawnRate", BindingFlags.NonPublic | BindingFlags.Static);
				_npcDefaultMaxSpawns = typeof(NPC).GetField("defaultMaxSpawns", BindingFlags.NonPublic | BindingFlags.Static);

				_steamid = typeof(ModLoader).GetProperty("SteamID64", BindingFlags.NonPublic | BindingFlags.Static);

				_hueTexture = Main.instance.GetType().GetField("hueTexture", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			catch (Exception e)
			{
				ModUtils.DebugText(e.Message + " " + e.StackTrace);
			}
		}

		public static void Update()
		{
			if (!Main.gameMenu)
			{
				if (ItemChanged())
				{
					if (InventoryChanged != null)
					{
						InventoryChanged(null, EventArgs.Empty);
					}
					SetPreviousInventory();
				}
			}
		}

		private static bool ItemChanged()
		{
			Player player = Main.player[Main.myPlayer];
			for (int i = 0; i < player.inventory.Length - 1; i++)
			{
				if (player.inventory[i].IsNotSameTypePrefixAndStack(previousInventoryItems[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static void SetPreviousInventory()
		{
			Player player = Main.player[Main.myPlayer];
			for (int i = 0; i < player.inventory.Length; i++)
			{
				previousInventoryItems[i] = player.inventory[i].Clone();
			}
		}

		public static void LoadPlayers()
		{
			_loadPlayersMethod.Invoke(null, null);
		}

		public static void StartRain()
		{
			//_startRainMethod.Invoke(null, null);
			Main.StartRain();
		}

		public static void StopRain()
		{
			//_stopRainMethod.Invoke(null, null);
			Main.StopRain();
		}

		public static void StartSandstorm()
		{
			Sandstorm.StartSandstorm();
		}

		public static void StopSandstorm()
		{
			Sandstorm.StopSandstorm();
		}

		public static void LoadNPC(int type, bool immediate = false)
		{
			if (immediate)
			{
				Main.instance.LoadNPC(type);
				return;
			}
			// Use this instead of Main.instance.LoadNPC because we don't need ImmediateLoad
			if (TextureAssets.Npc[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Npc[type].Name, AssetRequestMode.AsyncLoad);
		}

		public static void LoadProjectile(int type)
		{
			if (TextureAssets.Projectile[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Projectile[type].Name, AssetRequestMode.AsyncLoad);
		}

		public static void LoadItem(int type)
		{
			// Use this instead of Main.instance.LoadItem because we don't need ImmediateLoad
			if (TextureAssets.Item[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Item[type].Name, AssetRequestMode.AsyncLoad);
		}

		public static void MouseText(string cursorText, int rare = 0, byte diff = 0)
		{
			_mouseTextMethod.Invoke(Main.instance, new object[] { cursorText, rare, diff });
		}

		public static void InvasionWarning()
		{
			_invasionWarningMethod.Invoke(null, null);
		}

		public static void MoveToPosition(Vector2 newPos)
		{
			Player player = Main.player[Main.myPlayer];
			player.position = newPos;
			player.velocity = Vector2.Zero;
			player.fallStart = (int)(player.position.Y / 16f);
		}

		/// <summary>
		/// Set the Delta Time
		/// </summary>
		/// <param name="gameTime">Games current Game Time</param>
		public static void SetDeltaTime(/*GameTime gameTime*/)
		{
			_deltaTime = 1f / 60f;// (float)gameTime.ElapsedGameTime.TotalSeconds;
		}

		public static void SetDeltaTime(float deltaTime)
		{
			_deltaTime = deltaTime;
		}

		public static bool StringStartsWith(string str, string startStr)
		{
			if (str.Length >= startStr.Length)
			{
				if (str.Substring(0, startStr.Length) == startStr) return true;
			}
			return false;
		}

		public static string GetEndOfString(string str, string startStr)
		{
			return str.Substring(startStr.Length, str.Length - startStr.Length);
		}

		public static Vector2 CursorPosition
		{
			get
			{
				return new Vector2(Main.mouseX, Main.mouseY);
			}
		}

		public static Vector2 CursorWorldCoords
		{
			get
			{
				return CursorPosition + Main.screenPosition;
			}
		}

		public static Vector2 CursorTileCoords
		{
			get
			{
				return GetTileCoordsFromWorldCoords(GetCursorWorldCoords());
			}
		}

		public static Vector2 GetCursorWorldCoords()
		{
			return new Vector2((int)Main.screenPosition.X + Main.mouseX, (int)Main.screenPosition.Y + Main.mouseY);
		}

		public static Vector2 GetTileCoordsFromWorldCoords(Vector2 worldCoords)
		{
			return new Vector2((int)worldCoords.X / 16, (int)worldCoords.Y / 16);
		}

		public static Vector2 GetWorldCoordsFromTileCoords(Vector2 tileCoords)
		{
			return new Vector2((int)tileCoords.X * 16, (int)tileCoords.Y * 16);
		}


		public static void DrawBorderedRect(SpriteBatch spriteBatch, Color infillColor, Color outlineColor, Vector2 position, Vector2 size, int outlineThickness)
		{
			Texture2D pixel = ModUtils.DummyTexture;
			Rectangle pixelRect = new Rectangle(0, 0, 1, 1);
			Vector2 positionOnScreen = (position * 16f) - Main.screenPosition;
			Vector2 sizeOnScreen = size * 16f;

			spriteBatch.Draw(pixel, positionOnScreen, pixelRect, infillColor, 0, Vector2.Zero, sizeOnScreen, SpriteEffects.None, 0);

			Vector2 outlineSize = new Vector2(sizeOnScreen.X, outlineThickness);
			Vector2 outlinePosition = positionOnScreen;
			spriteBatch.Draw(pixel, outlinePosition, pixelRect, outlineColor, 0, Vector2.Zero, outlineSize, SpriteEffects.None, 0);
			outlinePosition.Y += sizeOnScreen.Y - outlineThickness;
			spriteBatch.Draw(pixel, outlinePosition, pixelRect, outlineColor, 0, Vector2.Zero, outlineSize, SpriteEffects.None, 0);

			outlineSize = new Vector2(outlineThickness, sizeOnScreen.Y);
			outlinePosition = positionOnScreen;
			spriteBatch.Draw(pixel, outlinePosition, pixelRect, outlineColor, 0, Vector2.Zero, outlineSize, SpriteEffects.None, 0);
			outlinePosition.X += sizeOnScreen.X - outlineThickness;
			spriteBatch.Draw(pixel, outlinePosition, pixelRect, outlineColor, 0, Vector2.Zero, outlineSize, SpriteEffects.None, 0);
		}

		public static void DrawBorderedRect(SpriteBatch spriteBatch, Color color, Vector2 position, Vector2 size, int borderWidth)
		{
			Color fillColor = color * .3f;
			ModUtils.DrawBorderedRect(spriteBatch, fillColor, color, position, size, borderWidth);
		}

		public static void DrawBorderedRect(SpriteBatch spriteBatch, Color color, Vector2 position, Vector2 size, int borderWidth, string text)
		{
			DrawBorderedRect(spriteBatch, color, position, size, borderWidth);
			Vector2 pos = ModUtils.GetWorldCoordsFromTileCoords(position) - Main.screenPosition;
			pos.X += 2;
			pos.Y += 2;
			spriteBatch.DrawString(FontAssets.MouseText.Value, text, pos, Color.White, 0f, Vector2.Zero, .7f, SpriteEffects.None, 0);
		}

		public static void DrawStringBorder(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, string text, Color borderColor, float boarderSize, Vector2 origin, float scale)
		{
			Vector2 pos = Vector2.Zero;
			int i = 0;
			while (i < 4)
			{
				switch (i)
				{
					case 0:
						pos.X = position.X - boarderSize;
						pos.Y = position.Y;
						break;

					case 1:
						pos.X = position.X + boarderSize;
						pos.Y = position.Y;
						break;

					case 2:
						pos.X = position.X;
						pos.Y = position.Y - boarderSize;
						break;

					case 3:
						pos.X = position.X;
						pos.Y = position.Y + boarderSize;
						break;
				}
				spriteBatch.DrawString(font, text, pos, borderColor, 0f, origin, scale, SpriteEffects.None, 0f);
				i++;
			}
		}

		private static Dictionary<int, Color> rarityColors = new Dictionary<int, Color>()
		{
			{-11, new Color(255, 175, 0) },
			{-1, new Color(130, 130, 130) },
			{1, new Color(150, 150, 255) },
			{2, new Color(150, 255, 150) },
			{3, new Color(255, 200, 150) },
			{4, new Color(255, 150, 150) },
			{5, new Color(255, 150, 255) },
			{6, new Color(210, 160, 255) },
			{7, new Color(150, 255, 10) },
			{8, new Color(255, 255, 10) },
			{9, new Color(5, 200, 255) },
		};

		public static Color GetItemColor(Item item)
		{
			if (rarityColors.ContainsKey(item.rare))
			{
				return rarityColors[item.rare];
			}
			return Color.White;
		}

		private static bool debug = false;

		public static void DebugText(string message)
		{
			if (debug)
			{
				string header = "HERO's Mod: ";
				if (Main.dedServ)
				{
					Console.WriteLine(header + message);
				}
				else
				{
					if (Main.gameMenu)
					{
						bGamesMod.instance.Logger.Debug(header + Main.myPlayer + ": " + message);
					}
					else
					{
						Main.NewText(header + message);
					}
				}
			}
		}

		public static Rectangle GetClippingRectangle(SpriteBatch spriteBatch, Rectangle r)
		{
			Vector2 vector = new Vector2(r.X, r.Y);
			Vector2 position = new Vector2(r.Width, r.Height) + vector;
			vector = Vector2.Transform(vector, Main.UIScaleMatrix);
			position = Vector2.Transform(position, Main.UIScaleMatrix);
			Rectangle result = new Rectangle((int)vector.X, (int)vector.Y, (int)(position.X - vector.X), (int)(position.Y - vector.Y));
			int width = spriteBatch.GraphicsDevice.Viewport.Width;
			int height = spriteBatch.GraphicsDevice.Viewport.Height;
			result.X = Utils.Clamp<int>(result.X, 0, width);
			result.Y = Utils.Clamp<int>(result.Y, 0, height);
			result.Width = Utils.Clamp<int>(result.Width, 0, width - result.X);
			result.Height = Utils.Clamp<int>(result.Height, 0, height - result.Y);
			return result;
		}
	}

	public enum NetworkMode : byte
	{
		None,
		Client,
		Server
	}
}