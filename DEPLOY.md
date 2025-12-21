
# How to Update NovaToolsHub

Since your project is already live, you usually only need to update the **Code** (HTML, CSS, C#), which is small and fast. You rarely need to update the huge AI Models (`App_Data`).

## 1. Create the Update Package
Run the helper script I created for you in PowerShell:

### For Regular Code Updates (Recommended)
This creates a small zip file (~60-80MB) and **skips** the heavy models.
```powershell
.\build_release.ps1
```

### For Full Updates (New Models Added)
Only run this if you added new AI models to `App_Data`. This creates a huge zip (~400MB+).
```powershell
.\build_release.ps1 -Full
```

## 2. Upload to Server
1.  Open **FileZilla** (or your Control Panel File Manager).
2.  Connect to your server.
3.  Go to the `/nova` directory.
4.  Upload the **`deploy.zip`** file you just created.

## 3. Apply Changes
1.  Log in to your **Hosting Control Panel** (Web File Manager).
2.  Go to `/nova`.
3.  Right-click `deploy.zip` and select **Unzip / Extract**.
4.  Select **Overwrite All** if asked.
5.  (Optional) Restart your website from the control panel to make sure changes take effect immediately.

Done! 🚀
