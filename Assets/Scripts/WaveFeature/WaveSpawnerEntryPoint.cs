using UnityEngine;

namespace WaveFeature
{
    public class WaveSpawnerEntryPoint : MonoBehaviour, IEntryPointInitializable, IEntryPointRunnable
    {
        [SerializeField]
        WaveSpawner waveSpawner;

        [SerializeField]
        WaveConfig waveConfig;

        [SerializeField]
        Transform[] spawnPoints;

        [SerializeField]
        Transform targetPoint;
        
        [SerializeField]
        WaveSpawnerView waveSpawnerView;

        [SerializeField]
        Transform poolRoot;

        bool isInitialized;

        public bool Init()
        {
            if (waveSpawner == null)
            {
                Debug.LogError("WaveSpawnerEntryPoint: waveSpawner is missing.");
                return false;
            }

            bool initialized = waveSpawner.Init(waveConfig, spawnPoints, targetPoint, waveSpawnerView, poolRoot);

            if (initialized == false)
                return false;

            isInitialized = true;
            return true;
        }

        public bool Run()
        {
            if (isInitialized == false)
            {
                Debug.LogError("WaveSpawnerEntryPoint: run called before init.");
                return false;
            }

            return waveSpawner.Run();
        }
    }
}
