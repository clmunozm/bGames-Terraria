using bGamesMod.UIKit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;

namespace bGamesMod.bGamesModServices
{
	internal class BuffServiceAfectivo : bGamesModService
	{
		private BuffWindowAfectivo _buffWindow;

		public static int[] SkipBuffs = new int[] {
			BuffID.Pygmies, BuffID.LeafCrystal, BuffID.IceBarrier, BuffID.BabySlime, BuffID.Ravens, BuffID.BeetleEndurance1, BuffID.BeetleEndurance2, BuffID.BeetleEndurance3,
			BuffID.BeetleMight1, BuffID.BeetleMight2, BuffID.BeetleMight3, BuffID.ImpMinion, BuffID.SpiderMinion, BuffID.TwinEyesMinion,
			BuffID.MinecartLeft, BuffID.MinecartLeftMech, BuffID.MinecartLeftWood, BuffID.MinecartRight, BuffID.MinecartRightMech, BuffID.MinecartRightWood,
			BuffID.SharknadoMinion, BuffID.UFOMinion, BuffID.DeadlySphere, BuffID.SolarShield1, BuffID.SolarShield2, BuffID.SolarShield3, BuffID.StardustDragonMinion,
			BuffID.StardustGuardianMinion, BuffID.HornetMinion, BuffID.PirateMinion, BuffID.StardustMinion };
		public BuffServiceAfectivo()
		{
			this._hotbarIcon = new UIImage(bGamesMod.instance.Assets.Request<Texture2D>("Images/bGames/afective", AssetRequestMode.ImmediateLoad));
			this.HotbarIcon.Tooltip = bGamesMod.HeroText("OpenBuffAfectivo");
			this.HotbarIcon.onLeftClick += HotbarIcon_onLeftClick;

			Login.OnLogout += HandleLogout;
		}

		private void HandleLogout()
		{
			if (_buffWindow != null)
			{
				_buffWindow.Visible = false;
			}
		}

		public override void MyGroupUpdated()
		{
			if (!HasPermissionToUse && _buffWindow != null)
			{
				_buffWindow.Visible = false;
			}
		}

		private void HotbarIcon_onLeftClick(object sender, EventArgs e)
		{
			// Verificar si el usuario ha iniciado sesión
			if (!Login.LoggedIn)
			{
				Main.NewText(bGamesMod.HeroText("BuffsLogin"));
				return;
			}
			if (_buffWindow == null)
			{
				if (!Main.dedServ)
				{
					_buffWindow = new BuffWindowAfectivo(Login.UserID);
					this.AddUIView(_buffWindow);
					_buffWindow.Visible = false;

					_buffWindow.Y = 270;
					_buffWindow.X = 130;
				}
			}
			_buffWindow.Visible = !_buffWindow.Visible;
			if (_buffWindow.Visible)
			{
				_buffWindow.LoadPlayerAttributes(Login.UserID);
			}
		}
	}

	internal class BuffWindowAfectivo : UIWindow
	{
		private UILabel lblPoints;
		private int puntos;
		private string userID;

