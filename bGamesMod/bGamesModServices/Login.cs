using bGamesMod.UIKit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using System.Net.Http;

namespace bGamesMod.bGamesModServices
{
	internal class Login : bGamesModService
	{
		private static bool _loggedIn = false;
		public static string userID = "null";
		public static event Action OnLogout;

		public static string UserID 
		{ 
			get { return userID; }
		}

		public static bool LoggedIn
		{
			get { return _loggedIn; }
			set
			{
				_loggedIn = value;
				LoginStatusChanged?.Invoke(null, EventArgs.Empty);
			}
		}

		internal static Asset<Texture2D> _loginTexture;
		internal static Asset<Texture2D> _logoutTexture;

		private static event EventHandler LoginStatusChanged;

		public Login()
		{
			MultiplayerOnly = true;
			if (_loginTexture == null)
			{
				_loginTexture = bGamesMod.instance.Assets.Request<Texture2D>("Images/login", AssetRequestMode.ImmediateLoad);
			}
			if (_logoutTexture == null)
			{
				_logoutTexture = bGamesMod.instance.Assets.Request<Texture2D>("Images/logout", AssetRequestMode.ImmediateLoad);
			}
			this._name = "Login";
			this._hotbarIcon = new UIImage(_loginTexture);
			this._hotbarIcon.onLeftClick += _hotbarIcon_onLeftClick;
			LoginStatusChanged += Login_LoginStatusChanged;
			this.HotbarIcon.Tooltip = bGamesMod.HeroText("Login");
			this.HasPermissionToUse = true;
		}

		private void Login_LoginStatusChanged(object sender, EventArgs e)
		{
			//ErrorLogger.Log("Login_LoginStatusChanged to "+ LoggedIn);
			if (LoggedIn)
			{
				this._hotbarIcon.Texture = _logoutTexture;
				this.HotbarIcon.Tooltip = bGamesMod.HeroText("Logout");
			}
			else
			{
				this._hotbarIcon.Texture = _loginTexture;
				this.HotbarIcon.Tooltip = bGamesMod.HeroText("Login");
			}
		}

		private void _hotbarIcon_onLeftClick(object sender, EventArgs e)
		{
			if (LoggedIn || userID != "null")
			{
				Main.NewText(bGamesMod.HeroText("LogoutSuccessful"));
				OnLogout?.Invoke();
				Login.userID = "null";
				Login.LoggedIn = false;
			}
			else
			{
				MasterView.gameScreen.AddChild(new LoginWindow());
			}
		}

		public override void Destroy()
		{
			//ErrorLogger.Log("Destroy");
			LoginStatusChanged -= Login_LoginStatusChanged;
			LoggedIn = false;
			base.Destroy();
		}
	}

	internal class LoginWindow : UIWindow
	{
		private UILabel lPassword = null;
		private UITextbox tbPassword = null;
		private UITextbox tbUsername = null;
		private UILabel lUsername = null;
		private UILabel lSaveLogin = null;
		private UIButton bSaveNone = null;
		private UIButton bSaveDefault = null;
		private UIButton bSavePlayer = null;
		private UICheckbox cbRememberPassword = null;
		private Color originalBGColor;
		private Color selectedBGColor;
		private LoginStorage loginStorage;
		private LoginSaveType saveType = LoginSaveType.None;
		private static float spacing = 16f;

