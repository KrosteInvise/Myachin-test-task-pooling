using System;
using UnityEngine;

namespace WaveFeature
{
    [Serializable]
    public class WaveDefinition
    {
        [SerializeField]
        EnemyType enemyType;

        [SerializeField, Min(1)] 
        int enemyCount = 5;

        [SerializeField, Min(0f)] 
        float spawnInterval = 0.5f;

        [SerializeField, Min(0f)] 
        float delayToNextWave = 1f;

        public EnemyType EnemyType => enemyType;

        public int EnemyCount => enemyCount;

        public float SpawnInterval => spawnInterval;

        public float DelayToNextWave => delayToNextWave;
    }
}