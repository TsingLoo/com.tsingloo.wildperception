using UnityEngine;
using UnityEngine.AI;


namespace WildPerception 
{
    public class GoRandomBasePedestrianBehaviour : BasePedestrianBehaviour
    {
        NavMeshAgent nav;
        
        [Range(-0.2f, 0.2f)]
        [SerializeField] private float walkingSpeedBias;


        void Start()
        {
            nav = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            nav.speed = nav.speed + walkingSpeedBias;

            gameObject.transform.localScale = Vector3.one * CalibrateTool.Instance.Scaling;
            InvokeRepeating(nameof(GoToRandomPosition), 2, 2 + Random.Range(0f, 2f));
            //respawnController SpawnController = GameObject.Find("SpawnController").GetComponent<respawnController>();
            //smallestX = SpawnController.smallestX;
            //largestX = SpawnController.largestX;
            //smallestZ = SpawnController.smallestZ;
            //largestZ = SpawnController.largestZ;

        }
        // Update is called once per frame
        void Update()
        {
            if (animator == null)
            {
                return;
            }
            int velocity = Animator.StringToHash("Velocity");
            if (transform.position.x == nav.destination.x && transform.position.z == nav.destination.z)
            {
                //Debug.Log("Stooooop!");
                nav.isStopped = true;
                //animator.SetBool("isWalking", false);
            }
            animator.SetFloat(velocity, nav.velocity.magnitude);
            if (!nav.isStopped)
            {
                /* Unable to rotate to the correct angle
                 * probably because of misunderstanding of quaternion functions 
                 *
                int rotation = Animator.StringToHash("Rotation");
                float rotateAngle = Quaternion.LookRotation(nav.destination - transform.position).eulerAngles.y - transform.rotation.eulerAngles.y;
                if (rotateAngle > 180)
                {
                    rotateAngle = 360 - rotateAngle;
                }
                else if (rotateAngle < -180)
                {
                    rotateAngle = -360 - rotateAngle;
                }
                float r = rotateAngle * (1.0f / 180.0f);
                */



                // Debug.Log(r);
                // animator.SetFloat(rotation, r);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(nav.destination - transform.position), 2);
                //animator.SetFloat(rotation, 0);

            }
        }

        void GoToRandomPosition()
        {

            float y = transform.position.y;
            float x = Random.Range(pedestriansManager.smallestX, pedestriansManager.largestX);
            float z = Random.Range(pedestriansManager.smallestZ, pedestriansManager.largestZ);

            nav.destination = new Vector3(x, y, z);
            nav.isStopped = false;
        }

        private void OnDrawGizmos()
        {
            if (!nav.pathPending)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawLine(transform.position, nav.destination);
        }
    }
}