		public LoginWindow()
		{
			UIView.exclusiveControl = this;

			Width = 600;
			this.Anchor = AnchorPosition.Center;

			lUsername = new UILabel(bGamesMod.HeroText("Username"));
			tbUsername = new UITextbox();
			lPassword = new UILabel(bGamesMod.HeroText("Password"));
			tbPassword = new UITextbox();
			tbPassword.PasswordBox = true;
			lSaveLogin = new UILabel(bGamesMod.HeroText("SaveLogin"));
			bSaveNone = new UIButton(bGamesMod.HeroText("SaveLoginNone"));
			bSaveDefault = new UIButton(bGamesMod.HeroText("SaveLoginDefault"));
			bSavePlayer = new UIButton(bGamesMod.HeroText("SaveLoginPlayer"));
			cbRememberPassword = new UICheckbox(bGamesMod.HeroText("RememberPassword"));
			UIButton bLogin = new UIButton(bGamesMod.HeroText("Login"));
			UIButton bCancel = new UIButton(bGamesMod.HeroText("Cancel"));

			originalBGColor = bSaveNone.BackgroundColor;
			selectedBGColor = new Color(68, 72, 179);

			// Begin loading "Remember Me" data
			loginStorage = new LoginStorage();
			if (loginStorage.LoadJSON())
			{
				LoginInfo userInfo;
				string destinationServer = RetrieveDestinationServer();

				userInfo = loginStorage.GetLogin(destinationServer, Main.player[Main.myPlayer].name);

				SetToggle(userInfo.GetSaveType());

				if (userInfo.Username != "")
				{
					tbUsername.Text = userInfo.Username;
				}

				if (userInfo.Password != "")
				{
					tbPassword.Text = userInfo.Password;
					cbRememberPassword.Selected = true;
				}
			}
			else
			{
				SetToggle(LoginSaveType.None);
			}

			lUsername.Scale = .5f;
			lPassword.Scale = .5f;
			lSaveLogin.Scale = .5f;

			bLogin.Anchor = AnchorPosition.TopRight;
			bCancel.Anchor = AnchorPosition.TopRight;

			tbUsername.Width = 300;
			tbPassword.Width = tbUsername.Width;
			lUsername.X = spacing;
			lUsername.Y = spacing;
			tbUsername.X = lUsername.X + lSaveLogin.Width + spacing;
			tbUsername.Y = lUsername.Y;
			lPassword.X = lUsername.X;
			lPassword.Y = lUsername.Y + lUsername.Height + spacing;
			tbPassword.X = tbUsername.X;
			tbPassword.Y = lPassword.Y;
			lSaveLogin.X = lUsername.X;
			lSaveLogin.Y = lPassword.Y + lPassword.Height + spacing;
			bSaveNone.X = lSaveLogin.X + lSaveLogin.Width + spacing;
			bSaveNone.Y = lSaveLogin.Y;
			bSaveDefault.X = bSaveNone.X + bSaveNone.Width;
			bSaveDefault.Y = bSaveNone.Y;
			bSavePlayer.X = bSaveDefault.X + bSaveDefault.Width;
			bSavePlayer.Y = bSaveDefault.Y;
			cbRememberPassword.X = bSaveNone.X;
			cbRememberPassword.Y = lSaveLogin.Y + lSaveLogin.Height + spacing;

			bCancel.Position = new Vector2(this.Width - spacing, cbRememberPassword.Y + cbRememberPassword.Height + spacing);
			bLogin.Position = new Vector2(bCancel.Position.X - bCancel.Width - spacing - lSaveLogin.Width / 2, bCancel.Position.Y);
			this.Height = bCancel.Y + bCancel.Height + spacing;
			
			bSaveNone.Tooltip = bGamesMod.HeroText("SaveLoginNoneTooltip");
			bSaveDefault.Tooltip = bGamesMod.HeroText("SaveLoginDefaultTooltip");
			bSavePlayer.Tooltip = bGamesMod.HeroText("SaveLoginPlayerTooltip");

			bCancel.onLeftClick += bCancel_onLeftClick;
			bLogin.onLeftClick += bLogin_onLeftClick;
			tbUsername.OnEnterPress += bLogin_onLeftClick;
			tbPassword.OnEnterPress += bLogin_onLeftClick;
			tbUsername.OnTabPress += tbUsername_OnTabPress;
			tbPassword.OnTabPress += tbPassword_OnTabPress;
			bSaveNone.onLeftClick += BSaveNone_onLeftClick;
			bSaveDefault.onLeftClick += BSaveDefault_onLeftClick;
			bSavePlayer.onLeftClick += BSavePlayer_onLeftClick;
			cbRememberPassword.onLeftClick += BRememberPassword_onLeftClick;

			AddChild(lUsername);
			AddChild(tbUsername);
			AddChild(lPassword);
			AddChild(tbPassword);
			AddChild(lSaveLogin);
			AddChild(bSaveNone);
			AddChild(bSaveDefault);
			AddChild(bSavePlayer);
			AddChild(cbRememberPassword);
			AddChild(bLogin);
			AddChild(bCancel);

			if (tbUsername.Text != "")
			{
				tbPassword.Focus();
			}
			else
			{
				tbUsername.Focus();
			}
		}

		private void SetToggle(LoginSaveType _saveType)
		{
			saveType = _saveType;

			UIButton[] buttons = { bSaveNone, bSaveDefault, bSavePlayer };

			foreach (var button in buttons)
			{
				button.SetBackgroundColor(originalBGColor);
			}

			if (_saveType == LoginSaveType.None)
			{
				bSaveNone.SetBackgroundColor(selectedBGColor);
				cbRememberPassword.Selected = false;
			}
			else if (_saveType == LoginSaveType.Default)
			{
				bSaveDefault.SetBackgroundColor(selectedBGColor);
			}
			else if (_saveType == LoginSaveType.Player)
			{
				bSavePlayer.SetBackgroundColor(selectedBGColor);
			}
		}

