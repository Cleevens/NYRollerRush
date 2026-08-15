NY ROLLER RUSH — production notes
Unity 2022.3 LTS + URP. Open Assets/Scenes/TestRun.unity and Press Play.

HOW THE PROJECT IS ORGANISED
  Assets/_Core/Scripts/          gameplay, shop, save, UI, audio (NYRollerRush.Core)
  Assets/_Core/Scripts/UI/       runtime Canvas UI (GameUI)
  Assets/_Core/ScriptableObjects authored Neighborhood + Shop Item .asset files
  Assets/_Core/Resources/        runtime-loadable copies + Audio clip drop folders
  Assets/_ThirdParty/            pooling, runner chunks, traffic (do not reference Core)
  Assets/Scenes/TestRun.unity    Camera, Light, Systems (TestRunBootstrap builds the rest)

CORE LOOP
  Menu → Play → HUD → Pause/Game Over → Shop / Restart / Main Menu
  GameManager owns state. SaveSystem writes PlayerPrefs key nyrr.save.

REPLACING PRIMITIVES WITH REAL ART
  TestRunBootstrap builds placeholder capsules/cubes at Play if nothing is assigned.
  To swap art:
    1. Make prefabs for Player, StreetChunk, Car, Coin, Pedestrian, PowerUp, Companion.
    2. Assign them on PoolHub (or disable TestRunBootstrap.buildIfMissing and place them in the scene).
    3. Keep the same tags: Player, Car, Pedestrian, Coin, PowerUp, Companion.
    4. Keep CharacterController on the player and SkateController on the same object.
    5. Chunk prefabs need Begin/End markers and optional car/ped/coin spawn points (PooledChunk.Configure).
  Materials/meshes can replace the primitive children without changing scripts.

AUDIO
  AudioManager looks at inspector clips first, then Resources:
    Resources/Audio/SFX/jump, land, lane_change, coin, powerup, near_miss, crash, button, high_score
    Resources/Audio/Music/times_square, midtown, central_park, brooklyn_bridge, soho_chinatown
  Drop .wav/.ogg with those names. Empty slots are silent; the hooks stay live.

CATALOGS
  Neighborhoods / shop items: Resources/Neighborhoods and Resources/ShopItems are loaded first.
  If those folders are empty, CatalogFactory builds the same data at runtime.
  Authoritative copies also live under Assets/_Core/ScriptableObjects/.
  After you author a real Systems object in a scene, you can assign the arrays in the inspector and skip Resources.

BALANCE (defaults)
  Skate: base 9.4, max 17.5, accel 1.15, 55s to max.
  Neighborhoods: Times Square 0, Midtown 2500, Central Park 5000, Brooklyn Bridge 8000, SoHo/Chinatown 12000.
  Run coins: max(10, score/65). Daily first-of-day bonus: 80 + 15 per streak (UTC).
  Equipped skates/armor apply at the start of every run via ShopManager.ApplyToSkater.

DEBUG KEYS (optional)
  1–8 power-ups, C +250 coins, N next unlocked neighborhood, B buy first affordable, U unlock all hoods, Esc pause.

MOBILE
  GameUI uses a SafeArea root (Screen.safeArea). Primary buttons are 64–80px tall at 1920x1080 reference.
  Swipe input is already on the player (CompositeRunnerInput).

POOLING
  All dynamic world objects go through PoolHub: chunks, cars, coins, pedestrians, power-ups, companions.
