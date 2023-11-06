using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Perception.GroundTruth;

namespace WildPerception
{
    public class MatchingsExporter : MonoBehaviour
    {
        /// <summary>
        /// Automatically managed by MainController, the modification of this in inspector will be override 
        /// </summary>
        [HideInInspector] public int cameraIndex = -1;

        /// <summary>
        /// Automatically managed by MainController, the modification of this in inspector will be override
        /// </summary>
        [HideInInspector] public string filePath = @"\matchings";

        Camera cam;
        PerceptionCamera pCam;
        CameraManager cameraManager;
        PedestriansManager pedestriansManager;

        public UnityEvent<bool,int> EndExportEvent;

        [HideInInspector] public int totalFramesCount = int.MaxValue;

        int frameBias = 0;
        int frameIndex = 0;

        StreamWriter sw_2D;
        StreamWriter sw_3D;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            pCam = GetComponent<PerceptionCamera>();
        }

        // Use this for initialization
        void Start()
        {
            if (cameraIndex != -1)
            {
                pCam.id = cameraIndex.ToString();
                InitTextFile(filePath, ".txt", ref sw_2D);
                InitTextFile(filePath, "_3d.txt", ref sw_3D);
                frameIndex = -frameBias;
            }
        }

        void Update()
        {
            if (cameraIndex == -1) return;

            if (pCam.SensorHandle.ShouldCaptureThisFrame)
            {
                ExportThisFrameHandler();
            }
        }


        private void OnDisable()
        {
            EndExportHandler(false, frameIndex);
        }

        public void SetMatchingsExporter(int cameraIndex, int totalCount, MainController controller)
        {
            this.cameraIndex = cameraIndex;
            //Obj.name = "Camera" + (Obj.GetOrAddComponent<MatchingsExporter>().cameraIndex).ToString();
            this.filePath = controller.matchings;
            pedestriansManager = controller.pedestriansManager;
            this.totalFramesCount = totalCount;
        }

        void ExportThisFrameHandler()
        {
            if (frameIndex >= 0)
            {
                Debug.Log(
                    $"[{nameof(MatchingsExporter)}][IO] {gameObject.name} Export this frame(index) : {frameIndex}");
                //Debug.Log("[IO]Export matchings for this frame : " + (Time.frameCount - CameraManager.Instance.BeginFrameCount).ToString());

                foreach (var bound in pedestriansManager.bounds_list)
                {
                    bound.UpdateDataTuple();
                    WriteFileByLine((frameIndex).ToString() + " " + bound.GetDataTuple_2D(cam), sw_2D);
                    WriteFileByLine((frameIndex).ToString() + " " + bound.GetDataTuple_3D(cam), sw_3D);
                }
            }

            if (frameIndex + 1 >= totalFramesCount)
            {
                EndExportHandler(true, frameIndex);
                return;
            }
            
            frameIndex++;
        }


        private void InitTextFile(string path, string suffix, ref StreamWriter sw)
        {
            string file_name = "Camera" + cameraIndex + suffix;

            //FileInfo file_info = new FileInfo(MainController.Instance.matchings + "//" + file_name);
            FileInfo file_info = new FileInfo(path + "//" + file_name);

            if (!file_info.Exists)
            {
                sw = file_info.CreateText();
                Debug.Log("[IO] File " + Path.Combine(path, file_name) + " created.");
            }
            else
            {
                sw = file_info.AppendText();
            }
        }


        private void FinishFile(StreamWriter sw)
        {
            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
            }
        }

        public void WriteFileByLine(string str_info, StreamWriter sw)
        {
            sw.WriteLine(str_info);
        }

        void EndExportHandler(bool isAutoEnd, int frameIndex)
        {
            FinishFile(sw_2D);
            FinishFile(sw_3D);

            EndExportEvent?.Invoke(isAutoEnd, frameIndex);
            //UtilExtension.QuitWithLog($"[IO] {gameObject.name} End export at {frameCount}");
        }
    }
}