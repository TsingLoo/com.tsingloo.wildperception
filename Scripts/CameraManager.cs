using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Perception.GroundTruth;

namespace WildPerception {	
	public class CameraManager : SingletonForMonobehaviour<CameraManager>
	{
	    public int BeginFrameCount = 150;
	
	    public enum eCameraPlaceType
	    {
	        Ellipse_Auto,
	        ByHand
	    }
	
	    public eCameraPlaceType cameraPlaceType;
	
	    [Header("Ellipse_Auto Settings")]
	    [SerializeField] int level = 2;
	    [SerializeField] int numsPerLevel = 3;
	    [SerializeField] float heightFirstLevel = 1.8f;
	    [SerializeField] float hPerLevel = 1f;
	    [SerializeField] float majorAxis = 3f;
	    [SerializeField] float minorAxis = 2f;

	    private bool haveFinished;
	
	    [Tooltip("Set up this if you are setting the cameras in the scene by hand")]
	    [HideInInspector] public Transform handPlacedCameraParent;

	    [Space(12)]
	    [Header("ExportFrames")]
	    [Tooltip("Camera will be generated around the center, and these camera would look at this posistion")]
	    public int totalwantedFramesCount = 200;
	    [HideInInspector] public Transform center;
	    public const int RESOLUTION_WIDTH = 1920;
	    public const int RESOLUTION_HEIGHT = 1080;
	    [HideInInspector] public List<Camera> cams;
	    [HideInInspector] public Transform gridOrigin;
	
	    [Header("Gizmos")]
	    bool popWarning = true;
	    [SerializeField] bool drawEllipseGizmos = true;
	
	
	
	    [SerializeField] GameObject CameraPrefab;
	
	    int validFrameIndex;

	    public void OnFinishGenerating(int frameIndex)
	    {
		    if (!haveFinished)
		    {
			    haveFinished = true;
			    UtilExtension.QuitWithLog($"[{nameof(CameraManager)}] Finished {frameIndex} frames"); // Call the method with frameIndex
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
	                    PlaceObjByEllipse(center, numsPerLevel, ref cams, controller, majorAxis / CalibrateTool.Instance.Scaling, minorAxis / CalibrateTool.Instance.Scaling, (heightFirstLevel + i * hPerLevel) / CalibrateTool.Instance.Scaling);
	                }
	                break;
	
	            case (int)eCameraPlaceType.ByHand:
	                if (handPlacedCameraParent == null || handPlacedCameraParent.childCount == 0)
	                {
						UtilExtension.QuitWithLogError($"[{nameof(CameraManager)}]You are setting cams in scene by hand. Please check {nameof(handPlacedCameraParent)}");
	                }
	                else
	                {
	                    for (int i = 0; i < handPlacedCameraParent.childCount; i++)
	                    {
		                    var childCamera = handPlacedCameraParent.GetChild(i);
		                    if (!childCamera.gameObject.activeSelf) continue;
		                    childCamera.GetComponent<MatchingsExporter>().SetMatchingsExporter(i + 1,totalwantedFramesCount ,controller);
		                    cams.Add(handPlacedCameraParent.GetChild(i).GetComponent<Camera>());
	                    }
	                }
	                break;
	
	            default:
					UtilExtension.QuitWithLogError($"[{nameof(CameraManager)}]NO TARGET CAMERA PLACEMENT CASE");
	                break;
	        }
	    }
	
	
	    private void Awake()
	    {
	        //BeginFrameCount = PlayerPrefs.GetInt(SaveDataManager.START_FRAME) + Time.frameCount;
	        perceptionStartAtFrame = BeginFrameCount;
	        CameraPrefab.GetComponent<PerceptionCamera>().firstCaptureFrame = perceptionStartAtFrame;
	    }

        /// <summary>
        /// Genrate GameObjects by Ellipse
        /// </summary>
        /// <param name="go"></param>
        /// <param name="num"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void PlaceObjByEllipse(Transform lookatTransform, int num, ref List<Camera> cams, MainController controller, float a = 3, float b = 4, float height = 2.8f)
        {
            float step_angle = 360 / num;
            for (int i = 0; i < num; i++)
            {
                float angle = (i * step_angle / 180) * Mathf.PI;
                float xx = a * Mathf.Cos(angle);
                float yy = b * Mathf.Sin(angle);

                Vector3 pos = new Vector3(xx, height, yy);
                //Debug.Log("[]" + xx);

                var Obj = Instantiate(CameraPrefab, lookatTransform.position + pos, Quaternion.identity);
                cams.Add(Obj.GetComponent<Camera>());
                //Obj.GetComponent<Camera>().targetDisplay = i;
                var exporter = Obj.GetOrAddComponent<MatchingsExporter>();
                exporter.EndExportEvent.AddListener(OnFinishGenerating);
                exporter.SetMatchingsExporter(cams.Count, totalwantedFramesCount, controller);
                Obj.name = "Camera" + (exporter.cameraIndex).ToString();
                // Obj.transform.LookAt(nert);
                Obj.transform.LookAt(lookatTransform.position);
            }
        }


        private void OnDrawGizmos()
	    {
	        if (drawEllipseGizmos)
	        {
	            MainController mainController = GetComponent<MainController>();
				CalibrateTool cbt = GetComponent<CalibrateTool>();
	            if (mainController.Center_HumanSpawn_CameraLookAt != null)
	            {
	                Vector3 center = mainController.Center_HumanSpawn_CameraLookAt.transform.position + new Vector3(0, heightFirstLevel, 0)/cbt.Scaling;
	                float stepAngle = 360f / numsPerLevel;
	
	                for (int i = 0; i < numsPerLevel; i++)
	                {
                        // Calculate the angle for the current camera
                        float angle = (i * stepAngle / 180) * Mathf.PI;

                        // Calculate the position of the camera on the ellipse
                        float xx = majorAxis * Mathf.Cos(angle) / cbt.Scaling ;
	                    float yy = minorAxis  * Mathf.Sin(angle) / cbt.Scaling;
						//Debug.Log(xx);
	                    Vector3 cameraPosition = new Vector3(center.x + xx, center.y, center.z + yy);
	
	                    // Draw a gizmo at the position of the camera on the ellipse
	                    Gizmos.color = Color.red;
	                    Gizmos.DrawSphere(cameraPosition, 0.2f);
						Gizmos.DrawLine(cameraPosition, center - new Vector3(0, heightFirstLevel, 0) / cbt.Scaling);
	                }
	            }
	            else if (popWarning)
	            {
	                if (!EditorApplication.isPlaying)
	                {
	                    Debug.LogWarning($"[{nameof(CameraManager)}]Go to SceneController => {nameof(MainController)} to init scene");
	                    popWarning = false;
	                }
	            }
	        }
	    }	
	}	
}
