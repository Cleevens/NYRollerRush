// NY ROLLER RUSH - CORE SYSTEM
// Source: AwesomeRunner Chunk + InfiniteRunner3D PathSpawnCollider — pooled street chunk.

using System;
using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Runner
{
    public class PooledChunk : PoolingObject
    {
        [SerializeField] Transform begin;
        [SerializeField] Transform end;
        [SerializeField] Transform[] carSpawns;
        [SerializeField] Transform[] pedSpawns;
        [SerializeField] Transform[] coinSpawns;

        EndlessChunkSpawner spawner;

        public Transform Begin => begin;
        public Transform End => end;
        public Transform[] CoinSpawns => coinSpawns;
        public static event Action<PooledChunk> Activated;

        public void Bind(EndlessChunkSpawner owner)
        {
            spawner = owner;
        }

        public void Configure(Transform beginPoint, Transform endPoint, Transform[] cars, Transform[] peds, Transform[] coins)
        {
            begin = beginPoint;
            end = endPoint;
            carSpawns = cars;
            pedSpawns = peds;
            coinSpawns = coins;
        }

        public void PlaceAfter(PooledChunk previous)
        {
            if (previous == null || previous.end == null || begin == null) return;
            transform.position = previous.end.position - begin.localPosition;
            transform.rotation = previous.transform.rotation;
        }

        public Transform RandomCarSpawn()
        {
            return Pick(carSpawns);
        }

        public Transform RandomPedSpawn()
        {
            return Pick(pedSpawns);
        }

        public override void OnRent()
        {
            Activated?.Invoke(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") || spawner == null) return;
            spawner.RecycleAndSpawn(this);
        }

        public override void OnReturn()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        static Transform Pick(Transform[] list)
        {
            if (list == null || list.Length == 0) return null;
            return list[UnityEngine.Random.Range(0, list.Length)];
        }
    }
}
