using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using Terraria;
using bGamesMod.bGamesModServices;
using bGamesMod.UIKit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.NetModules;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using ReLogic.Content.Sources;
using Terraria.ID;

namespace bGamesMod.UIKit
{
	internal class LoginButton : UIState
	{
		private Texture2D buttonTexture;
		private Rectangle buttonRectangle;
		private bool isModalOpen = false;

		public LoginButton(Texture2D texture)
		{
			buttonTexture = texture;
			Width.Set(buttonTexture.Width, 0f);
			Height.Set(buttonTexture.Height, 0f);
			Left.Set(10f, 0f); // Coloca el botón en el lado izquierdo de la pantalla
			Top.Set(100f, 0f); // Ajusta la posición según sea necesario
		}

		public override void OnInitialize()
		{
			buttonRectangle = new Rectangle((int)Left.Pixels, (int)Top.Pixels, buttonTexture.Width, buttonTexture.Height);
			base.OnInitialize();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			// Dibujar el botón
			spriteBatch.Draw(buttonTexture, buttonRectangle, Color.White);

			// Dibujar el modal si está abierto
			if (isModalOpen)
			{
				DrawModal(spriteBatch);
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (Main.mouseLeft && buttonRectangle.Contains(Main.mouseX, Main.mouseY))
			{
				// Si el botón es clicado, abrir/cerrar el modal
				isModalOpen = !isModalOpen;
			}
		}

		private void DrawModal(SpriteBatch spriteBatch)
		{
			
		}
	}

}
