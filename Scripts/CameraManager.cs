using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Serialization;

namespace WildPerception
{
    public class CameraManager : SingletonForMonobehaviour<CameraManager>
    {
        [FormerlySerializedAs("totalwantedFramesCount")] [Header("Export Frames Settings")] [Space(8)]
        public int TotalWantedFramesCount = 200;

        [Space(8)]
        //[Tooltip("Camera will be generated around the center, and these camera would look at this posistion")]
        public int FirstDroppedFrameCount = 150;

        [HideInInspector] public List<Camera> cams;
        [HideInInspector] public Transform gridOrigin;
        [HideInInspector] public Transform center;
        [HideInInspector] public Transform cameraLookat;

        public enum eCameraPlaceType
        {
            Ellipse_Auto,
            ByHand
        }

        [Header("Camera Settings")] [SerializeField]
        GameObject CameraPrefab;

        public eCameraPlaceType cameraPlaceType;

        [Header("Ellipse_Auto Settings")] [SerializeField]
        int level = 2;

        [SerializeField] int numsPerLevel = 3;
        [SerializeField] float heightFirstLevel = 1.8f;
        [SerializeField] float hPerLevel = 1f;
        [SerializeField] float majorAxis = 3f;
        [SerializeField] float minorAxis = 2f;

        private bool haveFinished;

        //[Tooltip("Set up this if you are setting the cameras in the scene by hand")]
        [HideInInspector] public Transform handPlacedCameraParent;

        [Space(12)] [Header("Gizmos")] [SerializeField]
        bool popWarning = true;

        [SerializeField] bool drawEllipseGizmos = true;

        int validFrameIndex;

        public void OnFinishGenerating(bool isAutoEnd, int frameIndex)
        {
            if (!haveFinished)
            {
                haveFinished = true;
                if (isAutoEnd)
                {
                    frameIndex++;
                }

                UtilExtension.QuitWithLog($"[{nameof(CameraManager)}] Finished {frameIndex} frames");
                //StartCoroutine(DelayQuit(frameIndex, 5f)); // Coroutine with parameters
            }
        }


        public UnityAction<int> ExportThisFrame;
        public UnityAction EndExport;

        int perceptionStartAtFrame;

        public void PlaceCamera(MainController controller)
        {
            switch ((int)cameraPlaceType)
            {
                case (int)eCameraPlaceType.Ellipse_Auto:

                    controller.HandPlacedCameraParent.SetActive(false);
                    for (int i = 0; i < level; i++)
                    {
                        PlaceObjByEllipse(center, cameraLookat, numsPerLevel, ref cams, controller,
                            majorAxis / CalibrateTool.Instance.Scaling, minorAxis / CalibrateTool.Instance.Scaling,
                            (heightFirstLevel + i * hPerLevel) / CalibrateTool.Instance.Scaling);
                    }

                    break;

                case (int)eCameraPlaceType.ByHand:
                    if (handPlacedCameraParent == null || handPlacedCameraParent.childCount == 0)
                    {
                        UtilExtension.QuitWithLogError(
                            $"[{nameof(CameraManager)}] You are setting cams in scene by hand. Please check {nameof(handPlacedCameraParent)}");
                    }
                    else
                    {
                        for (int i = 0; i < handPlacedCameraParent.childCount; i++)
                        {
                            var childCamera = handPlacedCameraParent.GetChild(i);
                            if (!childCamera.gameObject.activeSelf) continue;
                            childCamera.GetComponent<MatchingsExporter>()
                                .SetMatchingsExporter(i + 1, TotalWantedFramesCount, controller);
                            cams.Add(handPlacedCameraParent.GetChild(i).GetComponent<Camera>());
                        }
                    }

                    break;

                default:
                    UtilExtension.QuitWithLogError($"[{nameof(CameraManager)}] NO TARGET CAMERA PLACEMENT CASE");
                    break;
            }
        }


        private void Awake()
        {
            //BeginFrameCount = PlayerPrefs.GetInt(SaveDataManager.START_FRAME) + Time.frameCount;
            perceptionStartAtFrame = FirstDroppedFrameCount;
            CameraPrefab.GetComponent<PerceptionCamera>().firstCaptureFrame = perceptionStartAtFrame;
        }

        /// <summary>
        /// Genrate GameObjects by Ellipse
        /// </summary>
        /// <param name="go"></param>
        /// <param name="num"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void PlaceObjByEllipse(Transform baseTransform, Transform LookAtCamera, int num, ref List<Camera> cams,
            MainController controller, float a = 3, float b = 4, float height = 2.8f)
        {
            float step_angle = 360 / num;
            for (int i = 0; i < num; i++)
            {
                float angle = (i * step_angle / 180) * Mathf.PI;
                float xx = a * Mathf.Cos(angle);
                float yy = b * Mathf.Sin(angle);

                Vector3 pos = new Vector3(xx, height, yy);
                //Debug.Log("[]" + xx);

                var Obj = Instantiate(CameraPrefab, baseTransform.position + pos, Quaternion.identity);
                cams.Add(Obj.GetComponent<Camera>());
                //Obj.GetComponent<Camera>().targetDisplay = i;
                var exporter = Obj.GetOrAddComponent<MatchingsExporter>();
                exporter.EndExportEvent.AddListener(OnFinishGenerating);
                exporter.SetMatchingsExporter(cams.Count, TotalWantedFramesCount, controller);
                Obj.name = "Camera" + (exporter.cameraIndex).ToString();
                // Obj.transform.LookAt(nert);
                Obj.transform.LookAt(LookAtCamera.position);
            }
        }


        private void OnDrawGizmos()
        {
            if (drawEllipseGizmos)
            {
                MainController mainController = GetComponent<MainController>();
                CalibrateTool cbt = GetComponent<CalibrateTool>();
                if (mainController.CameraLookat != null)
                {
                    Vector3 baseCenter = mainController.Center_HumanSpawn.transform.position +
                                         new Vector3(0, heightFirstLevel, 0) / cbt.Scaling;
                    float stepAngle = 360f / numsPerLevel;

                    for (int i = 0; i < numsPerLevel; i++)
                    {
                        // Calculate the angle for the current camera
                        float angle = (i * stepAngle / 180) * Mathf.PI;

                        // Calculate the position of the camera on the ellipse
                        float xx = majorAxis * Mathf.Cos(angle) / cbt.Scaling;
                        float yy = minorAxis * Mathf.Sin(angle) / cbt.Scaling;
                        //Debug.Log(xx);
                        Vector3 cameraPosition = new Vector3(baseCenter.x + xx, baseCenter.y, baseCenter.z + yy);

                        // Draw a gizmo at the position of the camera on the ellipse
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(cameraPosition, 0.2f);
                        Gizmos.DrawLine(cameraPosition, mainController.CameraLookat.transform.position);
                    }
                }
                else if (popWarning)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        Debug.LogWarning(
                            $"[{nameof(CameraManager)}] Go to SceneController => {nameof(MainController)} to init scene");
                        popWarning = false;
                    }
                }
            }
        }
    }
}