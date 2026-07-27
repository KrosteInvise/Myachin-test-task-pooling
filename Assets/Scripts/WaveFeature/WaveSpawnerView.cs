using TMPro;
using UnityEngine;

namespace WaveFeature
{
    public class WaveSpawnerView : MonoBehaviour, IWaveSpawnerView
    {
        [SerializeField]
        TextMeshProUGUI currentWaveText;

        [SerializeField]
        TextMeshProUGUI remainingEnemiesText;

        public void DrawCurrentWave(int waveNumber, int totalWaves)
        {
            currentWaveText.text = $"Wave: {waveNumber}/{totalWaves}";
        }

        public void DrawRemainingEnemies(int enemiesLeft)
        {
            remainingEnemiesText.text = $"Enemies left: {Mathf.Max(0, enemiesLeft)}";
        }
    }
}
