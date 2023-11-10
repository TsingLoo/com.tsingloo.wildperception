using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WildPerception
{
    public class LocalFilePedestrianModelProvider : AbstractPedestrianModelProvider
    {
        List<string> usedHumanModels = new List<string>(); // List to keep track of used human model names

        [SerializeField] bool RandomPickModels = true;
        int modelFileIndex = 0;

        /// <summary>
        /// The selected folder path must exactly contains "Resources/Model", for example: D:/UnityProjects/com.tsingloo.wildperception/Resources/Models
        /// </summary>
        public string model_PATH = @"..\com.tsingloo.wildperception\Resources\Models";

        private void Awake()
        {
            // Check if the selected path includes "Resources/Models"
            if (!LocalFilePedestrianModelProviderExtention.CheckModelPath(model_PATH)) return;
            // Get all files in the selected directory
            string[] files = Directory.GetFiles(model_PATH);

            // Check if the directory contains only .prefab files and their .meta files
            if (files.Any(
                    file => Path.GetExtension(file) != ".prefab" && Path.GetExtension(file) != ".meta"))
            {
                UtilExtension.QuitWithLogError(
                    $"[{nameof(LocalFilePedestrianModelProvider)}] The selected folder must contain only .prefab files and their respective .meta files.");
                return;
            }
        }

        public override GameObject GetPedestrianModel()
        {
            string humanModelPath = "";
            bool isDuplicateModel = true;
            //Debug.Log($"[{nameof(PeopleManager)}]Assets/Bundles/Resources/{PATH}");
            //Debug.Log(model_PATH);
            string[] humanModelFiles = Directory.GetFiles(model_PATH, "*.prefab", SearchOption.TopDirectoryOnly);

            if (humanModelFiles == null || humanModelFiles.Length == 0)
            {
                UtilExtension.QuitWithLogError(
                    $"[{nameof(LocalFilePedestrianModelProvider)}] Invalid models folder : {model_PATH}");
                return null;
            }
            else
            {
                if (RandomPickModels)
                {
                    while (isDuplicateModel)
                    {
                        humanModelPath = humanModelFiles[UnityEngine.Random.Range(0, humanModelFiles.Length)];

                        if (!usedHumanModels.Contains(humanModelPath))
                        {
                            isDuplicateModel = false;
                        }
                    }
                }
                else
                {
                    humanModelPath = humanModelFiles[modelFileIndex];
                }

                // Find the index of "Resources" in the file path
                int resourcesIndex = humanModelPath.IndexOf("Resources");

                // If "Resources" is found in the file path
                if (resourcesIndex != -1)
                {
                    // Remove the "Resources" part and the ".prefab" postfix from the file path
                    humanModelPath = humanModelPath.Substring(resourcesIndex + "Resources".Length + 1);
                    Debug.Log(humanModelPath);
                    //humanModel = System.IO.Path.GetFileNameWithoutExtension(humanModel);
                }
                else
                {
                    UtilExtension.QuitWithLogError(
                        $"[{nameof(LocalFilePedestrianModelProvider)}] File path does not contain 'Resources' folder. Check the field of Human Models_Folder in inspector");
                }

                humanModelPath = humanModelPath.Replace(".prefab", "");
                Debug.Log($"[{nameof(LocalFilePedestrianModelProvider)}] Reading model {humanModelPath}");
                GameObject humanPrefab = Resources.Load<GameObject>(humanModelPath);

                if (RandomPickModels)
                {
                    usedHumanModels.Add(humanModelPath);
                    if (usedHumanModels.Count >= humanModelFiles.Length)
                    {
                        usedHumanModels.Clear();
                    }
                }
                else
                {
                    if (modelFileIndex + 1 == humanModelFiles.Length)
                    {
                        modelFileIndex = 0;
                    }

                    modelFileIndex++;
                }

                return Instantiate(humanPrefab);
            }
        }
    }

    public static class LocalFilePedestrianModelProviderExtention
    {
        public static bool CheckModelPath(string path, bool fromEditor = false)
        {
            // Check if the selected path includes "Resources/Models"
            if (!path.Contains(Path.Combine("Resources", "Models")))
            {
                if (!fromEditor)
                {
                    UtilExtension.QuitWithLogError(
                        $"[{nameof(LocalFilePedestrianModelProvider)}] The selected path does not include {Path.Combine("Resources", "Models")}. Please select a valid folder.");
                }

                return false;
            }

            return true;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(LocalFilePedestrianModelProvider))]
    public class LocalFileModelProviderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            LocalFilePedestrianModelProvider provider = (LocalFilePedestrianModelProvider)target;

            // Add a button to open the folder panel
            if (GUILayout.Button("Select Pedestrian Model Path"))
            {
                // Open folder panel and get the selected path
                string selectedPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Model Folder", "", "");

                // If a path was selected (cancel wasn't pressed)
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (!LocalFilePedestrianModelProviderExtention.CheckModelPath(selectedPath, true))
                    {
                        UnityEditor.EditorUtility.DisplayDialog("Invalid Path",
                            "The selected path does not include 'Resources/Models'. Please select a valid folder.",
                            "OK");
                        return;
                    }

                    // Get all files in the selected directory
                    string[] files = Directory.GetFiles(selectedPath);

                    // Check if the directory contains only .prefab files and their .meta files
                    if (files.Any(
                            file => Path.GetExtension(file) != ".prefab" && Path.GetExtension(file) != ".meta"))
                    {
                        UnityEditor.EditorUtility.DisplayDialog("Invalid Contents",
                            "The selected folder must contain only .prefab files and their respective .meta files.",
                            "OK");
                        return;
                    }

                    // If everything is fine, set the selected path
                    provider.model_PATH = selectedPath;

                    // Mark the object as changed so the new path is saved
                    UnityEditor.EditorUtility.SetDirty(provider);
                }
            }
        }
    }

#endif
}