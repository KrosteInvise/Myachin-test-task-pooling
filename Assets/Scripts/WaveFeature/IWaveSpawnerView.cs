namespace WaveFeature
{
    public interface IWaveSpawnerView
    {
        void DrawCurrentWave(int waveNumber, int totalWaves);

        void DrawRemainingEnemies(int enemiesLeft);
    }
}
