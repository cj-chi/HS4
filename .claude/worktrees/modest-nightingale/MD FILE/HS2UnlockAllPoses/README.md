# HS2 Unlock All Poses

BepInEx 5 plugin for Honey Select 2: relaxes pose visibility checks (state, achievement, pain, faintness) so more poses are selectable at any time. Does **not** relax number-of-characters, place, event, or DLC checks (to avoid crashes).

## Do everything (one command)

From the `HS2UnlockAllPoses` folder, with your game installed (e.g. at `D:\hs2`):

```powershell
.\DoEverything.ps1 -GamePath "D:\hs2"
```

This will: **build** (using game's Managed DLLs), **back up** any existing `HS2UnlockAllPoses.dll` to `BepInEx\plugins\Backup\` with a timestamp (no overwrite), then **deploy** the new DLL. Replace `D:\hs2` with your HS2 install path. (Some repacks use `HoneySelect2_Data` instead of `HS2_Data`; the deploy script uses `HS2_Data` by default—if build fails, use `.\deploy.ps1 -GamePath "D:\hs2" -HS2Managed "D:\hs2\HoneySelect2_Data\Managed"`.)

## Build

- Set your game path when building:
  ```bash
  dotnet build -p:HS2Managed=D:\hs2\HS2_Data\Managed
  ```
  Replace `D:\hs2` with your HS2 install directory.

- Or copy `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll` from your game's `HS2_Data\Managed` into the `refs` folder, then run `dotnet build`.

## Install

1. Ensure BepInEx 5 is installed in your HS2 game folder.
2. Before copying the plugin: if `BepInEx\plugins\HS2UnlockAllPoses.dll` already exists, back it up with a timestamp (e.g. `HS2UnlockAllPoses.dll.backup_20250226_143052`) in a `Backup` subfolder—never overwrite existing backups.
3. Copy `HS2UnlockAllPoses.dll` (and any dependency DLLs from the build output) to `[GameFolder]\BepInEx\plugins\`.

## Rollback

Remove or rename `HS2UnlockAllPoses.dll` from `BepInEx\plugins`, or restore from a backup if you had one.
