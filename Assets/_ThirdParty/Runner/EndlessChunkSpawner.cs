// NY ROLLER RUSH - CORE SYSTEM
// Source: AwesomeRunner ChunkSpawner + InfiniteRunner3D PathSpawnCollider — pooled endless streets.

using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Runner
{
    public class EndlessChunkSpawner : MonoBehaviour
    {
        [SerializeField] int initialChunks = 6;
        [SerializeField] Vector3 firstChunkPosition = Vector3.zero;
        [SerializeField] bool spawnOnStart = true;

        PooledChunk lastChunk;
        public static EndlessChunkSpawner Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Start()
        {
            if (spawnOnStart)
                SpawnInitial();
        }

        public void ResetCourse()
        {
            if (PoolHub.Instance != null)
                PoolHub.Instance.RecycleLive(PooledKind.Chunk);
            lastChunk = null;
            SpawnInitial();
        }

        public void SpawnInitial()
        {
            if (PoolHub.Instance == null) return;

            lastChunk = null;
            for (int i = 0; i < initialChunks; i++)
                SpawnNext();
        }

        public void RecycleAndSpawn(PooledChunk chunk)
        {
            if (chunk != null)
                PoolHub.Instance.Return(PooledKind.Chunk, chunk.gameObject);
            SpawnNext();
        }

        public PooledChunk SpawnNext()
        {
            Vector3 pos = lastChunk == null || lastChunk.End == null
                ? firstChunkPosition
                : lastChunk.End.position;
            var go = PoolHub.Instance.Rent(PooledKind.Chunk, pos, Quaternion.identity);
            if (go == null) return null;

            var chunk = go.GetComponent<PooledChunk>();
            if (chunk == null) return null;

            chunk.Bind(this);
            if (lastChunk != null)
                chunk.PlaceAfter(lastChunk);
            lastChunk = chunk;
            return chunk;
        }
    }
}
