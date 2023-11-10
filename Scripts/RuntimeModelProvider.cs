using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if false || ENABLE_CV_SyntheticHumans
namespace WildPerception
{
    using Unity.CV.SyntheticHumans;
    using Unity.CV.SyntheticHumans.Generators;
    using Unity.CV.SyntheticHumans.Randomizers;

    public class RuntimeModelProvider : AbstractPedestrianModelProvider
    {
        public HumanGenerationConfigParameter humanGenerationConfigs = new HumanGenerationConfigParameter();
        public Dictionary<HumanGenerationConfig, HumanGenerationConfig> m_RunTimeCopiesOfConfigs = new();

        public void Awake()
        {
            foreach (var config in humanGenerationConfigs.categories.Select(cat => cat.Item1))
            {
                if (m_RunTimeCopiesOfConfigs.ContainsKey(config))
                {
                    Debug.LogError(
                        $"Duplicate {nameof(HumanGenerationConfig)} assets have been added to the {nameof(HumanGenerationRandomizer)}. This will cause an incorrect distribution of configs. Please make sure each added config is a unique asset.");
                }
                else
                {
                    m_RunTimeCopiesOfConfigs.Add(config, UnityEngine.Object.Instantiate(config));
                }
            }

            foreach (var config in m_RunTimeCopiesOfConfigs.Values)
            {
                Type configType = config.GetType();
                // Get the MethodInfo for the Init method using reflection
                MethodInfo initMethod = configType.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);

                if (initMethod != null)
                {
                    // Invoke the Init method on the config instance
                    initMethod.Invoke(config, null);
                }
                else
                {
                    Debug.LogError("Init method not found in the HumanGenerationConfig class.");
                }
            }
        }

        public override GameObject GetPedestrianModel()
        {
            var configToUse = humanGenerationConfigs.Sample();
            configToUse = m_RunTimeCopiesOfConfigs[configToUse];

            var human = HumanGenerator.GenerateHuman(configToUse);

            return human;
        }
    }
}
#endif