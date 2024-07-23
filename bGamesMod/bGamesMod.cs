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

// TODO, freeze is bypassable.
// TODO, regions prevent all the chest movement and right click.
// TODO -- Should I have all services use the same Global hooks?
namespace bGamesMod
{
	internal record CallButtonParameters(string permissionName, Asset<Texture2D> texture, Action buttonClickedAction, Action<bool> groupUpdated, Func<string> tooltip);

	internal record PermissionsParameters(string permissionName, string permissionDisplayName, Action<bool> groupUpdated);

	internal class bGamesMod : Mod
	{
		public static bGamesMod instance;
		internal List<UIKit.UIComponents.ModCategory> modCategories;

		internal List<CallButtonParameters> callButtons = new();
		internal List<PermissionsParameters> permissionsParameters = new();

		internal Dictionary<string, Action<bool>> crossModGroupUpdated = new Dictionary<string, Action<bool>>();

		public override void Load()
		{
			try
			{
				instance = this;

				modCategories = new List<UIKit.UIComponents.ModCategory>();

				if (!Main.dedServ)
				{
					// TODO: this should be async, but I'm too lazy to rewrite it to support assets
					UIKit.UIButton.buttonBackground = Assets.Request<Texture2D>("Images/UIKit/buttonEdge", AssetRequestMode.ImmediateLoad);
					UIKit.UIView.closeTexture = Assets.Request<Texture2D>("Images/closeButton", AssetRequestMode.ImmediateLoad);
					UIKit.UITextbox.textboxBackground = Assets.Request<Texture2D>("Images/UIKit/textboxEdge", AssetRequestMode.ImmediateLoad);
					UIKit.UISlider.barTexture = Assets.Request<Texture2D>("Images/UIKit/barEdge", AssetRequestMode.ImmediateLoad);
					UIKit.UIScrollView.ScrollbgTexture = Assets.Request<Texture2D>("Images/UIKit/scrollbgEdge", AssetRequestMode.ImmediateLoad);
					UIKit.UIScrollBar.ScrollbarTexture = Assets.Request<Texture2D>("Images/UIKit/scrollbarEdge", AssetRequestMode.ImmediateLoad);
					UIKit.UIDropdown.capUp = Assets.Request<Texture2D>("Images/UIKit/dropdownCapUp", AssetRequestMode.ImmediateLoad);
					UIKit.UIDropdown.capDown = Assets.Request<Texture2D>("Images/UIKit/dropdownCapDown", AssetRequestMode.ImmediateLoad);
					UIKit.UICheckbox.checkboxTexture = Assets.Request<Texture2D>("Images/UIKit/checkBox", AssetRequestMode.ImmediateLoad);
					UIKit.UICheckbox.checkmarkTexture = Assets.Request<Texture2D>("Images/UIKit/checkMark", AssetRequestMode.ImmediateLoad);
				}

				Init_Load();
			}
			catch (Exception e)
			{
				ModUtils.DebugText("Load:\n" + e.Message + "\n" + e.StackTrace + "\n");
			}
		}

		internal static string HeroText(string key)
		{
			return Language.GetTextValue($"Mods.bGamesMod.{key}");
		}

		// Clear EVERYthing, mod is unloaded.
		public override void Unload()
		{
			UIKit.UIComponents.ItemBrowser.Filters = null;
			UIKit.UIComponents.ItemBrowser.DefaultSorts = null;
			UIKit.UIComponents.ItemBrowser.Categories = null;
			UIKit.UIComponents.ItemBrowser.CategoriesLoaded = false;
			UIKit.UIButton.buttonBackground = null;
			UIKit.UIView.closeTexture = null;
			UIKit.UITextbox.textboxBackground = null;
			UIKit.UISlider.barTexture = null;
			UIKit.UIScrollView.ScrollbgTexture = null;
			UIKit.UIScrollBar.ScrollbarTexture = null;
			UIKit.UIDropdown.capUp = null;
			UIKit.UIDropdown.capDown = null;
			UIKit.UICheckbox.checkboxTexture = null;
			UIKit.UICheckbox.checkmarkTexture = null;
			bGamesModServices.Login._loginTexture = null;
			bGamesModServices.Login._logoutTexture = null;
			try
			{
				if (ServiceController != null)
				{
					if (ServiceController.Services != null)
					{
						foreach (var service in ServiceController.Services)
						{
							service.Unload();
						}
					}
					ServiceController.RemoveAllServices();
				}
				if (!Main.dedServ)
					MasterView.ClearMasterView();
			}
			catch (Exception e)
			{
				ModUtils.DebugText("Unload:\n" + e.Message + "\n" + e.StackTrace + "\n");
			}
			_hotbar = null;
			ServiceController = null;
			ModUtils.previousInventoryItems = null;
			modCategories = null;
			instance = null;
		}
		

