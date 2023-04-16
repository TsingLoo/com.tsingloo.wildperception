using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.Consumers;
using UnityEngine.Perception.Settings;

namespace WildPerception {	
	[RequireComponent(typeof(PeopleManager))]
	[RequireComponent(typeof(CalibrateTool))]
	public class MainController : SingletonForMonobehaviour<MainController>
	{
    #region Inspector
	
	    public string MultiviewX_Folder = "D:\\PycharmProjects\\MultiviewX_FYP";
	    public string humanModels_Folder = "C:\\FYP\\FYP_UnityProjects\\Assets\\Bundles\\Resources\\Prefabs";
	
	    public GameObject GridOrigin_OpenCV;
	    public GameObject Center_HumanSpawn_CameraLookAt;
	    public GameObject HandPlacedCameraParent;
	    public void InitScene() 
	    {
	        InitKeyObj(nameof(GridOrigin_OpenCV));
	        InitKeyObj(nameof(Center_HumanSpawn_CameraLookAt));
	        InitKeyObj(nameof(HandPlacedCameraParent));
	
	        AssignTransform();
	    }
	
	    void InitKeyObj(string objname) 
	    {
	        var obj = GameObject.Find(objname);
	        if (obj == null)
	        {
	            Debug.LogWarning($"[{nameof(MainController)}] {objname} Not found in scene, create one.");
	            obj= new GameObject(objname);
	        }
	    }
	
    #endregion
	
	    CameraManager cameraManager;
	    public PeopleManager peopleManager;
	    CalibrateTool calibrateTool;
	
	    //[HideInInspector] public string Image_subsets;
	    [HideInInspector] public string matchings;
	    [HideInInspector] public string validate;
	
	    private void Awake()
	    {
	        AssignTransform();
	        matchings = Path.Join(MultiviewX_Folder, nameof(matchings));
	        validate = Path.Join(MultiviewX_Folder, nameof(validate));
	        Prepare();
	    }
	
	    public void AssignTransform()
	    {
	        cameraManager = GetComponent<CameraManager>();
	        peopleManager = GetComponent<PeopleManager>();
	        calibrateTool = GetComponent<CalibrateTool>();
	
	        cameraManager.handPlacedCameraParent = HandPlacedCameraParent.transform;
	        cameraManager.gridOrigin = GridOrigin_OpenCV.transform;
	        cameraManager.center = Center_HumanSpawn_CameraLookAt.transform;
	
	        calibrateTool.gridOrigin = GridOrigin_OpenCV.transform;
	        calibrateTool.chessboardGenerateCenter = Center_HumanSpawn_CameraLookAt.transform;
	    }
	
	    public void DeleteFolder(string foldername) 
	    {
	        DirectoryInfo dir = new DirectoryInfo(foldername);
	
	        if (dir.Exists)
	        {
	            dir.Delete(true);
	            Debug.Log("[IO]" + foldername + " have been deleted");
	        }
	    }
	
	
	    void Prepare() 
	    {
	        DeleteFolder(matchings);
	
	        Directory.CreateDirectory(matchings);
	        Debug.Log($"[{nameof(MainController)}][IO]Folder " + matchings + " created.");
	        //DeleteFolder(Image_subsets);
	        cameraManager.PlaceCamera(this);
	
	        calibrateTool.camerasToCalibrate = cameraManager.cams.ToList();
	        calibrateTool.targetParentFolder = MultiviewX_Folder;
	        calibrateTool.gridOrigin = GridOrigin_OpenCV.transform;

			peopleManager.model_PATH = humanModels_Folder;
	
	        var p = PerceptionSettings.GetOutputBasePath();
	        if (!Directory.Exists(p))
	        {
	            CalibrateTool.Instance.PERCEPTION_PATH = PerceptionSettings.defaultOutputPath;
	        }
	        else
	        {
	            CalibrateTool.Instance.PERCEPTION_PATH = p;
	        }
	
	        if(DatasetCapture.activateEndpoint.GetType() ==  typeof(SoloEndpoint)) 
	        {
	            Debug.LogError($"[{nameof(MainController)}]SoloEndpoint is currently not supported, please turn to Edit => Project Settings => Perception => Change Endpoint Type to use the Perception Endpoint");
#if UNITY_EDITOR
	            EditorApplication.ExitPlaymode();
#else
	            Application.Quit();
#endif
	        }
	    }
	}
}