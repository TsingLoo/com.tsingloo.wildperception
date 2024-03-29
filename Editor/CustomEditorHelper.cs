using System.IO;
using UnityEditor;
using UnityEngine;

namespace WildPerception
{
    [CustomEditor(typeof(MainController))]
    public class MainControllerCustom : Editor
    {
        SerializedProperty multiviewXPerceptionFolderProp;

        private void OnEnable()
        {
            // Link the serialized property to the actual serialized field from the target.
            multiviewXPerceptionFolderProp = serializedObject.FindProperty("MultiviewX_Perception_Folder");
        }

        public override void OnInspectorGUI()
        {
            // Load the actual serialized properties into the serialized object.
            serializedObject.Update();

            // Draw the default inspector without the MultiviewX_Perception_Folder property
            DrawPropertiesExcluding(serializedObject, "MultiviewX_Perception_Folder");

            EditorGUILayout.PropertyField(multiviewXPerceptionFolderProp);

            // New button for selecting the MultiviewX_Perception_Folder
            if (GUILayout.Button("Select MultiviewX_Perception Folder"))
            {
                bool folderSelected = false;
                string selectedPath = "";

                while (!folderSelected)
                {
                    selectedPath = EditorUtility.OpenFolderPanel("Select MultiviewX_Perception Folder", "", "");

                    // Check if the path was selected (cancel was not pressed)
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        if (UtilExtension.CheckPythonPath(selectedPath, true))
                        {
                            multiviewXPerceptionFolderProp.stringValue = selectedPath;
                            serializedObject.ApplyModifiedProperties(); // Apply the changes to the serialized object
                            folderSelected = true;
                        }
                        else
                        {
                            bool tryAgain = EditorUtility.DisplayDialog(
                                "File Not Found",
                                "'run_all.py' was not found in the selected directory. Please select the correct folder.",
                                "Try Again",
                                "Cancel"
                            );

                            if (!tryAgain)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // User pressed cancel
                        break;
                    }
                }
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Init Scene"))
            {
                ((MainController)target).InitScene();
            }

            GUILayout.Space(5f);

            if (GUILayout.Button("Assign Transform"))
            {
                ((MainController)target).AssignTransform();
            }

            // Apply changes to the serialized object
            serializedObject.ApplyModifiedProperties();
        }
    }

    public class AddSceneControllerEditorWindow : EditorWindow
    {
        [MenuItem("Window/WildPerception/Add SceneController to Scene")]
        public static void AddSceneControllerToScene()
        {
            GameObject
                prefab = Resources.Load(
                    "SceneController") as GameObject; // Load the SceneController prefab from Resources folder
            if (prefab != null)
            {
                GameObject newObject =
                    PrefabUtility.InstantiatePrefab(prefab) as GameObject; // Instantiate the prefab
                if (newObject != null)
                {
                    Undo.RegisterCreatedObjectUndo(newObject,
                        "Add SceneController"); // Register the object for undo
                    Selection.activeGameObject = newObject; // Select the newly added object in the hierarchy
                }
                else
                {
                    Debug.LogError(
                        "Failed to add SceneController prefab to scene."); // Log an error if prefab instantiation fails
                }
            }
            else
            {
                Debug.LogError(
                    "SceneController prefab not found in Resources folder."); // Log an error if prefab is not found
            }
        }

        [MenuItem("Window/WildPerception/Add Camera_Perception")]
        public static void AddPerceptionCameraPrefabToScene()
        {
            GameObject prefab = Resources.Load("Camera_Perception") as GameObject;
            if (prefab != null)
            {
                GameObject newObject =
                    PrefabUtility.InstantiatePrefab(prefab) as GameObject; // Instantiate the prefab
                if (newObject != null)
                {
                    Undo.RegisterCreatedObjectUndo(newObject,
                        "Add Camera_Perception"); // Register the object for undo
                    Selection.activeGameObject = newObject; // Select the newly added object in the hierarchy
                }
                else
                {
                    Debug.LogError(
                        "Failed to add Camera_Perception to scene."); // Log an error if prefab instantiation fails
                }
            }
            else
            {
                Debug.LogError(
                    "Camera_Perception not found in Resources folder."); // Log an error if prefab is not found
            }
        }

        [MenuItem("Assets/Extract Animation Clip")]
        private static void ExtractFbxAnimationClips()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            foreach (GameObject obj in selectedObjects)
            {
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(obj));
                var outputDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));
                foreach (var item in subAssets)
                {
                    if (item is AnimationClip animClip)
                    {
                        AnimationClip newClip = new AnimationClip();
                        EditorUtility.CopySerialized(animClip, newClip);
                        string animFile = string.Format("{0}/{1}.anim", outputDir, animClip.name);
                        AssetDatabase.CreateAsset(newClip, animFile);
                    }
                }
            }

            AssetDatabase.Refresh();
        }
    }
}