# Installing Blade Proclamations — short version

The supplied package is source code because this build environment does not contain the Windows/.NET 10/Dalamud compilation toolchain.

## One-time setup

1. Install **Visual Studio 2022 Community**.
2. In the installer, select **.NET desktop development**.
3. Install the **.NET 10 SDK** if Visual Studio did not include it.

## Compile

1. Extract the BladeProclamations folder somewhere permanent, for example:

   ```text
   C:\FFXIV Plugins\BladeProclamations
   ```

2. Open **PowerShell** inside that folder.
3. Run:

   ```powershell
   dotnet restore
   dotnet build -c Release
   ```

4. Find `BladeProclamations.dll` inside:

   ```text
   bin\x64\Release\net10.0-windows\
   ```

## Add it to Dalamud

1. Launch FFXIV through XIVLauncher.
2. Enter `/xlsettings` in chat.
3. Select **Experimental**.
4. Under **Dev Plugin Locations**, add the full path to `BladeProclamations.dll`.
5. Enter `/xlplugins`.
6. Go to **Dev Tools → Installed Dev Plugins**.
7. Enable **Blade Proclamations**.

## Place the overlay

1. Enter `/bp unlock`.
2. Drag and resize the visible panel.
3. Enter `/bp test` to preview it.
4. Enter `/bp lock` when satisfied.
5. Enter `/bp` any time you want to edit quotes, colors, duration, size, or action toggles.
