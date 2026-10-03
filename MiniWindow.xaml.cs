using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace SearchBrowser
{
    public partial class MiniWindow : Window
    {
        private MainWindow _mainWindow;
        private BrowserTab _tab;
        
        public MiniWindow(MainWindow mainWindow, BrowserTab tab)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _tab = tab;
            
            // Move WebView2 to this window
            _mainWindow.WebViewsContainer.Children.Remove(_tab.WebView);
            WebViewContainer.Children.Add(_tab.WebView);
            _tab.WebView.Visibility = Visibility.Visible;
            
            // Position at bottom right
            var desktop = SystemParameters.WorkArea;
            this.Left = desktop.Right - this.Width - 20;
            this.Top = desktop.Bottom - this.Height - 20;
        }
        
        private void DragBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            ReturnToMain();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            ReturnToMain();
            base.OnClosed(e);
        }
        
        private bool _returned = false;
        private void ReturnToMain()
        {
            if (_returned) return;
            _returned = true;
            
            WebViewContainer.Children.Remove(_tab.WebView);
            _mainWindow.ReturnFromMiniWindow(_tab);
            this.Close();
        }
    }
}
