using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.Consumers;
using UnityEngine.Perception.Settings;
using UnityEngine.Serialization;

namespace WildPerception {	
	[RequireComponent(typeof(PedestriansManager))]
	[RequireComponent(typeof(CalibrateTool))]
	public class MainController : SingletonForMonobehaviour<MainController>
	{
    #region Inspector
    
	    public GameObject GridOrigin_OpenCV;
	    public GameObject Center_HumanSpawn_CameraLookAt;
	    public GameObject HandPlacedCameraParent;

	    [Header("Python Code Path")]
	    public string MultiviewX_Perception_Folder = "D:\\PycharmProjects\\MultiviewX_Perception";
	    //public string humanModels_Folder = "com.tsingloo.wildperception\\Resources\\Models";

	    
	    [Header("PedestrianModelProvider")]
	    [SerializeField] private AbstractPedestrianModelProvider pedestrianModelProvider;
	    
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

        [HideInInspector] public CameraManager cameraManager;
	    public PedestriansManager pedestriansManager;
	    [HideInInspector] public CalibrateTool calibrateTool;
	
	    //[HideInInspector] public string Image_subsets;
	    [HideInInspector] public string matchings;
	    [HideInInspector] public string validate;
	
	    private void Awake()
	    {
		    if (!File.Exists(Path.Combine(MultiviewX_Perception_Folder, "run_all.py")))
		    {
				UtilExtension.QuitWithLogError($"Wrong {nameof(MultiviewX_Perception_Folder)}");
		    }
		    
	        AssignTransform();
	        matchings = Path.Join(MultiviewX_Perception_Folder, nameof(matchings));
	        validate = Path.Join(MultiviewX_Perception_Folder, nameof(validate));
	        Prepare();
	    }
	
	    public void AssignTransform()
	    {
	        cameraManager = GetComponent<CameraManager>();
	        pedestriansManager = GetComponent<PedestriansManager>();
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
	        calibrateTool.targetParentFolder = MultiviewX_Perception_Folder;
	        calibrateTool.gridOrigin = GridOrigin_OpenCV.transform;

	        pedestriansManager.pedestrianModelProvider = pedestrianModelProvider;
	
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
				UtilExtension.QuitWithLogError($"[{nameof(MainController)}] SoloEndpoint is currently not supported, please turn to Edit => Project Settings => Perception => Change Endpoint Type to use the Perception Endpoint");
	        }
	    }
	}
}
