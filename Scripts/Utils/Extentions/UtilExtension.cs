using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WildPerception
{
    public static class UtilExtension
    {
        public static void QuitWithLogError(string msg)
        {
            Debug.LogError(msg);

#if UNITY_EDITOR
            bool userSelection = EditorUtility.DisplayDialog(
                "Error",
                msg + "\n\nThe application will now exit play mode.",
                "Exit Play Mode"
            );
            
            if (userSelection)
            {
                EditorApplication.ExitPlaymode();
            }
#else
    Application.Quit();
#endif
        }

        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            dict.TryGetValue(key, out value);

            return value;
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                return obj.AddComponent<T>();
            }

            return component;
        }

        public static void SafeSetActive(UnityEngine.Object obj, bool active)
        {
            if (obj != null)
            {
                if (obj is GameObject)
                {
                    ((GameObject)obj).SetActive(active);
                }
                else if (obj is Component)
                {
                    ((GameObject)obj).gameObject.SetActive(active);
                }
            }
        }
    }
}