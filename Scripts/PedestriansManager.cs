using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


namespace WildPerception
{
    public class PedestriansManager : SingletonForMonobehaviour<PedestriansManager>
    {
        [HideInInspector] public AbstractPedestrianModelProvider pedestrianModelProvider;

        //public BasePedestrianBehaviourParameters PedestrianBehaviourParameters;
        
        /// <summary>
        /// The target num of models in scene 
        /// </summary>
        public int TotalHumanCount;

        [Header("Respawn Area Settings")]
        //Change the Respawn area size 
        public float largestX;
        public float smallestX;
        public float largestZ;
        public float smallestZ;

        [Header("Outter Bound Settings")]
        //Change the outter bound area
        public float outterBoundRadius = 25;

        [Header("NavMeshAgent Settings")]
        [SerializeField] RuntimeAnimatorController defaultAnimator;
        [SerializeField] float walkingSpeed;
        [SerializeField] float baseOffset;

        //keep track of the each person
        [HideInInspector] public List<PersonBound> bounds_list = new List<PersonBound>(25);

        [HideInInspector] public List<int> PID_List = new List<int>(128);
        //ArrayList humanList = new ArrayList();  //list of Index of human, starts from zero

        [Space(12)]
        [Header("Gizmos")]
        [SerializeField] bool popWarning = true;
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

            SpawnOriginalHumans(TotalHumanCount);
            StartCoroutine(nameof(CheckInsideCircle));

            if (outterBoundRadius < Mathf.Min(calibrateTool.MAP_WIDTH * calibrateTool.Scaling,
                    calibrateTool.MAP_HEIGHT * calibrateTool.Scaling) * 0.55f)
            {
                Debug.LogWarning($"[{nameof(PedestriansManager)}]Please increase the outbound radius");
                outterBoundRadius = Mathf.Max(calibrateTool.MAP_WIDTH * calibrateTool.Scaling,
                    calibrateTool.MAP_HEIGHT * calibrateTool.Scaling) * 0.8f;
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
                SpawnPosition = GetComponent<MainController>().Center_HumanSpawn_CameraLookAt.transform.position +
                                new Vector3(X, 0, Z);
                SpawnPosition = new Vector3(SpawnPosition.x, cameraManager.gridOrigin.position.y, SpawnPosition.z);
                SpawnHuman(SpawnPosition);
            }
        }

        void SpawnObjectsOutofPOMInCircle()
        {
            var originPosition = CalibrateTool.Instance.gridOrigin.position;

            Vector2 randomPoint = Random.insideUnitCircle * outterBoundRadius;

            Vector3 spawnPosition = new Vector3(
                originPosition.x + randomPoint.x +
                CalibrateTool.Instance.MAP_WIDTH * CalibrateTool.Instance.Scaling / 2f,
                originPosition.y,
                originPosition.z + randomPoint.y +
                CalibrateTool.Instance.MAP_HEIGHT * CalibrateTool.Instance.Scaling / 2f
            );

            while (spawnPosition.x > originPosition.x &&
                   spawnPosition.x < originPosition.x +
                   CalibrateTool.Instance.MAP_WIDTH * CalibrateTool.Instance.Scaling &&
                   spawnPosition.z > originPosition.z &&
                   spawnPosition.z < originPosition.z +
                   CalibrateTool.Instance.MAP_HEIGHT * CalibrateTool.Instance.Scaling)
            {
                // Generate a new random point
                randomPoint = Random.insideUnitCircle * outterBoundRadius;

                // Update the spawn position
                spawnPosition = new Vector3(
                    originPosition.x + randomPoint.x +
                    CalibrateTool.Instance.MAP_WIDTH * CalibrateTool.Instance.Scaling / 2f,
                    originPosition.y,
                    originPosition.z + randomPoint.y +
                    CalibrateTool.Instance.MAP_HEIGHT * CalibrateTool.Instance.Scaling / 2f
                );
            }

            // Spawn the human at the calculated spawn position
            SpawnHuman(spawnPosition);
        }

        void SpawnHuman(Vector3 SpawnPosition)
        {
            GameObject human = Instantiate(pedestrianModelProvider.GetPedestrianModel(), SpawnPosition, UnityEngine.Random.rotation);
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
                UtilExtension.QuitWithLogError(
                    $"[{nameof(PedestriansManager)}] {human} is not a human model with a valid Avatar. Check path Human Models_Folder");
                return;
            }

            var nav = human.GetOrAddComponent<NavMeshAgent>();
            var bound = human.GetOrAddComponent<PersonBound>();
            bound.gridOriginPos = mainController.GridOrigin_OpenCV.transform.position;
            var pc = human.GetOrAddComponent<GoRandomBasePedestrianBehaviour>();
            pc.pedestriansManager = this;
            nav.speed = walkingSpeed;
            nav.baseOffset = baseOffset;
            bounds_list.Add(bound);
            int PID = UnityEngine.Random.Range(0, 1000);
            while (PID_List.Contains(PID))
            {
                PID = UnityEngine.Random.Range(0, 1000);
            }

            bound.PID = PID;
        }
        
        // Call this method with the type of behavior you want to add
        public void AddBehavior<T>() where T : BasePedestrianBehaviour
        {
            // First, remove all existing behaviors that are mutually exclusive
            var existingBehaviors = GetComponents<BasePedestrianBehaviour>();
            foreach (var behavior in existingBehaviors)
            {
                if (behavior is T) return; // Behavior already added
                Destroy(behavior);
            }

            // Add the new behavior
            gameObject.AddComponent<T>();
        }


        private void OnDrawGizmos()
        {
            MainController mc = GetComponent<MainController>();

            if (mc.Center_HumanSpawn_CameraLookAt != null)
            {
                if (DrawRespawnArea)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(
                        new Vector3(smallestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position,
                        new Vector3(largestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
                    Gizmos.DrawLine(
                        new Vector3(smallestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position,
                        new Vector3(smallestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
                    Gizmos.DrawLine(
                        new Vector3(largestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position,
                        new Vector3(largestX, 0, smallestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
                    Gizmos.DrawLine(
                        new Vector3(largestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position,
                        new Vector3(smallestX, 0, largestZ) + mc.Center_HumanSpawn_CameraLookAt.transform.position);
                }

                if (DrawOutterBound)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(mc.Center_HumanSpawn_CameraLookAt.transform.position, outterBoundRadius);
                }
            }
            else if (popWarning)
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.LogWarning(
                        $"[{nameof(CameraManager)}]Go to SceneController => {nameof(MainController)} to init scene");
                    popWarning = false;
                }
            }
        }
    }
}