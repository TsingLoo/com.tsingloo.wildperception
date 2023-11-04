using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WildPerception
{
    public class LocalFilePedestrianModelProvider : AbstractPedestrianModelProvider
    {
        List<string> usedHumanModels = new List<string>(); // List to keep track of used human model names
        
        /// <summary>
        /// The selected folder path must exactly contains "Resources/Model", for example: D:/UnityProjects/com.tsingloo.wildperception/Resources/Models
        /// </summary>
        public string model_PATH = @"Please Select Model Folder";

        private void Awake()
        {
            // Check if the selected path includes "Resources/Models"
            if (!model_PATH.Contains("Resources/Models"))
            {
                UtilExtension.QuitWithLogError($"[{nameof(LocalFilePedestrianModelProvider)}] The selected path does not include 'Resources/Models'. Please select a valid folder.");
                return;
            }

            // Get all files in the selected directory
            string[] files = Directory.GetFiles(model_PATH);

            // Check if the directory contains only .prefab files and their .meta files
            if (files.Any(
                    file => Path.GetExtension(file) != ".prefab" && Path.GetExtension(file) != ".meta"))
            {
                UtilExtension.QuitWithLogError($"[{nameof(LocalFilePedestrianModelProvider)}] The selected folder must contain only .prefab files and their respective .meta files.");
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
                UtilExtension.QuitWithLogError($"[{nameof(LocalFilePedestrianModelProvider)}] Invalid models folder : {model_PATH}");
                return null;
            }
            else
            {
                while (isDuplicateModel)
                {
                    humanModelPath = humanModelFiles[UnityEngine.Random.Range(0, humanModelFiles.Length)];

                    if (!usedHumanModels.Contains(humanModelPath))
                    {
                        isDuplicateModel = false;
                    }
                }

                // Find the index of "Resources" in the file path
                int resourcesIndex = humanModelPath.IndexOf("Resources");

                // If "Resources" is found in the file path
                if (resourcesIndex != -1)
                {
                    // Remove the "Resources" part and the ".prefab" postfix from the file path
                    humanModelPath = humanModelPath.Substring(resourcesIndex + "Resources".Length + 1);
                    //Debug.Log(humanModel);
                    //humanModel = System.IO.Path.GetFileNameWithoutExtension(humanModel);
                }
                else
                {
                    UtilExtension.QuitWithLogError(
                        $"[{nameof(LocalFilePedestrianModelProvider)}] File path does not contain 'Resources' folder. Check the field of Human Models_Folder in inspector");
                }

                humanModelPath = humanModelPath.Replace(".prefab", "");
                Debug.Log(humanModelPath);
                GameObject humanPrefab = Resources.Load<GameObject>(humanModelPath);
                usedHumanModels.Add(humanModelPath);
                if (usedHumanModels.Count >= humanModelFiles.Length)
                {
                    usedHumanModels.Clear();
                }
                return humanPrefab;
                //GameObject humanPrefab = Resources.Load<GameObject>(PATH + Randoms(0, modelCount, preset_humans).ToString());
            }
        }
    }
}