		public BuffWindowAfectivo(string userID)
		{
			this.userID = userID;
			this.CanMove = true;
			UILabel lTitle = new UILabel(bGamesMod.HeroText("BuffsAfectivo"));
			UILabel lSeconds = new UILabel("Puntos");

			lblPoints = new UILabel("Loading...");
			UIScrollView scrollView = new UIScrollView();
			UIImage bClose = new UIImage(closeTexture);

			lTitle.Scale = .6f;
			lTitle.X = Spacing;
			lTitle.Y = Spacing;
			lTitle.OverridesMouse = false;

			bClose.Y = Spacing;
			bClose.onLeftClick += bClose_onLeftClick;

			lblPoints.Scale = .6f;
			lblPoints.Width = 75;
			lblPoints.Y = lTitle.Y + lTitle.Height;

			scrollView.X = lTitle.X;
			scrollView.Y = lblPoints.Y + lblPoints.Height + Spacing;
			scrollView.Width = 300;
			scrollView.Height = 250;

			float yPos = Spacing;
			int[] affectiveBuffs = new int[] {
			BuffID.WellFed, BuffID.Lovestruck, BuffID.Sunflower, BuffID.WellFed2, BuffID.WellFed3, BuffID.StarInBottle,
			BuffID.PeaceCandle
			};

			foreach (int buffType in affectiveBuffs)
			{
				UIRect bg = new UIRect();
				bg.ForegroundColor = buffType % 2 == 0 ? Color.Transparent : Color.Blue * .1f;
				bg.X = Spacing;
				bg.Y = yPos;
				bg.Width = scrollView.Width - 20 - Spacing * 2;
				bg.Tag = buffType;
				string buffDescription = Lang.GetBuffDescription(buffType);
				bg.Tooltip = (buffDescription == null ? "" : buffDescription);
				bg.onLeftClick += bg_onLeftClick;

				UIImage buffImage = new UIImage(TextureAssets.Buff[buffType]);
				buffImage.X = Spacing;
				buffImage.Y = SmallSpacing / 2;
				buffImage.OverridesMouse = false;

				bg.Height = buffImage.Height + SmallSpacing;
				yPos += bg.Height;

				UILabel label = new UILabel(Lang.GetBuffName(buffType));
				label.Scale = .4f;
				label.Anchor = AnchorPosition.Left;
				label.X = buffImage.X + buffImage.Width + Spacing;
				label.Y = buffImage.Y + buffImage.Height / 2;
				label.OverridesMouse = false;

				bg.AddChild(buffImage);
				bg.AddChild(label);
				scrollView.AddChild(bg);
			}

			scrollView.ContentHeight = yPos;

			this.Width = scrollView.X + scrollView.Width + Spacing;
			this.Height = scrollView.Y + scrollView.Height + Spacing;

			lblPoints.X = Width - lblPoints.Width - Spacing;
			bClose.X = Width - bClose.Width - Spacing;

			lSeconds.Scale = .4f;
			lSeconds.Anchor = AnchorPosition.Right;
			lSeconds.X = lblPoints.X - Spacing;
			lSeconds.Y = lblPoints.Y + lblPoints.Height / 2;

			AddChild(lTitle);
			AddChild(lSeconds);
			AddChild(lblPoints);
			AddChild(scrollView);
			AddChild(bClose);
		}

		public async void LoadPlayerAttributes(string userID)
		{
			try
			{
				using (var client = new HttpClient())
				{
					var response = await client.GetStringAsync("http://localhost:3001/player_all_attributes/" + userID);
					var attributes = JsonConvert.DeserializeObject<List<PlayerAttribute>>(response);
					if (attributes != null && attributes.Any())
					{
						puntos = attributes[2].data;
						lblPoints.Text = puntos.ToString();
					}
				}
			}
			catch (Exception ex)
			{
				Main.NewText($"Error loading attributes: {ex.Message}");
			}
		}

		private async void SpendPoints(int buffType, int pointsToSpend)
		{
			try
			{
				using (var client = new HttpClient())
				{
					var postData = new {
						id_player = userID,
						id_videogame = 5,
						id_attributes = 2,
						new_data = pointsToSpend
					};

					var json = JsonConvert.SerializeObject(postData);
					var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
					var response = await client.PostAsync("http://localhost:3002/spend_attribute/", content);
					if (response.IsSuccessStatusCode)
					{
						LoadPlayerAttributes(userID);
					}
					else
					{
						Main.NewText($"Error spending points: {response.ReasonPhrase}");
					}
				}
			}
			catch (Exception ex)
			{
				Main.NewText($"Error spending points: {ex.Message}");
			}
		}

		private void bClose_onLeftClick(object sender, EventArgs e)
		{
			this.Visible = false;
		}

		private void bg_onLeftClick(object sender, EventArgs e)
		{
			UIView view = (UIView)sender;
			int buffType = (int)view.Tag;
			int pointsToSpend = 5; // You can modify this value as needed

			if (pointsToSpend <= puntos)
			{
				Main.player[Main.myPlayer].AddBuff(buffType, 60 * 60); // 60 seconds
				SpendPoints(buffType, pointsToSpend);
			}
			else
			{
				Main.NewText("Not enough points to spend.");
			}
		}

		internal class PlayerAttribute
		{
			public int id_attributes { get; set; }
			public string name { get; set; }
			public int data { get; set; }
		}
	}
}
