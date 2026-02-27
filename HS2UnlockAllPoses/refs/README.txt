To build the plugin, either:

1. Build with game path: dotnet build -p:HS2Managed=D:\hs2\HS2_Data\Managed
   (Replace D:\hs2 with your HS2 game install folder.)

2. Or copy these 3 DLLs from your game's HS2_Data\Managed folder into this refs folder:
   - Assembly-CSharp.dll
   - UnityEngine.dll
   - UnityEngine.CoreModule.dll
   Then run: dotnet build
