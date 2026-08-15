// NY ROLLER RUSH - CORE SYSTEM
// Source: InfiniteRunner3D StuffSpawner — coins/obstacles rented from PoolHub.

using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Runner
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] bool spawnOnEnable;
        [SerializeField] bool randomX;
        [SerializeField] float minX = -2f;
        [SerializeField] float maxX = 2f;
        [SerializeField] float obstacleChance = 0.5f;
        [SerializeField] float coinChance = 0.33f;

        public void Configure(Transform[] points)
        {
            spawnPoints = points;
        }

        void OnEnable()
        {
            if (!spawnOnEnable || spawnPoints == null || spawnPoints.Length == 0 || PoolHub.Instance == null)
                return;

            int obstacleIndex = -1;
            if (Random.value < obstacleChance && spawnPoints.Length > 1)
                obstacleIndex = Random.Range(1, spawnPoints.Length);

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Vector3 pos = spawnPoints[i].position;
                if (randomX)
                    pos.x += Random.Range(minX, maxX);

                if (i == obstacleIndex)
                    continue;

                if (Random.value < coinChance)
                    PoolHub.Instance.Rent(PooledKind.Coin, pos, Quaternion.identity);
            }
        }
    }
}
