// NY ROLLER RUSH - CORE SYSTEM
// Screen Space Overlay: Menu, HUD, Shop, Game Over, Pause. Built at runtime with uGUI.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NYRollerRush.Core
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        Font font;
        GameObject menu;
        GameObject hud;
        GameObject shop;
        GameObject gameOver;
        GameObject pause;
        Text hudScore;
        Text hudDist;
        Text hudWallet;
        Text hudHood;
        Text hudPowers;
        Text menuBest;
        Text menuWallet;
        Text menuHood;
        Text goScore;
        Text goCoins;
        Text goBest;
        Text shopWallet;
        Text menuDaily;
        Transform shopList;
        GameObject splash;
        float splashUntil;
        ShopCategory shopCategory = ShopCategory.Skates;
        GameState shopReturn = GameState.Menu;

        void Awake()
        {
            Instance = this;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            Build();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (splash != null && splash.activeSelf && Time.unscaledTime >= splashUntil)
                splash.SetActive(false);
            if (hud != null && hud.activeSelf)
                RefreshHud();
        }

        public void ShowSplashThenMenu()
        {
            ShowMenu();
            if (splash == null) return;
            splash.SetActive(true);
            splashUntil = Time.unscaledTime + 1.15f;
        }

        public void ShowMenu()
        {
            SetOnly(menu);
            RefreshMenu();
        }

        public void ShowHud()
        {
            SetOnly(hud);
            RefreshHud();
        }

        public void ShowGameOver()
        {
            SetOnly(gameOver);
            RefreshGameOver();
        }

        public void ShowShop(GameState returnState)
        {
            shopReturn = returnState;
            SetOnly(shop);
            RebuildShopList();
            RefreshShopWallet();
        }

        public void ShowPause()
        {
            if (pause != null)
                pause.SetActive(true);
        }

        public void HidePause()
        {
            if (pause != null)
                pause.SetActive(false);
        }

        public void RefreshActive()
        {
            if (menu != null && menu.activeSelf) RefreshMenu();
            if (hud != null && hud.activeSelf) RefreshHud();
            if (shop != null && shop.activeSelf)
            {
                RebuildShopList();
                RefreshShopWallet();
            }
            if (gameOver != null && gameOver.activeSelf) RefreshGameOver();
        }

        void SetOnly(GameObject visible)
        {
            if (menu != null) menu.SetActive(visible == menu);
            if (hud != null) hud.SetActive(visible == hud);
            if (shop != null) shop.SetActive(visible == shop);
            if (gameOver != null) gameOver.SetActive(visible == gameOver);
            HidePause();
            if (splash != null && visible != menu)
                splash.SetActive(false);
        }

        void RefreshHud()
        {
            var gm = GameManager.Instance;
            var wallet = CurrencyManager.Instance;
            if (hudScore != null && gm != null)
                hudScore.text = "SCORE  " + Mathf.FloorToInt(gm.Score);
            if (hudDist != null && gm != null)
                hudDist.text = "DIST  " + Mathf.FloorToInt(gm.Distance) + "m";
            if (hudWallet != null && wallet != null)
                hudWallet.text = "COINS  " + wallet.Coins + "    GEMS  " + wallet.Gems;
            if (hudHood != null)
            {
                var n = NeighborhoodManager.Instance != null ? NeighborhoodManager.Instance.Current : null;
                hudHood.text = n != null ? n.displayName.ToUpperInvariant() : "NYC";
            }

            if (hudPowers != null)
            {
                var lines = PowerUpManager.Instance != null ? PowerUpManager.Instance.HudLines : null;
                hudPowers.text = lines != null && lines.Count > 0 ? string.Join("\n", lines) : "";
            }
        }

        void RefreshMenu()
        {
            var save = SaveSystem.Instance;
            var wallet = CurrencyManager.Instance;
            if (menuBest != null)
                menuBest.text = "BEST  " + (save != null ? save.Data.highScore : 0);
            if (menuWallet != null && wallet != null)
                menuWallet.text = "COINS  " + wallet.Coins + "    GEMS  " + wallet.Gems;
            if (menuHood != null)
            {
                var n = NeighborhoodManager.Instance != null ? NeighborhoodManager.Instance.Current : null;
                menuHood.text = n != null ? "NOW SKATING  " + n.displayName : "NOW SKATING  Times Square";
            }
            if (menuDaily != null)
            {
                if (DailyReward.GrantedThisSession)
                    menuDaily.text = "DAILY BONUS  +" + DailyReward.LastGrant + "  (streak " + (SaveSystem.Instance != null ? SaveSystem.Instance.Data.dailyRewardStreak : 1) + ")";
                else
                    menuDaily.text = SaveSystem.Instance != null && SaveSystem.Instance.Data.dailyRewardStreak > 0
                        ? "DAILY STREAK  " + SaveSystem.Instance.Data.dailyRewardStreak
                        : "";
            }
        }

        void RefreshGameOver()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (goScore != null)
                goScore.text = "SCORE  " + Mathf.FloorToInt(gm.Score);
            if (goCoins != null)
                goCoins.text = "COINS EARNED  +" + gm.LastCoinsEarned;
            if (goBest != null)
                goBest.text = gm.IsNewHighScore ? "NEW HIGH SCORE!" : "BEST  " + (SaveSystem.Instance != null ? SaveSystem.Instance.Data.highScore : 0);
        }

        void RefreshShopWallet()
        {
            var wallet = CurrencyManager.Instance;
            if (shopWallet != null && wallet != null)
                shopWallet.text = "COINS  " + wallet.Coins + "    GEMS  " + wallet.Gems;
        }

        void RebuildShopList()
        {
            if (shopList == null || ShopManager.Instance == null) return;
            for (int i = shopList.childCount - 1; i >= 0; i--)
                Destroy(shopList.GetChild(i).gameObject);

            var items = ShopManager.Instance.ItemsIn(shopCategory);
            for (int i = 0; i < items.Count; i++)
                BuildShopRow(items[i], i);
            RefreshShopWallet();
        }

        void BuildShopRow(ShopItemData item, int index)
        {
            bool owned = ShopManager.Instance.Owns(item.id);
            bool equipped = ShopManager.Instance.IsEquipped(item.id);
            string price = item.coinCost > 0 || item.gemCost > 0
                ? (item.coinCost > 0 ? item.coinCost + "c" : "") + (item.gemCost > 0 ? "  " + item.gemCost + "g" : "")
                : "FREE";
            string status = equipped ? "EQUIPPED" : owned ? "OWNED" : price;

            var row = Panel(shopList, "Item_" + item.id, new Vector2(0f, 1f), new Vector2(1f, 1f), new Color(0.12f, 0.13f, 0.16f, 0.95f));
            var rt = row.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -10f - index * 78f);
            rt.sizeDelta = new Vector2(-16f, 70f);

            Label(row.transform, item.displayName, 18, TextAnchor.MiddleLeft, new Vector2(16, 8), new Vector2(280, 28));
            Label(row.transform, status, 16, TextAnchor.MiddleLeft, new Vector2(16, -22), new Vector2(280, 24));

            if (!owned)
            {
                var buy = Button(row.transform, "BUY", new Vector2(-20, 0), new Vector2(128, 44), new Color(0.2f, 0.55f, 0.35f));
                buy.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
                buy.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
                buy.onClick.AddListener(() =>
                {
                    ShopManager.Instance.TryBuy(item.id);
                    RebuildShopList();
                });
            }
            else if (!equipped)
            {
                var eq = Button(row.transform, "EQUIP", new Vector2(-20, 0), new Vector2(128, 44), new Color(0.25f, 0.4f, 0.7f));
                eq.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
                eq.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
                eq.onClick.AddListener(() =>
                {
                    ShopManager.Instance.Equip(item.id);
                    RebuildShopList();
                });
            }
        }

        void Build()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var canvasGo = new GameObject("GameCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var safe = Panel(canvasGo.transform, "SafeArea", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0));
            ApplySafeArea(safe.GetComponent<RectTransform>());
            var host = safe.transform;

            menu = BuildMenu(host);
            hud = BuildHud(host);
            shop = BuildShop(host);
            gameOver = BuildGameOver(host);
            pause = BuildPause(host);
            pause.SetActive(false);
            splash = BuildSplash(canvasGo.transform);
            splash.SetActive(false);
        }

        GameObject BuildMenu(Transform parent)
        {
            var root = Panel(parent, "Menu", Vector2.zero, Vector2.one, new Color(0.05f, 0.06f, 0.08f, 0.82f)).gameObject;
            Label(root.transform, "NY ROLLER RUSH", 52, TextAnchor.MiddleCenter, new Vector2(0, 220), new Vector2(900, 70));
            menuBest = Label(root.transform, "BEST  0", 26, TextAnchor.MiddleCenter, new Vector2(0, 150), new Vector2(600, 36));
            menuWallet = Label(root.transform, "COINS  0    GEMS  0", 22, TextAnchor.MiddleCenter, new Vector2(0, 110), new Vector2(600, 32));
            menuHood = Label(root.transform, "NOW SKATING  Times Square", 20, TextAnchor.MiddleCenter, new Vector2(0, 78), new Vector2(800, 30));
            menuDaily = Label(root.transform, "", 18, TextAnchor.MiddleCenter, new Vector2(0, 42), new Vector2(800, 28));

            var play = Button(root.transform, "PLAY", new Vector2(0, -30), new Vector2(340, 80), new Color(0.15f, 0.62f, 0.4f));
            play.onClick.AddListener(() => GameManager.Instance?.StartRun());
            var shopBtn = Button(root.transform, "SHOP", new Vector2(0, -122), new Vector2(340, 68), new Color(0.22f, 0.38f, 0.62f));
            shopBtn.onClick.AddListener(() => GameManager.Instance?.OpenShop());
            var nextHood = Button(root.transform, "NEXT NEIGHBORHOOD", new Vector2(0, -202), new Vector2(400, 60), new Color(0.35f, 0.28f, 0.2f));
            nextHood.onClick.AddListener(() =>
            {
                NeighborhoodManager.Instance?.SelectNextUnlocked();
                RefreshMenu();
            });
            return root;
        }

        GameObject BuildHud(Transform parent)
        {
            var root = Panel(parent, "HUD", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0)).gameObject;
            hudScore = Label(root.transform, "SCORE  0", 28, TextAnchor.UpperLeft, new Vector2(24, -20), new Vector2(480, 36));
            Pin(hudScore.rectTransform, new Vector2(0, 1), new Vector2(0, 1));
            hudDist = Label(root.transform, "DIST  0m", 22, TextAnchor.UpperLeft, new Vector2(24, -56), new Vector2(400, 30));
            Pin(hudDist.rectTransform, new Vector2(0, 1), new Vector2(0, 1));
            hudWallet = Label(root.transform, "COINS  0", 20, TextAnchor.UpperRight, new Vector2(-24, -20), new Vector2(420, 30));
            Pin(hudWallet.rectTransform, new Vector2(1, 1), new Vector2(1, 1));
            hudHood = Label(root.transform, "TIMES SQUARE", 20, TextAnchor.UpperCenter, new Vector2(0, -18), new Vector2(500, 30));
            Pin(hudHood.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hudPowers = Label(root.transform, "", 18, TextAnchor.UpperLeft, new Vector2(24, -96), new Vector2(480, 180));
            Pin(hudPowers.rectTransform, new Vector2(0, 1), new Vector2(0, 1));
            var pauseBtn = Button(root.transform, "II", new Vector2(-28, -78), new Vector2(72, 56), new Color(0.15f, 0.15f, 0.18f, 0.8f));
            Pin(pauseBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1));
            pauseBtn.onClick.AddListener(() => GameManager.Instance?.TogglePause());
            return root;
        }

        GameObject BuildGameOver(Transform parent)
        {
            var root = Panel(parent, "GameOver", Vector2.zero, Vector2.one, new Color(0.04f, 0.04f, 0.06f, 0.88f)).gameObject;
            Label(root.transform, "GAME OVER", 48, TextAnchor.MiddleCenter, new Vector2(0, 220), new Vector2(700, 60));
            goBest = Label(root.transform, "BEST  0", 28, TextAnchor.MiddleCenter, new Vector2(0, 150), new Vector2(700, 36));
            goScore = Label(root.transform, "SCORE  0", 26, TextAnchor.MiddleCenter, new Vector2(0, 110), new Vector2(700, 32));
            goCoins = Label(root.transform, "COINS EARNED  +0", 22, TextAnchor.MiddleCenter, new Vector2(0, 70), new Vector2(700, 30));

            var restart = Button(root.transform, "RESTART", new Vector2(0, -20), new Vector2(340, 72), new Color(0.15f, 0.62f, 0.4f));
            restart.onClick.AddListener(() => GameManager.Instance?.Restart());
            var shopBtn = Button(root.transform, "SHOP", new Vector2(0, -104), new Vector2(340, 64), new Color(0.22f, 0.38f, 0.62f));
            shopBtn.onClick.AddListener(() => GameManager.Instance?.OpenShop());
            var menuBtn = Button(root.transform, "MAIN MENU", new Vector2(0, -180), new Vector2(340, 64), new Color(0.3f, 0.3f, 0.34f));
            menuBtn.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
            return root;
        }

        GameObject BuildShop(Transform parent)
        {
            var root = Panel(parent, "Shop", Vector2.zero, Vector2.one, new Color(0.06f, 0.07f, 0.09f, 0.94f)).gameObject;
            var title = Label(root.transform, "SHOP", 40, TextAnchor.UpperCenter, new Vector2(0, -24), new Vector2(400, 50));
            Pin(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            shopWallet = Label(root.transform, "COINS  0", 20, TextAnchor.UpperRight, new Vector2(-24, -28), new Vector2(420, 28));
            Pin(shopWallet.rectTransform, new Vector2(1, 1), new Vector2(1, 1));

            float x = -420f;
            AddCat(root.transform, "SKATES", ShopCategory.Skates, ref x);
            AddCat(root.transform, "PROTECTION", ShopCategory.Helmet, ref x);
            AddCat(root.transform, "CLOTHING", ShopCategory.Clothing, ref x);
            AddCat(root.transform, "AVATARS", ShopCategory.Avatar, ref x);

            var listHost = Panel(root.transform, "List", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.72f), new Color(0.08f, 0.09f, 0.11f, 0.9f));
            shopList = listHost.transform;

            var back = Button(root.transform, "BACK", new Vector2(28, -32), new Vector2(160, 56), new Color(0.28f, 0.28f, 0.3f));
            Pin(back.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1));
            back.onClick.AddListener(() => GameManager.Instance?.CloseShop(shopReturn));
            return root;
        }

        void AddCat(Transform parent, string label, ShopCategory cat, ref float x)
        {
            var btn = Button(parent, label, new Vector2(x, 200), new Vector2(200, 52), new Color(0.18f, 0.2f, 0.24f));
            x += 220f;
            ShopCategory captured = cat;
            btn.onClick.AddListener(() =>
            {
                shopCategory = captured;
                RebuildShopList();
            });
        }

        GameObject BuildPause(Transform parent)
        {
            var root = Panel(parent, "Pause", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.55f)).gameObject;
            Label(root.transform, "PAUSED", 44, TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(400, 50));
            var resume = Button(root.transform, "RESUME", new Vector2(0, 0), new Vector2(300, 72), new Color(0.15f, 0.62f, 0.4f));
            resume.onClick.AddListener(() => GameManager.Instance?.TogglePause());
            var menuBtn = Button(root.transform, "MAIN MENU", new Vector2(0, -88), new Vector2(300, 64), new Color(0.3f, 0.3f, 0.34f));
            menuBtn.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
            return root;
        }

        GameObject BuildSplash(Transform parent)
        {
            var root = Panel(parent, "Splash", Vector2.zero, Vector2.one, new Color(0.04f, 0.05f, 0.07f, 0.96f)).gameObject;
            Label(root.transform, "NY ROLLER RUSH", 56, TextAnchor.MiddleCenter, new Vector2(0, 20), new Vector2(1000, 80));
            Label(root.transform, "ROLL THE CITY", 22, TextAnchor.MiddleCenter, new Vector2(0, -40), new Vector2(600, 36));
            return root;
        }

        static void ApplySafeArea(RectTransform rt)
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Rect sa = Screen.safeArea;
            Vector2 min = sa.position;
            Vector2 max = sa.position + sa.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            rt.anchorMin = min;
            rt.anchorMax = max;
        }

        static void Pin(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
        }

        GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        Text Label(Transform parent, string text, int size, TextAnchor align, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(text.Length > 18 ? "Label" : text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        Button Button(Transform parent, string label, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            var txt = Label(go.transform, label, 20, TextAnchor.MiddleCenter, Vector2.zero, size);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => AudioManager.Instance?.Play(SfxId.Button));
            return button;
        }
    }
}
