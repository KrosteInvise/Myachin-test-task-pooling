using System.Collections.Generic;

namespace WaveFeature
{
    public class WaveSpawnerModel
    {
        readonly List<WaveDefinition> waves;

        public int CurrentWaveNumber { get; private set; }
        public int RemainingEnemiesInWave { get; private set; }
        public int TotalWaves => waves.Count;

        public WaveSpawnerModel(List<WaveDefinition> waves)
        {
            this.waves = waves ?? new List<WaveDefinition>();
        }

        public bool TryStartNextWave(out WaveDefinition wave)
        {
            int nextWaveIndex = CurrentWaveNumber;
            if (nextWaveIndex >= waves.Count)
            {
                wave = null;
                return false;
            }

            wave = waves[nextWaveIndex];
            CurrentWaveNumber++;
            RemainingEnemiesInWave = wave?.EnemyCount ?? 0;
            return true;
        }

        public void MarkEnemyCompleted()
        {
            if (RemainingEnemiesInWave > 0)
            {
                RemainingEnemiesInWave--;
            }
        }
    }
}