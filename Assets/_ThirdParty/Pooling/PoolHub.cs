// NY ROLLER RUSH - CORE SYSTEM
// Single rent/return entry for chunks, cars, coins, pedestrians, and power-ups.

using System;
using UnityEngine;

namespace NYRollerRush.Pooling
{
    public enum PooledKind
    {
        Chunk,
        Car,
        Coin,
        Pedestrian,
        PowerUp,
        Companion
    }

    public class PoolHub : MonoBehaviour
    {
        public static PoolHub Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] GameObject chunkPrefab;
        [SerializeField] GameObject carPrefab;
        [SerializeField] GameObject coinPrefab;
        [SerializeField] GameObject pedestrianPrefab;
        [SerializeField] GameObject powerUpPrefab;
        [SerializeField] GameObject companionPrefab;

        [Header("Prewarm counts")]
        [SerializeField] int chunkPrewarm = 8;
        [SerializeField] int carPrewarm = 16;
        [SerializeField] int coinPrewarm = 32;
        [SerializeField] int pedestrianPrewarm = 16;
        [SerializeField] int powerUpPrewarm = 10;
        [SerializeField] int companionPrewarm = 4;

        GameObjectPool chunks;
        GameObjectPool cars;
        GameObjectPool coins;
        GameObjectPool pedestrians;
        GameObjectPool powerUps;
        GameObjectPool companions;
        bool built;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (!built)
                BuildPools();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            DisposeAll();
        }

        public void AssignPrefabs(GameObject chunk, GameObject car, GameObject coin, GameObject pedestrian, GameObject powerUp = null, GameObject companion = null)
        {
            chunkPrefab = chunk;
            carPrefab = car;
            coinPrefab = coin;
            pedestrianPrefab = pedestrian;
            powerUpPrefab = powerUp;
            companionPrefab = companion;
            BuildPools();
        }

        public void BuildPools()
        {
            DisposeAll();
            chunks = Create(chunkPrefab, chunkPrewarm, "Chunks");
            cars = Create(carPrefab, carPrewarm, "Cars");
            coins = Create(coinPrefab, coinPrewarm, "Coins");
            pedestrians = Create(pedestrianPrefab, pedestrianPrewarm, "Pedestrians");
            powerUps = Create(powerUpPrefab, powerUpPrewarm, "PowerUps");
            companions = Create(companionPrefab, companionPrewarm, "Companions");
            built = true;
        }

        public void PrewarmAll()
        {
            Prewarm(PooledKind.Chunk, chunkPrewarm);
            Prewarm(PooledKind.Car, carPrewarm);
            Prewarm(PooledKind.Coin, coinPrewarm);
            Prewarm(PooledKind.Pedestrian, pedestrianPrewarm);
            Prewarm(PooledKind.PowerUp, powerUpPrewarm);
            Prewarm(PooledKind.Companion, companionPrewarm);
        }

        public void Prewarm(PooledKind kind, int count)
        {
            GetPool(kind)?.Prewarm(Mathf.Max(0, count));
        }

        public GameObject Rent(PooledKind kind, Vector3 position, Quaternion rotation)
        {
            var pool = GetPool(kind);
            if (pool == null) return null;

            var go = pool.Rent(position, rotation, transform);
            var pooled = go.GetComponent<PoolingObject>();
            if (pooled != null)
                pooled.OwningPool = pool;
            return go;
        }

        public T Rent<T>(PooledKind kind, Vector3 position, Quaternion rotation) where T : Component
        {
            var go = Rent(kind, position, rotation);
            return go == null ? null : go.GetComponent<T>();
        }

        public void Return(PooledKind kind, GameObject instance)
        {
            if (instance == null) return;
            var pool = GetPool(kind);
            if (pool != null)
                pool.Return(instance);
            else
                instance.SetActive(false);
        }

        public void ReturnAll(PooledKind kind)
        {
            GetPool(kind)?.Clear();
            GetPool(kind)?.Prewarm(CountFor(kind));
        }

        public void RecycleLive(PooledKind kind)
        {
            GetPool(kind)?.ReturnRented();
        }

        public void RecycleWorld()
        {
            RecycleLive(PooledKind.Car);
            RecycleLive(PooledKind.Coin);
            RecycleLive(PooledKind.Pedestrian);
            RecycleLive(PooledKind.PowerUp);
            RecycleLive(PooledKind.Companion);
            RecycleLive(PooledKind.Chunk);
        }

        public bool HasPool(PooledKind kind) => GetPool(kind) != null;

        GameObjectPool Create(GameObject prefab, int prewarm, string folder)
        {
            if (prefab == null) return null;
            var parent = transform.Find(folder + "Pool");
            if (parent == null)
            {
                parent = new GameObject(folder + "Pool").transform;
                parent.SetParent(transform, false);
            }

            var pool = new GameObjectPool(prefab);
            if (prewarm > 0)
                pool.Prewarm(prewarm);
            return pool;
        }

        GameObjectPool GetPool(PooledKind kind)
        {
            switch (kind)
            {
                case PooledKind.Chunk: return chunks;
                case PooledKind.Car: return cars;
                case PooledKind.Coin: return coins;
                case PooledKind.Pedestrian: return pedestrians;
                case PooledKind.PowerUp: return powerUps;
                case PooledKind.Companion: return companions;
                default: return null;
            }
        }

        int CountFor(PooledKind kind)
        {
            switch (kind)
            {
                case PooledKind.Chunk: return chunkPrewarm;
                case PooledKind.Car: return carPrewarm;
                case PooledKind.Coin: return coinPrewarm;
                case PooledKind.Pedestrian: return pedestrianPrewarm;
                case PooledKind.PowerUp: return powerUpPrewarm;
                case PooledKind.Companion: return companionPrewarm;
                default: return 0;
            }
        }

        void DisposeAll()
        {
            try { chunks?.Dispose(); } catch (Exception) { }
            try { cars?.Dispose(); } catch (Exception) { }
            try { coins?.Dispose(); } catch (Exception) { }
            try { pedestrians?.Dispose(); } catch (Exception) { }
            try { powerUps?.Dispose(); } catch (Exception) { }
            try { companions?.Dispose(); } catch (Exception) { }
            chunks = cars = coins = pedestrians = powerUps = companions = null;
            built = false;
        }
    }
}
