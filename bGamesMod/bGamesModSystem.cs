using bGamesMod.bGamesModServices;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace bGamesMod
{
	public class bGamesModSystem : ModSystem
	{
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"HerosMod: UI",
					delegate {
						try
						{
							bGamesMod.Update();

							bGamesMod.ServiceHotbar.Update();

							bGamesMod.Draw(Main.spriteBatch);
						}
						catch (Exception e)
						{
							ModUtils.DebugText("PostDrawInInventory Error: " + e.Message + e.StackTrace);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}

			int rulerLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
			if (rulerLayerIndex != -1)
			{
				layers.Insert(rulerLayerIndex, new LegacyGameInterfaceLayer(
					"HerosMod: Game Scale UI",
					delegate {
						return true;
					},
					InterfaceScaleType.Game)
				);
			}
		}	
	}
}
