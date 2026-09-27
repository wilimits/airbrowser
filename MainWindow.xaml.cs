using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using System.Linq;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace SearchBrowser
{
    public class BrowserTab : INotifyPropertyChanged
    {
        public bool IsPrivate { get; set; }
        private string _title = "Loading...";
        private string _faviconUri = "";
        
        public string Title 
        { 
            get => _title; 
            set { _title = value; OnPropertyChanged(); } 
        }
        
        public string FaviconUri 
        { 
            get => _faviconUri; 
            set { _faviconUri = value; OnPropertyChanged(); } 
        }
        
        public string Url { get; set; }
        public WebView2 WebView { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class Bookmark
    {
        public required string Url { get; set; }
        public required string FaviconUri { get; set; }
        public string Title { get; set; } = "";
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<BrowserTab> Tabs { get; set; } = new ObservableCollection<BrowserTab>();
        public ObservableCollection<Bookmark> Bookmarks { get; set; } = new ObservableCollection<Bookmark>();
        private BrowserTab _currentTab;

        public MainWindow()
        {
            InitializeComponent();
            TabsList.ItemsSource = Tabs;
            BookmarksList.ItemsSource = Bookmarks;

            this.StateChanged += MainWindow_StateChanged;
            this.Closing += (s, e) => SaveState();

            LoadState();
            PasswordImporter.LoadCredentials();
        }

        private string GetAppDataFolder()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AirBrowser");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private void SaveState()
        {
            try
            {
                string dir = GetAppDataFolder();
                var options = new JsonSerializerOptions { WriteIndented = true };
                
                File.WriteAllText(Path.Combine(dir, "bookmarks.json"), JsonSerializer.Serialize(Bookmarks, options));
                
                var tabsData = Tabs.Where(t => !t.IsPrivate).Select(t => new { t.Title, t.Url }).ToList();
                File.WriteAllText(Path.Combine(dir, "tabs.json"), JsonSerializer.Serialize(tabsData, options));
            }
            catch { }
        }

        private void LoadState()
        {
            try
            {
                string dir = GetAppDataFolder();
                string bookmarksFile = Path.Combine(dir, "bookmarks.json");
                if (File.Exists(bookmarksFile))
                {
                    var loadedBookmarks = JsonSerializer.Deserialize<List<Bookmark>>(File.ReadAllText(bookmarksFile));
                    if (loadedBookmarks != null)
                    {
                        foreach (var b in loadedBookmarks) Bookmarks.Add(b);
                    }
                }
                else
                {
                    Bookmarks.Add(new Bookmark { Url = "https://x.com", FaviconUri = "https://abs.twimg.com/favicons/twitter.3.ico", Title = "X (Twitter)" });
                    Bookmarks.Add(new Bookmark { Url = "https://netflix.com", FaviconUri = "https://assets.nflxext.com/us/ffe/siteui/common/icons/nficon2016.ico", Title = "Netflix" });
                    Bookmarks.Add(new Bookmark { Url = "https://figma.com", FaviconUri = "https://static.figma.com/app/icon/1/favicon.png", Title = "Figma" });
                    Bookmarks.Add(new Bookmark { Url = "https://shopify.com", FaviconUri = "https://cdn.shopify.com/static/shopify-favicon.png", Title = "Shopify" });
                }

                string tabsFile = Path.Combine(dir, "tabs.json");
                bool loadedAny = false;
                if (File.Exists(tabsFile))
                {
                    using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(tabsFile));
                    foreach (JsonElement el in doc.RootElement.EnumerateArray())
                    {
                        string url = el.TryGetProperty("Url", out var u) ? u.GetString() : "";
                        if (!string.IsNullOrEmpty(url)) 
                        {
                            AddNewTab(url);
                            loadedAny = true;
                        }
                    }
                }
                
                if (!loadedAny) AddNewTab("https://www.google.com");
            }
            catch 
            {
                if (Tabs.Count == 0) AddNewTab("https://www.google.com");
            }
        }

        private void BookmarkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTab != null && !string.IsNullOrEmpty(_currentTab.Url))
            {
                Bookmarks.Add(new Bookmark 
                { 
                    Url = _currentTab.Url, 
                    FaviconUri = _currentTab.FaviconUri ?? "",
                    Title = _currentTab.Title
                });
            }
        }

        private void Bookmark_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Bookmark bookmark)
            {
                if (_currentTab != null && _currentTab.WebView?.CoreWebView2 != null)
                {
                    _currentTab.WebView.CoreWebView2.Navigate(bookmark.Url);
                }
                else
                {
                    AddNewTab(bookmark.Url);
                }
            }
        }

        private void DeleteBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Bookmark bookmark)
            {
                Bookmarks.Remove(bookmark);
            }
        }

        private void ImportBookmarks_Click(object sender, RoutedEventArgs e)
        {
            int chromeCount = ImportFromBrowser(@"Google\Chrome\User Data\Default");
            int edgeCount = ImportFromBrowser(@"Microsoft\Edge\User Data\Default");
            
            // Also try Profile 1
            if (chromeCount <= 0) chromeCount = ImportFromBrowser(@"Google\Chrome\User Data\Profile 1");
            if (edgeCount <= 0) edgeCount = ImportFromBrowser(@"Microsoft\Edge\User Data\Profile 1");

            if (chromeCount == 0 && edgeCount == 0)
            {
                MessageBox.Show("We couldn't find Google Chrome or Microsoft Edge bookmarks on this PC.", "No Bookmarks Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                int total = chromeCount + edgeCount;
                MessageBox.Show($"Successfully imported {total} bookmarks!", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ImportPasswords_Click(object sender, RoutedEventArgs e)
        {
            var instructionDialog = new ImportDialog();
            instructionDialog.Owner = this;
            
            if (instructionDialog.ShowDialog() != true) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select Chrome Passwords CSV",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    int count = PasswordImporter.ImportFromCsv(dialog.FileName);
                    if (count > 0)
                    {
                        MessageBox.Show($"Successfully imported {count} passwords!", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No valid passwords found in the selected file.", "Import Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to import passwords: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private int ImportFromBrowser(string relativeDir)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dirPath = Path.Combine(localAppData, relativeDir);

            if (!Directory.Exists(dirPath)) return 0;

            string[] possibleFiles = { "Bookmarks", "AccountBookmarks" };
            int count = 0;

            foreach (string file in possibleFiles)
            {
                string path = Path.Combine(dirPath, file);
                if (File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        using JsonDocument doc = JsonDocument.Parse(json);
                        JsonElement root = doc.RootElement;
                        
                        if (root.TryGetProperty("roots", out JsonElement roots))
                        {
                            if (roots.TryGetProperty("bookmark_bar", out JsonElement bookmarkBar))
                                count += ParseBookmarkNode(bookmarkBar);
                            if (roots.TryGetProperty("other", out JsonElement other))
                                count += ParseBookmarkNode(other);
                        }
                    }
                    catch { }
                }
            }
            return count;
        }

        private int ParseBookmarkNode(JsonElement node)
        {
            int added = 0;
            if (node.TryGetProperty("type", out JsonElement typeElement))
            {
                string type = typeElement.GetString() ?? "";
                if (type == "url")
                {
                    string url = node.GetProperty("url").GetString() ?? "";
                    string name = node.GetProperty("name").GetString() ?? "";
                    
                    if (!Bookmarks.Any(b => b.Url == url))
                    {
                        string host = "";
                        try { host = new Uri(url).Host; } catch { }
                        string favicon = string.IsNullOrEmpty(host) ? "" : $"https://www.google.com/s2/favicons?domain={host}&sz=32";
                        Bookmarks.Add(new Bookmark { Url = url, FaviconUri = favicon, Title = name });
                        added++;
                    }
                }
                else if (type == "folder" && node.TryGetProperty("children", out JsonElement children))
                {
                    foreach (JsonElement child in children.EnumerateArray())
                        added += ParseBookmarkNode(child);
                }
            }
            return added;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                RootLayout.Margin = new Thickness(8);
            }
            else
            {
                RootLayout.Margin = new Thickness(0);
            }
        }

        // AdBlock Domains
        private static readonly string[] AdDomains = new[]
        {
            "doubleclick.net", "googleadservices.com", "googlesyndication.com",
            "adsystem.com", "yandex.ru/ads", "an.yandex.ru", "mc.yandex.ru",
            "pagead2", "ads.twitter.com", "analytics", "tracking", "pixel"
        };

        private async void AddNewTab(string url, bool isPrivate = false)
        {
            var webView = new WebView2();
            if (isPrivate)
            {
                webView.CreationProperties = new CoreWebView2CreationProperties
                {
                    IsInPrivateModeEnabled = true,
                    ProfileName = "PrivateSession"
                };
            }

            WebViewsContainer.Children.Add(webView);
            
            webView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                {
                    webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
                    webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;

                    // Network-level AdBlock
                    webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                    webView.CoreWebView2.WebResourceRequested += (sender, args) =>
                    {
                        try
                        {
                            Uri uri = new Uri(args.Request.Uri);
                            string hostAndPath = uri.Host.ToLower() + uri.AbsolutePath.ToLower();
                            if (AdDomains.Any(domain => hostAndPath.Contains(domain)))
                            {
                                var env = webView.CoreWebView2.Environment;
                                args.Response = env.CreateWebResourceResponse(null, 403, "Blocked by Air AdBlock", "");
                            }
                        }
                        catch { }
                    };

                    webView.CoreWebView2.DocumentTitleChanged += (sender, args) =>
                    {
                        if (Tabs.FirstOrDefault(t => t.WebView == webView) is BrowserTab tab)
                            tab.Title = string.IsNullOrWhiteSpace(webView.CoreWebView2.DocumentTitle) ? "Untitled" : webView.CoreWebView2.DocumentTitle;
                    };

                    webView.CoreWebView2.FaviconChanged += (sender, args) =>
                    {
                        if (Tabs.FirstOrDefault(t => t.WebView == webView) is BrowserTab tab)
                            tab.FaviconUri = webView.CoreWebView2.FaviconUri;
                    };

                    webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
                    {
                        if (Tabs.FirstOrDefault(t => t.WebView == webView) is BrowserTab tab)
                        {
                            tab.Url = webView.Source?.ToString() ?? tab.Url;
                            if (_currentTab == tab)
                                UrlTextBox.Text = webView.Source?.ToString();
                        }

                        // Autofill injected passwords
                        try
                        {
                            string host = webView.Source?.Host ?? "";
                            string searchHost = host.Replace("www.", "");
                            var cred = PasswordImporter.SavedCredentials.FirstOrDefault(c => c.Url.Contains(searchHost));
                            
                            if (cred != null)
                            {
                                string safeUser = cred.Username.Replace("'", "\\'").Replace("\n", "");
                                string safePass = cred.Password.Replace("'", "\\'").Replace("\n", "");
                                
                                string js = $@"
                                    (function() {{
                                        let attempts = 0;
                                        let timer = setInterval(() => {{
                                            attempts++;
                                            if (attempts > 20) {{ clearInterval(timer); return; }} // Try for 10 seconds
                                            
                                            let inputs = Array.from(document.querySelectorAll('input'));
                                            let u = inputs.find(i => (i.type === 'text' || i.type === 'email' || i.type === 'tel' || (i.name && i.name.toLowerCase().includes('user'))) && !i.disabled && i.offsetParent !== null);
                                            let p = inputs.find(i => i.type === 'password' && !i.disabled && i.offsetParent !== null);
                                            
                                            if (u && p && !p.value) {{
                                                // Native setter for React/Vue sites
                                                let nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
                                                
                                                nativeInputValueSetter.call(u, '{safeUser}');
                                                u.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                                u.dispatchEvent(new Event('change', {{ bubbles: true }}));

                                                nativeInputValueSetter.call(p, '{safePass}');
                                                p.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                                p.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                                
                                                clearInterval(timer);
                                            }}
                                        }}, 500);
                                    }})();
                                ";
                                await webView.ExecuteScriptAsync(js);
                            }
                        }
                        catch { }
                    };
                }
            };
            
            await webView.EnsureCoreWebView2Async(null);
            
            var newTab = new BrowserTab { Url = url, WebView = webView, IsPrivate = isPrivate };
            if (isPrivate) newTab.Title = "Private Tab";
            newTab.FaviconUri = "pack://application:,,,/SearchBrowser;component/default_favicon.png";

            Tabs.Add(newTab);
            
            webView.Source = new Uri(url);
            TabsList.SelectedItem = newTab;
        }

        private void TabsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabsList.SelectedItem is BrowserTab selectedTab)
            {
                _currentTab = selectedTab;
                foreach (WebView2 wv in WebViewsContainer.Children)
                {
                    wv.Visibility = (wv == selectedTab.WebView) ? Visibility.Visible : Visibility.Hidden;
                }
                UrlTextBox.Text = selectedTab.WebView.Source?.ToString() ?? selectedTab.Url;
            }
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BrowserTab tab)
            {
                WebViewsContainer.Children.Remove(tab.WebView);
                tab.WebView.Dispose();
                Tabs.Remove(tab);

                if (Tabs.Count == 0)
                    Close(); // Close app if last tab is closed
            }
        }

        private void NewTabBtn_Click(object sender, RoutedEventArgs e) => AddNewTab("https://www.google.com");
        private void NewPrivateTabBtn_Click(object sender, RoutedEventArgs e) => AddNewTab("https://www.google.com", true);

        private void ToggleSidebarBtn_Click(object sender, RoutedEventArgs e)
        {
            SidebarColumn.Width = SidebarColumn.Width.Value == 0 ? new GridLength(240) : new GridLength(0);
        }

        // Titlebar Buttons
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        // Navigation
        private void BackButton_Click(object sender, RoutedEventArgs e) { if (_currentTab?.WebView.CanGoBack == true) _currentTab.WebView.GoBack(); }
        private void ForwardButton_Click(object sender, RoutedEventArgs e) { if (_currentTab?.WebView.CanGoForward == true) _currentTab.WebView.GoForward(); }
        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _currentTab?.WebView.Reload();

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _currentTab != null)
            {
                string url = UrlTextBox.Text;
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    if (url.Contains(".") && !url.Contains(" ")) url = "https://" + url;
                    else url = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
                }
                _currentTab.WebView.Source = new Uri(url);
            }
        }

        private async void ReaderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTab?.WebView != null)
            {
                string script = @"
                    document.querySelectorAll('header, footer, nav, aside, .sidebar, .ads, iframe, script').forEach(el => el.style.display = 'none');
                    document.body.style.maxWidth = '700px';
                    document.body.style.margin = '0 auto';
                    document.body.style.fontFamily = 'Georgia, serif';
                    document.body.style.fontSize = '22px';
                    document.body.style.lineHeight = '1.6';
                    document.body.style.backgroundColor = '#fbfbfb';
                    document.body.style.color = '#111';
                    document.body.style.padding = '40px';
                ";
                await _currentTab.WebView.ExecuteScriptAsync(script);
            }
        }

        private void DevToolsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTab?.WebView != null)
            {
                _currentTab.WebView.CoreWebView2.OpenDevToolsWindow();
            }
        }
    }
}