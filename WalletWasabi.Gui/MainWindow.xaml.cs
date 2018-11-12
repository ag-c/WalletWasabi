using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvalonStudio.Extensibility;
using AvalonStudio.Extensibility.Theme;
using AvalonStudio.Shell;
using AvalonStudio.Shell.Controls;
using WalletWasabi.Gui.Tabs.WalletManager;
using WalletWasabi.Gui.ViewModels;

namespace WalletWasabi.Gui
{
	public class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				HasSystemDecorations = true;

				// This will need implementing properly once this is supported by avalonia itself.
				var color = (ColorTheme.CurrentTheme.Background as SolidColorBrush).Color;
				(PlatformImpl as Avalonia.Native.WindowImpl).SetTitleBarColor(color);
			}
		}

		private void InitializeComponent()
		{
			Activated += OnActivated;
			Closing += MainWindow_ClosingAsync;
			AvaloniaXamlLoader.Load(this);
		}

		private async void MainWindow_ClosingAsync(object sender, CancelEventArgs e)
		{
			UiConfig conf = Global.UiConfig;
			conf.WindowState = WindowState;
			conf.Width = Width;
			conf.Height = Height;

			await conf.ToFileAsync();
		}

#pragma warning disable IDE1006 // Naming Styles

		private async void OnActivated(object sender, EventArgs e)
#pragma warning restore IDE1006 // Naming Styles
		{
			Activated -= OnActivated;
			DisplayWalletManager();
			var uiConfigFilePath = Path.Combine(Global.DataDir, "UiConfig.json");
			var uiConfig = new UiConfig(uiConfigFilePath);
			await uiConfig.LoadOrCreateDefaultFileAsync();
			Logging.Logger.LogInfo<UiConfig>("UiConfig is successfully initialized.");
			Global.InitializeUiConfig(uiConfig);
			MainWindowViewModel.Instance.RefreshUiFromConfig(Global.UiConfig);
		}

		private void DisplayWalletManager()
		{
			var isAnyWalletAvailable = Directory.Exists(Global.WalletsDir) && Directory.EnumerateFiles(Global.WalletsDir).Any();

			var walletManagerViewModel = new WalletManagerViewModel();
			IoC.Get<IShell>().AddDocument(walletManagerViewModel);

			if (isAnyWalletAvailable)
			{
				walletManagerViewModel.SelectLoadWallet();
			}
			else
			{
				walletManagerViewModel.SelectGenerateWallet();
			}
		}
	}
}
