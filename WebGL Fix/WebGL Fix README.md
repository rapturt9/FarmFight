# How to Build WebGL

The bad file is gitignored, so this needs to be done manually.

1. Replace **"\FarmFight\FarmFightUnity\Library\PackageCache\com.unity.multiplayer.mlapi@3e3aef6aa0\Runtime\Transports\UNET\RelayTransport.cs"** with **"\FarmFight\WebGL Fix\RelayTransport.cs"**

   NOTE: I don't know if that is your exact path. It should look *something* like "\FarmFight\FarmFightUnity\Library\PackageCache\com.unity.multiplayer.mlapi**@...**\Runtime\Transports\UNET\RelayTransport.cs" but might have a different ID after the @.

2. On Unity, go to File -> Build Settings.

   Make sure WebGL is selected. If it isn't then click "Switch Platform"

3. Click "Build and Run". 

4. Select a path to build to. I created the new folder "\FarmFight\Build\" and selected that

   You need to build and run every time you want to run the WebGL version unless you have your own webserver set up. 

