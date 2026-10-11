<div align="center">
  <h1>Air Browser</h1>
  <p><b>A browser for Windows with nothing in the way.</b></p>
  
  <p>
    <a href="https://apps.microsoft.com/detail/9n1gtl34cwh9?hl=en-US&gl=SA"><img src="https://img.shields.io/badge/Microsoft_Store-Verified-0078D4?logo=windows&style=flat-square" alt="Microsoft Store"></a>
    <img src="https://img.shields.io/badge/version-v1.1.6-10b981?style=flat-square" alt="Version 1.1.6">
    <a href="https://wilimits.com/project/airbrowser/"><img src="https://img.shields.io/badge/website-wilimits.com-111827?style=flat-square" alt="Website"></a>
  </p>

  <p>
    <a href="https://wilimits.com/project/airbrowser/">Official Site</a> •
    <a href="https://apps.microsoft.com/detail/9n1gtl34cwh9?hl=en-US&gl=SA">Get on Microsoft Store</a> •
    <a href="https://github.com/wilimits/airbrowser">GitHub</a>
  </p>
</div>

<img src="https://wilimits.com/assets/uploads/img_6ac124722114f.webp" width="100%" alt="AirBrowser Interface Preview">
---

## What it is
**Air** is a native, minimalist browser designed specifically for Windows. Built to get out of your way and let the web breathe, it uses the Microsoft Edge WebView2 engine already installed on your PC. The result? A browser that is tiny (~3.2 MB installer), lightning-fast (<0.35s cold start), and respects your privacy.

## What's new in v1.1.6
* 🏠 **Spaces (Work & Personal):** Segmented workspace switcher in the sidebar to keep professional research and personal tabs strictly separated with independent local persistence.
* 🪟 **Dual Screen Split View:** Multitask side-by-side with an interactive resizable divider. Compare documentation while writing code or watch streams while taking notes in a single window.
* 🔗 **Arc-Style Centered Omnibar:** Distraction-free, compact 360px address bar pill displaying focused domain names (e.g., `github.com`) that smoothly expands on focus and press.
* 🔖 **Modern Bookmarks:** Pinned at the top of the sidebar with sleek rounded context menus and red accent deletion actions.

## What it does
* **Fast & Native:** Built entirely in C# WPF. Opens in a blink, and uses significantly less memory (~70–90 MB idle) than traditional Chromium-based behemoths.
* **Spaces (Work / Personal):** Keep your work research and personal tabs in independent tab pools with 1-click sidebar switching.
* **Dual Screen Split View:** Side-by-side simultaneous browsing of two independent web pages with an adjustable divider.
* **Arc-Style Centered Omnibar:** Clean compact address bar showing domain names without visual clutter.
* **Smart Picture-in-Picture (Floating Video):** Leave YouTube or Netflix and the video automatically follows you! Air detects playing videos and seamlessly pops them out into a borderless, always-on-top mini window when you switch tabs.
* **Built-in Engine:** Leverages the native Windows WebView2 engine. No bloated downloads or duplicate browser binaries.
* **Smart Passwords:** Export your passwords from Chrome or Edge via CSV. Air securely imports them into an encrypted local profile and uses an intelligent, React-aware autofill engine to seamlessly log you into complex modern web apps.
* **Network-Level AdBlock:** Ads and trackers are blocked at the network layer before they even start loading. Pages load instantly without heavy scripts.
* **Minimal Sidebar & UI:** Tabs and bookmarks live on the side. Tools like Private Tabs, Bookmarks, and Web Inspector are neatly tucked away in a modern popup Settings menu.
* **Private Tabs:** True incognito mode right alongside your regular tabs. Private tabs have their own isolated session, cookies, and leave no trace in history.
* **Web Inspector:** Chromium's powerful developer tools are still right at your fingertips when you need them.

## What it doesn't do
* 🚫 **No tracking:** Air does not track your usage, clicks, or habits. 
* 🚫 **No cloud sync:** We don't send your data to our servers. Everything (bookmarks, history, passwords) is stored locally in `%LOCALAPPDATA%\AirBrowser`.
* 🚫 **No bloatware:** No news feeds, no shopping widgets, no "recommended content", and no crypto wallets.
* 🚫 **No battery drain:** By using native WPF and WebView2, it doesn't waste your laptop's battery or RAM.

## Privacy, sensibly

| Data | What Air does | Standard practice |
| ---- | ------------- | ----------------- |
| **Telemetry** | **None.** There is no analytics, crash reporting, or phone-home tracking. | Usually defaults to ON and tracks how you use the app. |
| **Passwords** | Imported and saved locally as encrypted JSON. Never leaves your PC. | Synced to a remote cloud account. |
| **History & Tabs** | Saved locally in your Windows AppData folder. | Synced to a remote cloud account. |

## For developers

### The Tech Stack
* **C# / .NET 10:** The core logic is written in modern C#.
* **WPF (Windows Presentation Foundation):** Used for creating a fast, native, and fully customizable Windows UI.
* **WebView2:** Microsoft's evergreen Edge engine handles the heavy lifting of web rendering.
* **System.Text.Json:** For lightweight, fast local state management.

### How to run locally
Ensure you have the [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed on your Windows machine.

```bash
# Clone the repository
git clone https://github.com/wilimits/airbrowser.git

# Navigate into the directory
cd airbrowser

# Build and run the project
dotnet run
```

## License
MIT License. See [LICENSE](LICENSE) for more information.
