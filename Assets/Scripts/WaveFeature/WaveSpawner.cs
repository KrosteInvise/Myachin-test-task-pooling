using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;
using UnityEngine.Pool;

namespace WaveFeature
{
    public class WaveSpawner : MonoBehaviour
    {
        readonly Dictionary<EnemyType, ObjectPool<Enemy>> pools = new();
        readonly Dictionary<Enemy, ObjectPool<Enemy>> poolByEnemy = new();
        readonly Dictionary<EnemyType, int> poolCapacities = new();

        WaveConfig waveConfig;
        Transform[] spawnPoints;
        Transform targetPoint;
        IWaveSpawnerView waveSpawnerView;
        Transform poolRoot;

        WaveSpawnerModel model;
        Coroutine wavesRoutine;
        bool isInitialized;

        public bool Init(WaveConfig waveConfig, Transform[] spawnPoints, 
            Transform targetPoint, IWaveSpawnerView waveSpawnerView, Transform poolRoot)
        {
            if (isInitialized)
            {
                Debug.LogWarning("WaveSpawner is already initialized.");
                return false;
            }

            this.waveConfig = waveConfig;
            this.spawnPoints = spawnPoints;
            this.targetPoint = targetPoint;
            this.waveSpawnerView = waveSpawnerView;
            this.poolRoot = poolRoot != null ? poolRoot : transform;

            if (waveConfig == null || waveConfig.Waves == null)
            {
                Debug.LogError("WaveSpawner init failed: waveConfig is missing.");
                return false;
            }

            model = new WaveSpawnerModel(waveConfig.Waves);
            BuildPools();
            isInitialized = true;
            return true;
        }

        public bool Run()
        {
            if (isInitialized == false)
            {
                Debug.LogError("WaveSpawner must be initialized before run.");
                return false;
            }

            if (wavesRoutine != null)
            {
                Debug.LogWarning("WaveSpawner is already running.");
                return false;
            }

            if (CanRun() == false)
            {
                Debug.LogError("WaveSpawner run failed: invalid runtime dependencies.");
                return false;
            }

            wavesRoutine = StartCoroutine(RunWavesRoutine());
            return true;
        }

        bool CanRun()
        {
            return model != null && targetPoint != null && spawnPoints is { Length: > 0 } && model.TotalWaves > 0;
        }
        
        void BuildPools()
        {
            Dictionary<EnemyType, int> capacities = new();

            foreach (WaveDefinition wave in waveConfig.Waves)
            {
                if (wave == null || wave.EnemyType == null || wave.EnemyType.Prefab == null)
                    continue;

                int current = capacities.GetValueOrDefault(wave.EnemyType, 0);

                capacities[wave.EnemyType] = current + wave.EnemyCount;
            }

            foreach (KeyValuePair<EnemyType, int> pair in capacities)
            {
                ObjectPool<Enemy> pool = CreatePool(pair.Key, pair.Value);
                pools[pair.Key] = pool;
                poolCapacities[pair.Key] = pair.Value;
                PrewarmPool(pair.Key, pool, pair.Value);
            }
        }

        ObjectPool<Enemy> CreatePool(EnemyType enemyType, int capacity)
        {
            Transform container = new GameObject($"{enemyType.name}Pool").transform;
            container.SetParent(poolRoot);

            int createdCount = 0;

            ObjectPool<Enemy> pool = new(
                createFunc: () =>
                {
                    if (createdCount >= capacity)
                        return null;

                    Enemy instance = Instantiate(enemyType.Prefab, container);
                    instance.gameObject.SetActive(false);
                    createdCount++;
                    return instance;
                },
                actionOnGet: enemy =>
                {
                    if (enemy != null)
                    {
                        enemy.gameObject.SetActive(true);
                    }
                },
                actionOnRelease: enemy =>
                {
                    if (enemy != null)
                    {
                        enemy.gameObject.SetActive(false);
                        enemy.transform.SetParent(container);
                    }
                },
                actionOnDestroy: enemy =>
                {
                    if (enemy != null)
                    {
                        Destroy(enemy.gameObject);
                    }
                },
                collectionCheck: false,
                defaultCapacity: capacity,
                maxSize: capacity
            );

            return pool;
        }

        void PrewarmPool(EnemyType enemyType, ObjectPool<Enemy> pool, int capacity)
        {
            List<Enemy> preWarmed = new(capacity);

            for (int i = 0; i < capacity; i++)
            {
                Enemy enemy = pool.Get();
                if (enemy == null)
                {
                    Debug.LogError($"Prewarm failed for {enemyType.name} at index {i}.");
                    break;
                }

                preWarmed.Add(enemy);
            }

            foreach (Enemy enemy in preWarmed)
            {
                pool.Release(enemy);
            }
        }
        IEnumerator RunWavesRoutine()
        {
            while (model.TryStartNextWave(out WaveDefinition wave))
            {
                waveSpawnerView?.DrawCurrentWave(model.CurrentWaveNumber, model.TotalWaves);
                waveSpawnerView?.DrawRemainingEnemies(model.RemainingEnemiesInWave);

                for (int i = 0; i < wave.EnemyCount; i++)
                {
                    bool spawned = SpawnEnemy(wave);
                    if (spawned == false)
                    {
                        model.MarkEnemyCompleted();
                        waveSpawnerView?.DrawRemainingEnemies(model.RemainingEnemiesInWave);
                    }

                    if (wave.SpawnInterval > 0f)
                    {
                        yield return new WaitForSeconds(wave.SpawnInterval);
                    }
                }

                while (model.RemainingEnemiesInWave > 0)
                {
                    yield return null;
                }

                if (wave.DelayToNextWave > 0f)
                {
                    yield return new WaitForSeconds(wave.DelayToNextWave);
                }
            }

            wavesRoutine = null;
        }

        bool SpawnEnemy(WaveDefinition wave)
        {
            if (pools.TryGetValue(wave.EnemyType, out ObjectPool<Enemy> pool) == false)
            {
                Debug.LogError($"Pool not found for enemy type: {wave.EnemyType.name}");
                return false;
            }

            if (pool.CountInactive <= 0)
            {
                int capacity = poolCapacities[wave.EnemyType];
                Debug.LogError($"Pool exhausted for {wave.EnemyType.name}. Capacity: {capacity}");
                return false;
            }

            Enemy enemy = pool.Get();
            if (enemy == null)
            {
                Debug.LogError($"Pool get returned null for {wave.EnemyType.name}.");
                return false;
            }

            poolByEnemy[enemy] = pool;
            enemy.transform.position = GetRandomSpawnPoint().position;
            enemy.Configure(HandleEnemyReachedTarget, targetPoint, wave.EnemyType.MoveSpeed);
            return true;
        }

        void HandleEnemyReachedTarget(Enemy enemy)
        {
            if (poolByEnemy.TryGetValue(enemy, out ObjectPool<Enemy> pool) == false)
            {
                return;
            }

            pool.Release(enemy);
            poolByEnemy.Remove(enemy);
            model.MarkEnemyCompleted();
            waveSpawnerView?.DrawRemainingEnemies(model.RemainingEnemiesInWave);
        }

        Transform GetRandomSpawnPoint()
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[spawnIndex];
        }
    }
}
