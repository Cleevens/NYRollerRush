// NY ROLLER RUSH - CORE SYSTEM
// Menu → Play → Pause/GameOver → Shop → Restart. Canvas UI, no OnGUI.

using NYRollerRush.Pooling;
using NYRollerRush.Runner;
using NYRollerRush.Traffic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NYRollerRush.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] bool startAutomatically;
        [SerializeField] float distanceScoreRate = 4f;
        [SerializeField] int nearMissBonus = 25;
        [SerializeField] Vector3 playerSpawn = new Vector3(0f, 0f, 2f);

        public GameState State { get; private set; } = GameState.Menu;
        public float Score { get; private set; }
        public float Distance => SkateController.Instance != null ? SkateController.Instance.Distance : 0f;
        public int LastCoinsEarned { get; private set; }
        public bool IsNewHighScore { get; private set; }

        RunnerSession session;
        bool awardedRunCoins;
        bool runBusy;
        GameState beforePause = GameState.Playing;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            session = GetComponent<RunnerSession>();
            if (session == null)
                session = gameObject.AddComponent<RunnerSession>();
            TrafficRules.PlayerZ = () => SkateController.Instance != null ? SkateController.Instance.transform.position.z : 0f;
            PickupRules.CanCollect = c => c != null && (c.CompareTag("Player") || c.CompareTag("Companion"));
            CollectibleCoin.Collected += OnCoinCollected;
            PooledChunk.Activated += OnChunkActivated;
        }

        void Start()
        {
            SaveSystem.Instance?.ApplyToManagers();
            NeighborhoodManager.Instance?.ApplyStarting();
            ShopManager.Instance?.ApplyToSkater();
            DailyReward.TryGrant();
            Time.timeScale = 1f;
            if (startAutomatically)
                StartRun();
            else if (GameUI.Instance != null)
                GameUI.Instance.ShowSplashThenMenu();
            else
                ReturnToMenu();
        }

        void Update()
        {
            HandleDebugKeys();
            if (Input.GetKeyDown(KeyCode.Escape) && (State == GameState.Playing || State == GameState.Paused))
                TogglePause();

            if (State == GameState.Playing && SkateController.Instance != null)
            {
                float mul = Mathf.Max(1f, SkateController.Instance.ScoreMul);
                AddScore(SkateController.Instance.CurrentSpeed * distanceScoreRate * mul * Time.deltaTime);
                NeighborhoodManager.Instance?.TryUnlockFromScore(Score);
            }
        }

        public void StartRun()
        {
            if (runBusy) return;
            runBusy = true;
            Time.timeScale = 1f;
            Score = 0f;
            LastCoinsEarned = 0;
            IsNewHighScore = false;
            awardedRunCoins = false;
            State = GameState.Playing;
            session?.BeginRun();
            PowerUpManager.Instance?.ClearAll();
            CompanionSkaterSpawner.Instance?.DespawnAll();
            PoolHub.Instance?.RecycleWorld();
            EndlessChunkSpawner.Instance?.ResetCourse();
            if (SkateController.Instance != null)
                SkateController.Instance.ResetRun(playerSpawn);
            ShopManager.Instance?.ApplyToSkater();
            NeighborhoodManager.Instance?.ApplyStarting();
            GameUI.Instance?.ShowHud();
            runBusy = false;
        }

        public void GameOver()
        {
            if (State != GameState.Playing && State != GameState.Paused) return;
            Time.timeScale = 1f;
            AudioManager.Instance?.SetMusicPaused(false);
            State = GameState.GameOver;
            session?.Die();
            PowerUpManager.Instance?.ClearAll();
            CompanionSkaterSpawner.Instance?.DespawnAll();
            int previousBest = SaveSystem.Instance != null ? SaveSystem.Instance.Data.highScore : 0;
            AwardRunPayout();
            IsNewHighScore = Mathf.FloorToInt(Score) > previousBest;
            NeighborhoodManager.Instance?.TryUnlockFromScore(Score);
            SaveSystem.Instance?.CaptureFromManagers();
            SaveSystem.Instance?.Save();
            AudioManager.Instance?.Play(SfxId.Crash);
            if (IsNewHighScore)
                AudioManager.Instance?.Play(SfxId.HighScore);
            GameUI.Instance?.ShowGameOver();
        }

        public void Restart()
        {
            StartRun();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.SetMusicPaused(false);
            State = GameState.Menu;
            PowerUpManager.Instance?.ClearAll();
            CompanionSkaterSpawner.Instance?.DespawnAll();
            GameUI.Instance?.ShowMenu();
        }

        public void OpenShop()
        {
            Time.timeScale = 1f;
            var from = State == GameState.Paused ? GameState.Menu : State;
            if (from == GameState.Playing)
                from = GameState.Menu;
            GameUI.Instance?.ShowShop(from);
        }

        public void CloseShop(GameState returnState)
        {
            if (returnState == GameState.GameOver)
            {
                State = GameState.GameOver;
                GameUI.Instance?.ShowGameOver();
            }
            else
                ReturnToMenu();
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                beforePause = State;
                State = GameState.Paused;
                Time.timeScale = 0f;
                AudioManager.Instance?.SetMusicPaused(true);
                GameUI.Instance?.ShowPause();
            }
            else if (State == GameState.Paused)
            {
                State = beforePause;
                Time.timeScale = 1f;
                AudioManager.Instance?.SetMusicPaused(false);
                GameUI.Instance?.HidePause();
                GameUI.Instance?.ShowHud();
            }
        }

        public void ReloadScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void AddScore(float amount)
        {
            Score += amount;
            session?.AddScore(amount);
        }

        public void AddCoinScore(int points)
        {
            AddScore(points);
            CurrencyManager.Instance?.AddCoins(Mathf.Max(1, points / 50));
        }

        public void AddNearMissScore()
        {
            float mul = SkateController.Instance != null ? Mathf.Max(1f, SkateController.Instance.ScoreMul) : 1f;
            AddScore(nearMissBonus * mul);
        }

        void AwardRunPayout()
        {
            if (awardedRunCoins) return;
            awardedRunCoins = true;
            LastCoinsEarned = Mathf.Max(10, Mathf.FloorToInt(Score / 65f));
            CurrencyManager.Instance?.AddCoins(LastCoinsEarned);
            if (Score >= 5000)
                CurrencyManager.Instance?.AddGems(1);
        }

        void OnCoinCollected(int points)
        {
            if (SkateController.Instance != null)
                SkateController.Instance.NotifyCoinCollected(points);
            else
                AddCoinScore(points);
        }

        void OnChunkActivated(PooledChunk chunk)
        {
            TrafficManager.Instance?.OnChunkActivated(chunk);
        }

        void HandleDebugKeys()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) PowerUpManager.Instance?.Activate(PowerUpKind.Magnet);
            if (Input.GetKeyDown(KeyCode.Alpha2)) PowerUpManager.Instance?.Activate(PowerUpKind.SpeedBoost);
            if (Input.GetKeyDown(KeyCode.Alpha3)) PowerUpManager.Instance?.Activate(PowerUpKind.Shield);
            if (Input.GetKeyDown(KeyCode.Alpha4)) PowerUpManager.Instance?.Activate(PowerUpKind.Strength);
            if (Input.GetKeyDown(KeyCode.Alpha5)) PowerUpManager.Instance?.Activate(PowerUpKind.ScoreMultiplier);
            if (Input.GetKeyDown(KeyCode.Alpha6)) PowerUpManager.Instance?.Activate(PowerUpKind.Ghost);
            if (Input.GetKeyDown(KeyCode.Alpha7)) PowerUpManager.Instance?.Activate(PowerUpKind.CompanionCall);
            if (Input.GetKeyDown(KeyCode.Alpha8)) PowerUpManager.Instance?.Activate(PowerUpKind.TrafficFreeze);
            if (Input.GetKeyDown(KeyCode.C)) CurrencyManager.Instance?.AddCoins(250);
            if (Input.GetKeyDown(KeyCode.N))
            {
                NeighborhoodManager.Instance?.SelectNextUnlocked();
                GameUI.Instance?.RefreshActive();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                var item = ShopManager.Instance != null ? ShopManager.Instance.FirstAffordable() : null;
                if (item != null)
                    ShopManager.Instance.TryBuy(item.id);
                GameUI.Instance?.RefreshActive();
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                NeighborhoodManager.Instance?.UnlockAllForDebug();
                GameUI.Instance?.RefreshActive();
            }
        }

        void OnDestroy()
        {
            CollectibleCoin.Collected -= OnCoinCollected;
            PooledChunk.Activated -= OnChunkActivated;
            if (Instance == this)
                Instance = null;
        }
    }
}
