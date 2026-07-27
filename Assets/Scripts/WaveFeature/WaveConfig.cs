using System.Collections.Generic;
using UnityEngine;

namespace WaveFeature
{
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "Wave Spawner/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [SerializeField]
        List<WaveDefinition> waves;

        public List<WaveDefinition> Waves => waves;
    }
}