		public override void PostSetupContent()
		{
			Init_PostSetupContent();

			permissionsParameters.Clear();

			if (!Main.dedServ)
			{
				callButtons.Clear();

				foreach (var service in ServiceController.Services)
				{
					service.PostSetupContent();
				}
			}
		}

		private static bool _prevGameMenu = true;

		// Holds all the loaded services.
		public static ServiceController ServiceController;

		public static RenderTarget2D RenderTarget { get; set; }

		private static ServiceHotbar _hotbar;
		public static ServiceHotbar ServiceHotbar
		{
			get { return _hotbar; }
		}

		public static void Init_Load()
		{
			ModUtils.Init();
		}

		public static void Init_PostSetupContent()
		{
			if (!Main.dedServ)
			{
				UIView.exclusiveControl = null;
				ServiceController = new ServiceController();
				_hotbar = new ServiceHotbar();
				UIKit.UIColorPicker colorPicker = new UIKit.UIColorPicker();
				colorPicker.X = 200;
				UIKit.MasterView.menuScreen.AddChild(colorPicker);
				LoadAddServices();
			}
		}

		public void RegisterButton(string permissionName, Asset<Texture2D> texture, Action buttonClickedAction, Action<bool> groupUpdated, Func<string> tooltip)
		{
			if (!Main.dedServ)
			{
				callButtons.Add(new(permissionName, texture, buttonClickedAction,groupUpdated, tooltip));
			}
		}

		// TODO, is this ok to do on load rather than on enter?
		public static void LoadAddServices()
		{

			ServiceController.AddService(new BuffServiceFisico());
			ServiceController.AddService(new BuffServiceSocial());
			ServiceController.AddService(new BuffServiceCognitivo());
			ServiceController.AddService(new BuffServiceAfectivo());

			ServiceController.AddService(new Login());
			ServiceHotbar.Visible = true;
		}

		public static void Update(/*GameTime gameTime*/)
		{
			if (ModUtils.NetworkMode != NetworkMode.Server)
			{
				ModUtils.PreviousKeyboardState = Main.keyState;
				ModUtils.PreviousMouseState = ModUtils.MouseState;
				ModUtils.MouseState = Mouse.GetState();

				ModUtils.SetDeltaTime(/*gameTime*/);
				ModUtils.Update();

				//Update all services in the ServiceController
				foreach (var service in ServiceController.Services)
				{
					service.Update();
				}
				MasterView.UpdateMaster();
			}
		}

		public static void GameEntered()
		{
			ModUtils.DebugText("Game Entered");

			if (ModUtils.NetworkMode == NetworkMode.None)
			{
				foreach (bGamesModService service in ServiceController.Services)
				{
					service.HasPermissionToUse = !service.MultiplayerOnly;
				}
				ServiceController.ServiceRemovedCall();
			}
			else
			{
				foreach (bGamesModService service in ServiceController.Services)
				{
					service.HasPermissionToUse = true;
				}
			}
		}

		public static void GameLeft()
		{
			ModUtils.DebugText("Game left");
			Login.LoggedIn = false;
		}

		public static void Draw(SpriteBatch spriteBatch)
		{
			UIKit.MasterView.DrawMaster(spriteBatch);
			{
				foreach (var service in ServiceController.Services)
				{
					service.Draw(spriteBatch);
				}
			}

			if (!string.IsNullOrEmpty(UIView.HoverText))
			{
				Main.hoverItemName = UIView.HoverText;
			}
		}
	}
}