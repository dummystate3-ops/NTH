# NovaToolsHub

ASP.NET Core app with a collection of tools (plus Tailwind CSS).

## Setup (local)

1. Create your local settings file:
   - Copy `appsettings.example.json` to `appsettings.json`
   - Fill in your API keys / admin password as needed

2. Install frontend deps (for Tailwind build):
   - `npm install`

3. Build CSS:
   - `npm run build:css`

4. Run the app:
   - `dotnet run`

## Notes

- `appsettings.json` is intentionally ignored by git (it may contain secrets).
- Set `AdminCredentials:Password` in your `appsettings.json` (there is no hard-coded default).
- The ONNX background-removal models are not committed (they exceed GitHub's 100MB limit). Place the required `.onnx` files in `App_Data/models/`.
