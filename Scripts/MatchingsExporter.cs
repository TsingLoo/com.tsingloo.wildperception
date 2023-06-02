using System.IO;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;

namespace WildPerception {	
	
	public class MatchingsExporter : MonoBehaviour
	{
		/// <summary>
		/// Automatically managed by MainController, the modification of this in inspector will be override 
		/// </summary>
	    [HideInInspector] public int cameraIndex;
		/// <summary>
		/// Automatically managed by MainController, the modification of this in inspector will be override
		/// </summary>
	    [HideInInspector] public string filePath = @"\matchings";
	    Camera cam;
	    PerceptionCamera pCam;
		CameraManager cameraManager;
		PeopleManager peopleManager;
	
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
	        pCam.id = cameraIndex.ToString();
	        InitTextFile(filePath, ".txt", ref sw_2D);
	        InitTextFile(filePath, "_3D.txt", ref sw_3D);
	        frameIndex = -frameBias;
        }
	
	    void Update()
	    {
	        if (pCam.SensorHandle.ShouldCaptureThisFrame)
	        {
	            ExportThisFrameHandler();
	        }
	    }
	
	
	    private void OnDisable()
	    {
	        EndExportHandler();
	    }
	
	    public void SetMatchingsExporter(int cameraIndex, MainController controller)
	    { 
	        this.cameraIndex = cameraIndex;
	        //Obj.name = "Camera" + (Obj.GetOrAddComponent<MatchingsExporter>().cameraIndex).ToString();
	        this.filePath = controller.matchings;
			peopleManager = controller.peopleManager;
	    }
	
	    void ExportThisFrameHandler()
	    {
	        if (frameIndex >= 0)
	        {
	            Debug.Log($"[{nameof(MatchingsExporter)}][IO] Export this frame : " + frameIndex.ToString());	
	            //Debug.Log("[IO]Export matchings for this frame : " + (Time.frameCount - CameraManager.Instance.BeginFrameCount).ToString());
	
	                foreach (var bound in peopleManager.bounds_list)
	                {
	                    bound.UpdateDataTuple();
	                    WriteFileByLine((frameIndex).ToString() + " " + bound.GetDataTuple_2D(cam), sw_2D);
	                    WriteFileByLine((frameIndex).ToString() + " " + bound.GetDataTuple_3D(cam), sw_3D);
	                }
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
	            Debug.Log("[IO]File " + Path.Combine(path ,file_name) + " created.");
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
	
	    void EndExportHandler()
	    {
	        FinishFile(sw_2D);
	        FinishFile(sw_3D);
	        Debug.Log("[IO] End export");
	    }
	
	}
}