		private void BSaveNone_onLeftClick(object sender, EventArgs e)
			=> SaveTypeToggle_onLeftClick(sender, e, LoginSaveType.None);

		private void BSaveDefault_onLeftClick(object sender, EventArgs e)
			=> SaveTypeToggle_onLeftClick(sender, e, LoginSaveType.Default);

		private void BSavePlayer_onLeftClick(object sender, EventArgs e)
			=> SaveTypeToggle_onLeftClick(sender, e, LoginSaveType.Player);

		private void SaveTypeToggle_onLeftClick(object sender, EventArgs e, LoginSaveType saveType)
			=> SetToggle(saveType);

		private void BRememberPassword_onLeftClick(object sender, EventArgs e)
		{
			if (saveType == LoginSaveType.None)
				SetToggle(LoginSaveType.Default);
		}


		/*private void bRegister_onLeftClick(object sender, EventArgs e)
		{
			if (tbUsername.Text.Length > 0 && tbPassword.Text.Length > 0)
			{
				tbUsername.Unfocus();
				tbPassword.Unfocus();
				SaveLogin();
				//HEROsModNetwork.LoginService.RequestRegistration(tbUsername.Text, tbPassword.Text);
				Close();
			}
			else
			{
				Main.NewText(HEROsMod.HeroText("PleaseFillInUsernamePassword"));
			}
		}*/

		private void tbPassword_OnTabPress(object sender, EventArgs e)
		{
			tbPassword.Unfocus();
			tbUsername.Focus();
		}

		private void tbUsername_OnTabPress(object sender, EventArgs e)
		{
			tbUsername.Unfocus();
			tbPassword.Focus();
		}

		private async void bLogin_onLeftClick(object sender, EventArgs e)
		{
			if (tbUsername.Text.Length > 0 && tbPassword.Text.Length > 0)
			{
				tbUsername.Unfocus();
				tbPassword.Unfocus();
				SaveLogin();

				string username = tbUsername.Text;
				string password = tbPassword.Text;
				string url = $"http://localhost:3010/player/{username}/{password}";

				using (var httpClient = new HttpClient())
				{
					try
					{
						var response = await httpClient.GetAsync(url);
						if (response.IsSuccessStatusCode)
						{
							Login.userID = await response.Content.ReadAsStringAsync();
							// Aquí puedes almacenar el userID según tus necesidades
							// Por ejemplo, podrías guardarlo en una variable estática o pasarlo a otro método
							Main.NewText(bGamesMod.HeroText("LoginSuccessful"));
							Login.LoggedIn = true;
							Close();
						}
						else
						{
							Main.NewText(bGamesMod.HeroText("InvalidCredentials"));
						}
					}
					catch (Exception ex)
					{
						Main.NewText(bGamesMod.HeroText("FailedConnectServer") + $"{ex.Message}");
					}
				}
			}
			else
			{
				Main.NewText(bGamesMod.HeroText("FillUserPassword"));
			}
		}

		private string RetrieveDestinationServer()
		{
			if (Netplay.ServerIP != null && Netplay.ListenPort != 0)
			{
				return Netplay.ServerIP.ToString() + ":" + Netplay.ListenPort.ToString();
			}
			else if (Main.LobbyId > 0)
			{
				return Main.LobbyId.ToString();
			}
			else if (Main.worldName != null && Main.worldName != "")
			{
				return Main.worldName;
			}
			else
			{
				return "";
			}
		}

		private void SaveLogin()
		{
			var username = "";
			var password = "";

			username = tbUsername.Text;

			if (cbRememberPassword.Selected)
				password = tbPassword.Text;

			string destinationServer = RetrieveDestinationServer();

			if (saveType == LoginSaveType.Default)
			{
				loginStorage.AddLogin(destinationServer, username, password);
			}
			else if (saveType == LoginSaveType.Player)
			{
				loginStorage.AddLogin(destinationServer, Main.player[Main.myPlayer].name, username, password);
			}
			else if (saveType == LoginSaveType.None)
			{
				loginStorage.RemoveLogin(destinationServer, Main.player[Main.myPlayer].name);
			}

			loginStorage.SaveJSON();
		}

		private void bCancel_onLeftClick(object sender, EventArgs e)
		{
			this.Close();
		}

		protected override float GetWidth()
		{
			return tbPassword.Width + lPassword.Width + spacing * 4;
		}

		private void Close()
		{
			UIView.exclusiveControl = null;
			this.Parent.RemoveChild(this);
		}

		public override void Update()
		{
			if (Main.gameMenu) this.Close();
			if (Parent != null)
				this.Position = new Vector2(Parent.Width / 2, Parent.Height / 2);
			base.Update();
		}
	}
}