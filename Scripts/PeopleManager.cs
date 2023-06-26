using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace WildPerception {	
	
	public class PeopleManager : SingletonForMonobehaviour<PeopleManager>
	{

		[SerializeField] RuntimeAnimatorController defaultAnimator;
	    [HideInInspector] public string model_PATH = @"Prefabs/Models/";
	
	    /// <summary>
	    /// The target num of models in scene 
	    /// </summary>
	    public int add_human_count;
	    public int preset_humans;
	
	
	
	    //Change the Respawn area size 
	    public float largestX;
	    public float smallestX;
	    public float largestZ;
	    public float smallestZ;
	
	    //Change the outter bound area
	    public float outterBoundRadius = 25;
	
	    public float walkingSpeed;
	  
	
	
	    //keep track of the each person
	    [HideInInspector] public List<PersonBound> bounds_list = new List<PersonBound>(25);
	    [HideInInspector] public List<int> PID_List = new List<int>(25);
	    List<string> usedHumanModels = new List<string>(); // List to keep track of used human model names
	                                                       //ArrayList humanList = new ArrayList();  //list of Index of human, starts from zero
	
	
	    [Header("Gizmos")]
	    bool popWarning = true;
	    [SerializeField] bool DrawRespawnArea = true;
	    [SerializeField] bool DrawOutterBound = true;
	
	    CameraManager cameraManager;
	    CalibrateTool calibrateTool;
		MainController mainController;
	
	    void Start()
	    {
	        cameraManager = GetComponent<CameraManager>();
	        calibrateTool = GetComponent<CalibrateTool>();
			mainController = GetComponent<MainController>();
	
	        SpawnOriginalHumans(preset_humans);
	        StartCoroutine(nameof(CheckInsideCircle));
	
	        if (outterBoundRadius < Mathf.Min(calibrateTool.MAP_WIDTH * calibrateTool.Scaling, calibrateTool.MAP_HEIGHT * calibrateTool.Scaling)*0.55f)
	        {
	            Debug.LogWarning($"[{nameof(PeopleManager)}]Please increase the ourbound radius");
	            outterBoundRadius = Mathf.Max(calibrateTool.MAP_WIDTH * calibrateTool.Scaling, calibrateTool.MAP_HEIGHT * calibrateTool.Scaling)*0.8f;
	        }
	    }
	
	
	    IEnumerator CheckInsideCircle()
	    {
	        //Debug.Log($"[{nameof(PeopleManager)}]Check circle");
	        while (true)
	        {
	            for (int i = 0; i < bounds_list.Count; i++)
	            {
	                var otherObject = bounds_list[i].gameObject;
	                // Calculate the distance between the center of the circle and the position of the other object
	                float distance = Vector3.Distance(cameraManager.center.position, otherObject.transform.position);
	                //Debug.Log($"[{nameof(PeopleManager)}]{otherObject.name} Distance is {distance}");
	                // If the distance is less than the radius, the other object is inside the circle
	                if (distance > outterBoundRadius)
	                {
	                   Destroy(otherObject);
	                   bounds_list.Remove(bounds_list[i]);
	                   SpawnObjectsOutofPOMInCircle();
	                }
	
	            }
	            yield return new WaitForSeconds(5f); // Wait for 5 seconds
	        }
	    }
	
	    void SpawnOriginalHumans(int nums = 1)
	    {
	        Vector3 SpawnPosition;
	        for (int i = 0; i < nums; i++)
	        {
	            float X = Random.Range(smallestX, largestX);
	            float Z = Random.Range(smallestZ, largestZ);
	            SpawnPosition = GetComponent<MainController>().Center_HumanSpawn_CameraLookAt.transform.position + new Vector3(X, 0, Z);
	            SpawnPosition = new Vector3(SpawnPosition.x, cameraManager.gridOrigin.position.y, SpawnPosition.z);
	            SpawnHuman(SpawnPosition);
	        } 
	    }
	
	    void SpawnObjectsOutofPOMInCircle()
	    {
	        var originPosition = CalibrateTool.Instance.gridOrigin.position;
	
	        Vector2 randomPoint = Random.insideUnitCircle * outterBoundRadius;
	
	        Vector3 spawnPosition = new Vector3(
	            originPosition.x + randomPoint.x + CalibrateTool.Instance.MAP_WIDTH * CalibrateTool.Instance.Scaling / 2f,
	            originPosition.y,
	            originPosition.z + randomPoint.y + CalibrateTool.Instance.MAP_HEIGHT* CalibrateTool.Instance.Scaling / 2f
	        );
	
	        while (spawnPosition.x > originPosition.x &&
	                   spawnPosition.x < originPosition.x + CalibrateTool.Instance.MAP_WIDTH * CalibrateTool.Instance.Scaling &&
	                   spawnPosition.z > originPosition.z &&
	                   spawnPosition.z < originPosition.z + CalibrateTool.Instance.MAP_HEIGHT * CalibrateTool.Instance.Scaling)
	        {
	            // Generate a new random point
	            randomPoint = Random.insideUnitCircle * outterBoundRadius;
	
	            // Update the spawn position
	            spawnPosition = new Vector3(
	                originPosition.x + randomPoint.x + CalibrateTool.Instance.MAP_WIDTH * CalibrateTool.Instance.Scaling / 2f,
	                originPosition.y,
	                originPosition.z + randomPoint.y + CalibrateTool.Instance.MAP_HEIGHT * CalibrateTool.Instance.Scaling / 2f
	            );
	        }
	
	        // Spawn the human at the calculated spawn position
	        SpawnHuman(spawnPosition);
	    }
	
	    void SpawnHuman(Vector3 SpawnPosition)
	    {
	        string humanModel = "";
	        bool isDuplicateModel = true;
			//Debug.Log($"[{nameof(PeopleManager)}]Assets/Bundles/Resources/{PATH}");
			//Debug.Log(model_PATH);
	        string[] humanModelFiles = Directory.GetFiles(model_PATH, "*.prefab", SearchOption.TopDirectoryOnly);
	        while (isDuplicateModel)
	        {
	            humanModel = humanModelFiles[UnityEngine.Random.Range(0, humanModelFiles.Length)];
	
	            if (!usedHumanModels.Contains(humanModel))
	            {
	                isDuplicateModel = false;
	            }
	        }

            // Find the index of "Resources" in the file path
            int resourcesIndex = humanModel.IndexOf("Resources");

            // If "Resources" is found in the file path
            if (resourcesIndex != -1)
            {
                // Remove the "Resources" part and the ".prefab" postfix from the file path
                humanModel = humanModel.Substring(resourcesIndex + "Resources".Length + 1);
				//Debug.Log(humanModel);
                //humanModel = System.IO.Path.GetFileNameWithoutExtension(humanModel);
            }
            else
            {
				UtilExtension.QuitWithLogError($"[{nameof(PeopleManager)}] File path does not contain 'Resources' folder. Check path Human Models_Folder");
			}
            humanModel = humanModel.Replace(".prefab", "");
            Debug.Log(humanModel);
            GameObject humanPrefab = Resources.Load<GameObject>(humanModel);
	        usedHumanModels.Add(humanModel);
	        if (usedHumanModels.Count >= humanModelFiles.Length)
	        {
	            usedHumanModels.Clear();
	        }
	        //GameObject humanPrefab = Resources.Load<GameObject>(PATH + Randoms(0, modelCount, preset_humans).ToString());
	        GameObject human = Instantiate(humanPrefab, SpawnPosition, UnityEngine.Random.rotation);
			var ani = human.GetComponent<Animator>();
            if (ani != null && ani.avatar != null && ani.avatar.isValid)
            {
                if (ani.runtimeAnimatorController == null)
                {
                    ani.runtimeAnimatorController = defaultAnimator;
                }
            }
            else
            {
                UtilExtension.QuitWithLogError($"[{nameof(PeopleManager)}] {humanModel} is not a human model with a valid Avatar. Check path Human Models_Folder");
                return;
            }

			var nav = human.GetOrAddComponent<NavMeshAgent>();
	        var bound = human.GetOrAddComponent<PersonBound>();
			bound.gridOriginPos = mainController.GridOrigin_OpenCV.transform.position;
	        var pc = human.GetOrAddComponent<PersonController>();
			pc.peopleManager = this;
			nav.speed = walkingSpeed;
	        bounds_list.Add(bound);
	        int PID = UnityEngine.Random.Range(0, 1000);
	        while (PID_List.Contains(PID))
	        {
	            PID = UnityEngine.Random.Range(0, 1000);
	        }
	        bound.PID = PID;
	    }
	
	
	    private void OnDrawGizmos()
	    {
	        MainController  mc = GetComponent<MainController>();
	
	        if (mc.Center_HumanSpawn_CameraLookAt != null)
	        {
	            if (DrawRespawnArea)
	            {
	                Gizmos.color = Color.green;
	                Gizmos.DrawLine(new Vector3(smallestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position, new Vector3(largestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
	                Gizmos.DrawLine(new Vector3(smallestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position, new Vector3(smallestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
	                Gizmos.DrawLine(new Vector3(largestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position, new Vector3(largestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
	                Gizmos.DrawLine(new Vector3(largestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position, new Vector3(smallestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
	            }
	            if (DrawOutterBound)
	            {
	                Gizmos.color = Color.red;
	                Gizmos.DrawWireSphere(mc.Center_HumanSpawn_CameraLookAt.transform.position, outterBoundRadius);
	            }
	        }
	        else if(popWarning)
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
