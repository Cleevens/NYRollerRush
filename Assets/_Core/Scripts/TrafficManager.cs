// NY ROLLER RUSH - CORE SYSTEM
// Spawns pooled cars / pedestrians and keeps them on waypoint + light rules.

using NYRollerRush.Pooling;
using NYRollerRush.Runner;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager Instance { get; private set; }

        [Header("Neighborhood tweaks")]
        public float carDensity = 0.65f;
        public float pedestrianDensity = 0.35f;
        public float carSpeed = 8.5f;
        public float pedestrianSpeed = 1.5f;
        public float carLightLookAhead = 10f;

        [Header("Spawn")]
        [SerializeField] float minCarGap = 0.55f;
        [SerializeField] float minPedGap = 0.9f;

        float carTimer;
        float pedTimer;
        Transform player;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
                return;
            if (PoolHub.Instance == null)
                return;

            if (player == null && SkateController.Instance != null)
                player = SkateController.Instance.transform;

            carTimer += Time.deltaTime * Mathf.Max(0.05f, carDensity);
            pedTimer += Time.deltaTime * Mathf.Max(0.05f, pedestrianDensity);

            if (carTimer >= minCarGap)
            {
                carTimer = 0f;
                SpawnCarAhead();
            }

            if (pedTimer >= minPedGap)
            {
                pedTimer = 0f;
                SpawnPedestrianAhead();
            }
        }

        public void ApplyNeighborhood(NeighborhoodData data)
        {
            if (data == null) return;
            carDensity = data.carDensity;
            pedestrianDensity = data.pedestrianDensity;
            carSpeed = data.carSpeed * data.speedModifier;
        }

        public void OnChunkActivated(PooledChunk chunk)
        {
            if (chunk == null) return;
            SpawnCoinsOnChunk(chunk);
            if (Random.value < 0.32f)
                SpawnPowerUpOnChunk(chunk);
            if (Random.value < 0.85f)
                SpawnCarOnChunk(chunk);
            if (Random.value < 0.55f)
                SpawnPedestrianOnChunk(chunk);
        }

        void SpawnPowerUpOnChunk(PooledChunk chunk)
        {
            var points = chunk.CoinSpawns;
            Vector3 pos = points != null && points.Length > 0 && points[0] != null
                ? points[Random.Range(0, points.Length)].position + Vector3.up * 0.2f
                : chunk.transform.position + new Vector3(LaneX(Random.Range(0, 3)), 0.9f, 14f);
            PowerUpManager.Instance?.Spawn(pos);
        }

        void SpawnCarAhead()
        {
            if (player == null || TrafficRules.Frozen) return;
            SpawnCarAt(new Vector3(LaneX(Random.Range(0, 3)), 0.55f, player.position.z + Random.Range(28f, 46f)));
        }

        void SpawnPedestrianAhead()
        {
            if (player == null || TrafficRules.Frozen) return;
            float side = Random.value < 0.5f ? -5.2f : 5.2f;
            SpawnPedestrianAt(new Vector3(side, 0f, player.position.z + Random.Range(18f, 36f)));
        }

        void SpawnCarOnChunk(PooledChunk chunk)
        {
            var point = chunk.RandomCarSpawn();
            SpawnCarAt(point != null ? point.position : chunk.transform.position + new Vector3(LaneX(Random.Range(0, 3)), 0.55f, 18f));
        }

        void SpawnPedestrianOnChunk(PooledChunk chunk)
        {
            var point = chunk.RandomPedSpawn();
            SpawnPedestrianAt(point != null ? point.position : chunk.transform.position + new Vector3(Random.value < 0.5f ? -5.2f : 5.2f, 0f, 12f));
        }

        void SpawnCoinsOnChunk(PooledChunk chunk)
        {
            var points = chunk.CoinSpawns;
            if (points == null) return;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null || Random.value > 0.55f) continue;
                PoolHub.Instance.Rent(PooledKind.Coin, points[i].position, Quaternion.identity);
            }
        }

        void SpawnCarAt(Vector3 position)
        {
            var go = PoolHub.Instance.Rent(PooledKind.Car, position, Quaternion.identity);
            if (go == null) return;
            var ai = go.GetComponent<KinematicVehicleAI>();
            if (ai == null) return;
            ai.SetSpeed(carSpeed * Random.Range(0.75f, 1.15f));
            ai.SetLightLookAhead(carLightLookAhead);
            var net = TrafficNetwork.Instance;
            if (net != null)
                ai.BindPath(net.FindPathNear(position));
        }

        void SpawnPedestrianAt(Vector3 position)
        {
            var go = PoolHub.Instance.Rent(PooledKind.Pedestrian, position, Quaternion.identity);
            if (go == null) return;
            go.GetComponent<PooledPedestrian>()?.SetWalk(Vector3.forward, pedestrianSpeed);
        }

        static float LaneX(int index) => (index - 1) * 2f;
    }
}
