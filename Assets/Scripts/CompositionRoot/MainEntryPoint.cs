using UnityEngine;

public class MainEntryPoint : MonoBehaviour
{
    [SerializeField]
    MonoBehaviour[] startupSequence;

    void Start()
    {
        if (startupSequence == null || startupSequence.Length == 0)
        {
            Debug.LogWarning("MainEntryPoint: startup sequence is empty.");
            return;
        }

        foreach (MonoBehaviour entryPoint in startupSequence)
        {
            if (entryPoint == null)
            {
                Debug.LogError("MainEntryPoint: startup sequence contains null entry.");
                return;
            }

            if (entryPoint is not IEntryPointInitializable initializable)
            {
                Debug.LogError($"MainEntryPoint: {entryPoint.name} does not implement IEntryPointInitializable.");
                return;
            }

            bool initialized = initializable.Init();
            if (initialized == false)
            {
                Debug.LogError($"MainEntryPoint: init failed for {entryPoint.name}.");
                return;
            }
        }

        foreach (MonoBehaviour entryPoint in startupSequence)
        {
            if (entryPoint == null)
            {
                Debug.LogError("MainEntryPoint: startup sequence contains null entry.");
                return;
            }

            if (entryPoint is not IEntryPointRunnable runnable)
                continue;

            bool started = runnable.Run();
            if (started == false)
            {
                Debug.LogError($"MainEntryPoint: run failed for {entryPoint.name}.");
                return;
            }
        }
    }
}
