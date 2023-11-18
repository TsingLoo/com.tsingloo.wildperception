/********************************************************************************
	
	 ** auth： Xiaonan Pan
	
	 ** date： 2023/03/22
	
	 ** desc： Refer to http://www.tsingloo.com/2023/03/01/0a2bf39019914a06954a4506b9f0ca37/ for more information
	
	 ** Ver.:  V0.0.4
	
	*********************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEditor;
using UnityEngine;
//using Unity.Mathematics;

namespace WildPerception
{
    public class CalibrateTool : MonoBehaviour
    {
        static CalibrateTool instance;
        public static CalibrateTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(CalibrateTool)) as CalibrateTool;
                }
	
                return instance;
            }
        }
        [Header("Could Skip")]
        /// <summary>
        /// Add the cameras you want to calibrate into this list
        /// </summary>
        [Tooltip("Skip this if you are using FYP_UnityProject. This is managed by CameraManager")]
        [HideInInspector]   public List<Camera> camerasToCalibrate;
	
        [Tooltip("Skip this if you are using FYP_UnityProject. This is managed by MainController")]
        /// <summary>
        /// Define the targetParentFolder to save the data;
        /// </summary>
        [HideInInspector] public string targetParentFolder = @"../CalibrateTool/";
	
	
        #region Inspector
        [Header("Main Properties")]
	
        /// <summary>
        /// Fake chessboard will be generated from this transform
        /// </summary>
        [HideInInspector] public Transform chessboardGenerateCenter;
	    
        /// <summary>
        /// Scaling of OpenCV coordinate
        /// </summary>
        [Range(0, 10f)]
        public float Scaling = 1;
	
        //[SerializeField] bool enableLog = false;
	
        [Header("Chessboard Properties")]
        /// <summary>
        /// The inteval bettween the chessboard to change its transform
        /// </summary>
        [SerializeField] float updateChessboardInterval = 0.2f;
	
        /// <summary>
        /// Count how many times(views) is the chessboard going to be updated
        /// </summary>
        [SerializeField] int chessboardCount = 30;
	
        /// <summary>
        /// The length of the sides of the fake checkerboard
        /// </summary>
        [SerializeField] float SQUARE_SIZE = 0.14f;
	
        /// <summary>
        /// How many squares in width of the chessboard, better be odd
        /// </summary>
        [SerializeField] int widthOfChessboard = 9;
	
        /// <summary>
        /// How many squares in height of the chessboard, better be odd
        /// </summary>
        [SerializeField] int heightOfChessboard = 7;
	
        [Header("Staic Mark Points Properties")]
        /// <summary>
        /// The square Ulength of mark points square
        /// </summary>
        [SerializeField] int UlengthOfMarkPointsSquare = 7;
	
        /// <summary>
        /// The Udistance bettween two mark points
        /// </summary>
        [SerializeField] float UgapBetweenMarkPoints = 1;
	
        [SerializeField] GameObject humanModel;
        [SerializeField] bool showModel = false;
	
        GameObject go;
	
        [Header("Dataset Parameters CV")]
        /// <summary>
        /// This transform indicate the Origin of grid of Wildtrack system.
        /// </summary>
        [HideInInspector] public Transform gridOrigin;
        public int MAP_HEIGHT = 16;
        public int MAP_WIDTH = 25;
        public int MAP_EXPAND = 40;
        public float MAN_RADIUS = 0.16f;
        public float MAN_HEIGHT = 1.8f;
        public int IMAGE_WIDTH = 1920;
        public int IMAGE_HEIGHT = 1080;
        [HideInInspector]
        public string PERCEPTION_PATH = @"f'perception'";
	
        //format the file name of frames, 0001.png for RW = 4 , 00001.png for RW = 5
        public int RJUST_WIDTH = 4;
	
        [Header("Others")]
        /// <summary>
        /// Random offsets for updating the chessboard 
        /// </summary>
        [SerializeField] float tRandomOffset = 5f;
        [SerializeField] float rRandomOffset = 120;
	
        [Space(12)]
        [Header("Gizmos")]
        [SerializeField] bool popWarning = true;
        [SerializeField] bool drawChessboard = true;
        [SerializeField] bool drawGrid = true;
        [SerializeField] bool drawScaling = true;
        [SerializeField] bool drawTRandom = true;
        [SerializeField] bool drawMarkPoints = true;
        #endregion
	
        /// <summary>
        /// A dictionary to store the cornerpoints(GameObject) on chessboard and their coordinates(follow OpenCV,(x,y,0)) of the chessboard
        /// </summary>
        Dictionary<GameObject, Vector3> cornerPointsDic = new Dictionary<GameObject, Vector3>();
	
        /// <summary>
        /// An array to store the static mark points 
        /// </summary>
        Vector3[] markPoints;
	
        Vector3[] validatePoints;
	
        /// <summary>
        /// Index of chessboard
        /// </summary>
        int boardIndex = 0;
	
        //bool hasWrittenCalib = false;
        List<List<Vector2[]>> pointsLists2d;
        List<List<Vector3[]>> pointsLists3d;
	
	
	
        void Start()
        {
            chessboardGenerateCenter.rotation = Quaternion.identity;
	
            GenerateMarkPoints();
            GenerateValidatePoints();
            GenerateVisualChessboard();
	
            WriteDatasetParametersPy(); 
            //WriteGridOrigin();
            targetParentFolder = Path.Combine(targetParentFolder,"calib");
            Debug.Log("[CalibrateTool][IO] Calibrate saved in " + targetParentFolder);
            ResetFolder(targetParentFolder);
	
            pointsLists2d = new List<List<Vector2[]>>(camerasToCalibrate.Count);
            pointsLists3d = new List<List<Vector3[]>>(camerasToCalibrate.Count);
            //Debug.Log($"[CalibrateTool][IO]{nameof(pointsLists2d)} is {pointsLists2d.Count}");
	
            BeginToCalibrate();
        }
	
        //void OnDisable()
        //{
        //    if (hasWrittenCalib) return;
        //    WriteCalib();
        //}
	
	
        void BeginToCalibrate()
        {
            if (camerasToCalibrate.Count < 1)
            {
                Debug.LogWarning("[CalibrateTool] No camera to calibrate, please check " + nameof(CalibrateTool) + "." + nameof(camerasToCalibrate));
                return;
            }
	
            int cameraIndex = 0;
            foreach (var cam in camerasToCalibrate)
            {
                //Debug.Log($"[CalibrateTool]Config Image size({IMAGE_WIDTH},{IMAGE_HEIGHT}) is not the same with Game View of Camera {cam.name}, target display {cam.targetDisplay}, size({cam.pixelWidth},{cam.pixelHeight})");
		        
                // if (cam.targetDisplay != 1)
                //    {
                //     RenderTexture renderTexture = new RenderTexture(IMAGE_WIDTH, IMAGE_HEIGHT, 24);
                //     cam.targetTexture = renderTexture;
                //    }
                //    else
                //    {
                //     if (cam.pixelHeight != IMAGE_HEIGHT || cam.pixelWidth != IMAGE_WIDTH)
                //     {
                //      UtilExtension.QuitWithLogError($"[CalibrateTool]Config Image size({IMAGE_WIDTH},{IMAGE_HEIGHT}) is not the same with Game View of Camera {cam.name}, target display {cam.targetDisplay}, size({cam.pixelWidth},{cam.pixelHeight})");
                //     }       
                //    }
		        
                if (cam.pixelHeight != IMAGE_HEIGHT || cam.pixelWidth != IMAGE_WIDTH)
                {
                    UtilExtension.QuitWithLogError($"[CalibrateTool] Config Image size({IMAGE_WIDTH},{IMAGE_HEIGHT}) is not the same with Game View of Camera {cam.name}, target display {cam.targetDisplay}, size({cam.pixelWidth},{cam.pixelHeight})");
                }      
		        

                WriteWorldPointsScreenPos(cam, cameraIndex, markPoints, nameof(markPoints));
                WriteWorldPointsScreenPos(cam, cameraIndex,validatePoints,nameof(validatePoints));
                pointsLists2d.Add(new List<Vector2[]>());
                pointsLists3d.Add(new List<Vector3[]>());
                //GetNativeCalibrationByMath(cam);
                cameraIndex++;
            }
	
            FlashCalibrate();
            //StartCoroutine(Func());
        }
	
        void FlashCalibrate()
        {
            while (boardIndex <= chessboardCount)// or for(i;i;i)
            {
                int cameraIndex = 0;
                foreach (var cam in camerasToCalibrate)
                {
	
                    List<Vector2> thisCamera2D = new List<Vector2>();
                    List<Vector3> thisCamera3D = new List<Vector3>();
	
                    AddChessboardScreenPosToDic(cam, cameraIndex, ref thisCamera2D, ref thisCamera3D);
	
                    if (thisCamera2D.Count != 0)
                    {
                        pointsLists2d[cameraIndex].Add(thisCamera2D.ToArray());
                        pointsLists3d[cameraIndex].Add(thisCamera3D.ToArray());
                    }
                    cameraIndex++;
                }
                UpdateChessboard();
            }
            Debug.Log("[CalibrateTool] UpdateChessboard STOP");
            //hasWrittenCalib = true;
            WriteCalib();
        }
	
        [Obsolete]
        IEnumerator Func()
        {
	
            while (true)// or for(i;i;i)
            {
                yield return new WaitForSeconds(updateChessboardInterval); // first
                //Specific functions put here 
	
                int cameraIndex = 0;
                foreach (var cam in camerasToCalibrate)
                {
	
                    List<Vector2> thisCamera2D = new List<Vector2>();
                    List<Vector3> thisCamera3D = new List<Vector3>();
	
                    AddChessboardScreenPosToDic(cam, cameraIndex, ref thisCamera2D, ref thisCamera3D);
	
                    if (thisCamera2D.Count != 0)
                    {
                        pointsLists2d[cameraIndex].Add(thisCamera2D.ToArray());
                        pointsLists3d[cameraIndex].Add(thisCamera3D.ToArray());
                    }
                    cameraIndex++;
                }
                UpdateChessboard();
                // Note the order of codes above.  Different order shows different outcome.
                if (boardIndex >= chessboardCount)
                {
                    Debug.Log("[CalibrateTool] UpdateChessboard STOP");
                    //hasWrittenCalib = true;
                    WriteCalib();
                    break;
                }
            }
        }
	
        void GetNativeCalibrationByMath(Camera cam)
        {
            float w = cam.pixelWidth;
            float h = cam.pixelHeight;
            float fov = cam.fieldOfView;
	
            float u_0 = w / 2;
            float v_0 = h / 2;
	
            float fx = (h / 2) / (float)System.Math.Tan((fov / 180 * System.Math.PI) / 2);
	
            //float fx = w / (2 * (math.tan((fov / 2) * (math.PI / 180))));
            float fy = h / (2 * (float)(Math.Tan((fov / 2) * (Math.PI / 180))));
	
	
            //float3x3 camIntriMatrix = new float3x3(new float3(fx, 0f, u_0),
            //                               new float3(0f, fy, v_0),
            //                               new float3(0f, 0f, 1f));
	
            //Debug.Log(camIntriMatrix);
            //Matrix4x4 cameraToWorldMatrix = cam.cameraToWorldMatrix;
            //Matrix4x4 projectionMatrix = cam.projectionMatrix;
	
            ////Debug.Log(cam.worldToCameraMatrix);
            //Debug.Log("[Calib][OpenCV-RightHandedness] " + cam.transform.name + " Rotation Matrix:");
            //Debug.Log(ConvertUnityWorldToCameraRotationToOpenCV(cam.worldToCameraMatrix));
            //Debug.Log(cam.projectionMatrix);
        }
	
	
        /// <summary>
        /// This method indicate the coordinates of the project
        /// </summary>
        ///[ExecuteAlways]
        void GenerateMarkPoints()
        {
            markPoints = new Vector3[(2 * UlengthOfMarkPointsSquare) * (2 * UlengthOfMarkPointsSquare)];
            int index = 0;
	
            for (int i = -UlengthOfMarkPointsSquare; i < UlengthOfMarkPointsSquare; i++)
            {
                for (int j = -UlengthOfMarkPointsSquare; j < UlengthOfMarkPointsSquare; j++)
                {
                    //Debug.Log(index);
                    markPoints[index] = chessboardGenerateCenter.transform.position + new Vector3(i * UgapBetweenMarkPoints, 0, j * UgapBetweenMarkPoints);
                    index++;
                }
            }
        }
	
	
        void GenerateValidatePoints() 
        {
            System.Random ran = new System.Random();
            validatePoints = new Vector3[markPoints.Length];
            for (int i = 0; i < validatePoints.Length; i++)
            {
                validatePoints[i] = new Vector3(markPoints[i].x + NextFloat(ran, -tRandomOffset*0.1f, tRandomOffset*0.1f), markPoints[i].y, markPoints[i].z + NextFloat(ran, -tRandomOffset * 0.1f, tRandomOffset * 0.1f));
            }
        }
	
        /// <summary>
        /// Generate the virtual chessboard
        /// </summary>
        void GenerateVisualChessboard()
        {
            gameObject.transform.position = chessboardGenerateCenter.transform.position;
            gameObject.transform.rotation = chessboardGenerateCenter.transform.rotation;
	
            for (int w = 0; w < widthOfChessboard; w++)
            {
                for (int h = 0; h < heightOfChessboard; h++)
                {
                    GameObject cornerPoint = new GameObject(w.ToString() + "_" + h.ToString());
                    cornerPoint.transform.position = chessboardGenerateCenter.position;
                    //Debug.Log($"[CalibrateTool][Chessboard]{nameof(cornerPoint)} 1: {cornerPoint.transform.position}");
                    cornerPoint.transform.SetParent(transform);
                    //Debug.Log($"[CalibrateTool][Chessboard]{nameof(cornerPoint)} 2: {cornerPoint.transform.localPosition}");
                    cornerPoint.transform.localPosition = new Vector3(w * SQUARE_SIZE, 0, h * SQUARE_SIZE) ;
                    //Debug.Log($"[CalibrateTool][Chessboard]{nameof(cornerPoint)} 3: {cornerPoint.transform.localPosition}");
                    cornerPointsDic.Add(cornerPoint, new Vector3(w * SQUARE_SIZE/Scaling, h * SQUARE_SIZE/Scaling, 0));
                }
            }
        }
	
        void UpdateChessboard()
        {
            boardIndex++;
            System.Random ran = new System.Random();
            transform.position = new Vector3(NextFloat(ran, tRandomOffset, -tRandomOffset), NextFloat(ran, tRandomOffset, -tRandomOffset), NextFloat(ran, tRandomOffset, -tRandomOffset)) + chessboardGenerateCenter.position;
            transform.rotation = Quaternion.Euler(NextFloat(ran, rRandomOffset, -rRandomOffset), NextFloat(ran, rRandomOffset, -rRandomOffset), NextFloat(ran, rRandomOffset, -rRandomOffset));
        }
	
        void WriteDatasetParametersPy() 
        {
            string pyPath = Path.Combine(targetParentFolder, "datasetParameters.py");
	
            //Debug.Log(pyPath);
	
            if (File.Exists(pyPath))
            {
                File.Delete(pyPath);
            }
	
            //FileInfo fileinfo = new FileInfo(pyPath);
            StreamWriter pysw = new StreamWriter(pyPath);
	
            pysw.WriteLine("GRID_ORIGIN = [" + gridOrigin.position.x.ToString() + "," + gridOrigin.position.y.ToString() + "," + gridOrigin.position.z.ToString() + "]");
            pysw.WriteLine("NUM_CAM = " + camerasToCalibrate.Count().ToString());
            pysw.WriteLine("CHESSBOARD_COUNT = " + chessboardCount.ToString());
            pysw.WriteLine(nameof(MAP_WIDTH) + " = " + MAP_WIDTH.ToString());
            pysw.WriteLine(nameof(MAP_HEIGHT) + " = " +  MAP_HEIGHT.ToString());
            pysw.WriteLine(nameof(MAP_EXPAND) + " = " + MAP_EXPAND.ToString());
            pysw.WriteLine(nameof(IMAGE_WIDTH) + " = " + IMAGE_WIDTH.ToString());
            pysw.WriteLine(nameof(IMAGE_HEIGHT) + " = " + IMAGE_HEIGHT.ToString());
            pysw.WriteLine(nameof(MAN_HEIGHT) + " = " + MAN_HEIGHT.ToString());
            pysw.WriteLine(nameof(MAN_RADIUS) + " = " + MAN_RADIUS.ToString());
            pysw.WriteLine(nameof(RJUST_WIDTH) + " = " + RJUST_WIDTH.ToString());
            pysw.WriteLine(nameof(Scaling) + " = " + Scaling.ToString()); 
            pysw.WriteLine(@"NUM_FRAMES = 0");
            pysw.WriteLine(@"DATASET_NAME = ''");
            pysw.WriteLine(@"");
            pysw.WriteLine(@"# If you are using perception package: this should NOT be 'perception', output path of perception instead");
            if (!PERCEPTION_PATH.Equals(@"f'perception'"))
            { 
                PERCEPTION_PATH = $"'{PERCEPTION_PATH}'";
            } 
            pysw.WriteLine($"{nameof(PERCEPTION_PATH)} = {PERCEPTION_PATH}");
            pysw.WriteLine(@"");
            pysw.WriteLine(@"# The following is for -view configure only:");
            pysw.WriteLine(@"");
            pysw.WriteLine(@"# Define how to convert your unit length to meter, if you are using cm, then 0.01");
            pysw.WriteLine(@"OverlapUnitConvert = 1.");
            pysw.WriteLine(@"# Define how to translate cams to make the world origin and the grid origin is the same");
            pysw.WriteLine(@"OverlapGridOffset = (0., 0., 0.)");
            pysw.Close();
        }
	
        void WriteWorldPointsScreenPos(Camera cam, int cameraIndex, Vector3[] arr, string name)
        {
            //Debug.Log($"[Calib]{name} length {arr.Length}");
            Vector2[] screenPoints = ConvertWorldPointsToScreenPointsOpenCV(arr, cam);
            List<Vector3> worldPointsList = arr.ToList(); // Convert arr1 to a List<Vector3>
            List<Vector2> screenPointsList = screenPoints.ToList(); // Convert arr2 to a List<Vector3>
	
            Rect screenBounds = new Rect(0, 0, Screen.width, Screen.height);
	
            for (int i = screenPointsList.Count - 1; i >= 0; i--)
            {
                if (!screenBounds.Contains(screenPoints[i]))
                {
                    //Debug.Log("[Calib] Delete this : " + screenPoints[i]);
                    worldPointsList.RemoveAt(i);
                    screenPointsList.RemoveAt(i);
                }
            }
	
	
	
            for (int i = 0; i < worldPointsList.Count; i++)
            {
                worldPointsList[i] = worldPointsList[i] - gridOrigin.position;
                worldPointsList[i] = new Vector3(worldPointsList[i].x, worldPointsList[i].z, worldPointsList[i].y);
                worldPointsList[i] = worldPointsList[i] / Scaling;
            }
	
	
            Vector3[] worldPoints = worldPointsList.ToArray(); // Convert the List<Vector3> back to an array
            screenPoints = screenPointsList.ToArray();
	
            //for (int i = 0; i < worldPoints.Length; i++)
            //{
            //    float temp = worldPoints[i].y;
            //    worldPoints[i].y = worldPoints[i].z;
            //    worldPoints[i].z = temp + 5;
            //    worldPoints[i].x = worldPoints[i].x + 5;
            //}
	
            string file_name_3d = name + "_3d.txt";
            string file_name = name + ".txt";
	
            StreamWriter sw_3d = CreateSW(cameraIndex, file_name_3d);
            StreamWriter sw = CreateSW(cameraIndex, file_name);
	
            WriteArrayToFile(worldPoints, ref sw_3d);
            WriteArrayToFile(screenPoints, ref sw);
	
            sw_3d.Close();
            sw.Close();
        }
	
        void AddChessboardScreenPosToDic(Camera cam, int cameraIndex, ref List<Vector2> pointsList2d, ref List<Vector3> pointsList3d)
        {
            List<Vector2> screenPointsList = new List<Vector2>();
            List<Vector3> outPutObjList = new List<Vector3>();
	
            Rect screenBounds = new Rect(0, 0, Screen.width, Screen.height);
	
            foreach (var go in cornerPointsDic)
            {
                Vector2 viewPos = cam.WorldToScreenPoint(go.Key.transform.position);
                if (screenBounds.Contains(viewPos))
                {
                    screenPointsList.Add(new Vector2(viewPos.x, Screen.height - viewPos.y));
                    outPutObjList.Add(go.Value);
                }
            }
	
            pointsList2d = screenPointsList;
            pointsList3d = outPutObjList;
	
            //string file_name = boardIndex + ".txt";
            //string file_name_3d = boardIndex + "_3d.txt";
	
            //StreamWriter sw = CreateSW(cameraIndex, file_name);
            //StreamWriter sw_3d = CreateSW(cameraIndex, file_name_3d);
            //WriteArrayToFile(imagePoints, ref sw);
            //WriteArrayToFile(outPutObjList.ToArray(), ref sw_3d);
	
            //sw.Close();
            //sw_3d.Close();
        }
        async void WriteCalib() 
        {
            //hasWrittenCalib= true;
            Debug.Log("[CalibrateTool] Begin to write calibration");
	
            //针对每个相机
            for (int i = 0; i < camerasToCalibrate.Count; i++)
            {
                //针对每个棋盘
                for (int j = 0; j < pointsLists2d[i].Count; j++)
                {
                    //开线程写
                    await Task.Run(() =>
                    {
                        string fileName2d = (j).ToString() + ".txt"; // Generate the file name (1.txt, 2.txt, etc.)
                        string filePath = Path.Combine(targetParentFolder, $"C{i + 1}");
                        string path = Path.Combine(filePath, fileName2d); // Combines the filename with the data path of your Unity project
                        using (StreamWriter writer2d = new StreamWriter(path))
                        {
                            //Debug.Log($"[CalibrateTool][Async][IO]{path}");
                            for (int k = 0; k < pointsLists2d[i][j].Length; k++)
                            {
                                var data = $"{pointsLists2d[i][j][k].x} {pointsLists2d[i][j][k].y}"; // Write the data to the file asynchronously
                                writer2d.WriteLine(data); // Write the data to the file asynchronously
                            }
                        }
	
                        string fileName3d = (j).ToString() + "_3d.txt"; // Generate the file name (1.txt, 2.txt, etc.)
                        string path3d = Path.Combine(filePath, fileName3d); // Combines the filename with the data path of your Unity project
                        using (StreamWriter writer3d = new StreamWriter(path3d))
                        {
                            //Debug.Log($"[CalibrateTool][Async][IO]{path3d}");
                            //Debug.Log($"[CalibrateTool]There are {pointsLists3d[i][j].Length} chessboards");
                            for (int k = 0; k < pointsLists3d[i][j].Length; k++)
                            {
                                var data = $"{pointsLists3d[i][j][k].x} {pointsLists3d[i][j][k].y} {pointsLists3d[i][j][k].z}"; // Write the data to the file asynchronously
                                //Debug.Log(data);
                                writer3d.WriteLine(data); // Write the data to the file asynchronously
                            }
                        }
                    });
                }
            }
            Debug.Log($"[{nameof(CalibrateTool)}] Calibration Finished");
        }
	
        void WriteToFile(string filePath, string fileName, string data)
        {
            string path = Path.Combine(filePath, fileName); // Combines the filename with the data path of your Unity project
            //Debug.Log($"[CalibrateTool][Async][IO]{path}");
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(data); // Write the data to the file asynchronously
            }
        }
	
        public void ResetFolder(string foldername)
        {
            DirectoryInfo dir = new DirectoryInfo(foldername);
	
            if (dir.Exists)
            {
                dir.Delete(true);
                Debug.Log($"$[CalibrateTool][IO] {foldername} have been reset");
            }
	
        }
	
        StreamWriter CreateSW(int cameraIdx, string filename)
        {
            StreamWriter sw;
            cameraIdx++;
	
            Directory.CreateDirectory(targetParentFolder + "/C" + cameraIdx.ToString() + "/");
            FileInfo fileInfo = new FileInfo(targetParentFolder + "/C" + cameraIdx.ToString() + "/" + filename);
            if (!fileInfo.Exists)
            {
                sw = fileInfo.CreateText();
                //if (enableLog)
                //{
                //    Debug.Log("[CalibrateTool][IO]File " + filename + " has been inited");
                //}
	
                return sw;
            }
            else
            {
                sw = fileInfo.AppendText();
                return sw;
            }
        }
	
	
        public void WriteArrayToFile(Vector2[] array, ref StreamWriter sw)
        {
            foreach (Vector3 element in array)
            {
                sw.WriteLine(element.x + " " + element.y);
            }
        }
	
        public void WriteArrayToFile(Vector3[] array, ref StreamWriter sw)
        {
            foreach (Vector3 element in array)
            {
                sw.WriteLine(element.x + " " + element.y + " " + element.z);
            }
        }
	
        public float NextFloat(System.Random ran, float minValue, float maxValue)
        {
            return (float)(ran.NextDouble() * (maxValue - minValue) + minValue);
        }
	
        public Vector2[] ConvertWorldPointsToScreenPointsOpenCV(Vector3[] worldPoints, Camera cam)
        {
            var points2D = new Vector2[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
	
                points2D[i] = cam.WorldToScreenPoint(worldPoints[i]);
                points2D[i].y = cam.pixelHeight - points2D[i].y;
                //Debug.Log(points2D[i]);
                //Debug.Log(points2D[i]);
            }
            return points2D;
	
        }
	
        public Matrix4x4 ConvertUnityWorldToCameraRotationToOpenCV(Matrix4x4 worldToCameraMatrix)
        {
            // Convert the right-handed coordinate system to left-handed
            // by negating the z-axis
            worldToCameraMatrix.SetColumn(2, -worldToCameraMatrix.GetColumn(2));
	
            // Convert the left-handed rotation matrix to a right-handed
            // rotation matrix by reversing the order of the columns
            worldToCameraMatrix = new Matrix4x4(
                worldToCameraMatrix.GetColumn(0),
                worldToCameraMatrix.GetColumn(2),
                worldToCameraMatrix.GetColumn(1),
                new Vector4(0, 0, 0, 1)
            );
	
            return worldToCameraMatrix;
        }
	
        #region Gizmos
        private void OnDrawGizmos()
        {
            MainController m = GetComponent<MainController>();
            if (m.Center_HumanSpawn != null && m.GridOrigin_OpenCV != null)
            {
                chessboardGenerateCenter = m.Center_HumanSpawn.transform;
                gridOrigin = m.GridOrigin_OpenCV.transform;

                if (drawChessboard)
                {
                    DrawChessboard();
                }
                if (drawGrid)
                {
                    DrawGrid(gridOrigin);
                }
                if (drawScaling)
                {
                    DrawScaling(gridOrigin);
                }
                if (drawTRandom)
                {
                    DrawTRandom();
                }
                if (drawMarkPoints)
                {
                    DrawMarkPoints();
                }
            }
            else if (popWarning)
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.LogWarning($"[{nameof(CalibrateTool)}] Go to SceneController => {nameof(MainController)} to init scene");
                    popWarning = false;
                }
            }
        }
	
        void DrawChessboard() 
        {
            foreach (var go in cornerPointsDic)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawCube(go.Key.transform.position , new Vector3(0.01f, 0.01f, 0.01f));
                Handles.Label(go.Key.transform.position , go.Value.ToString());
            }
        }
	
        void DrawGrid(Transform gridOrigin)
        {
            Gizmos.color = new Color(0, 0, 1, 1f);
            //Gizmos.DrawLine(gridOrigin.transform.position * Scaling, gridOrigin.transform.position * Scaling + MAP_HEIGHT * Scaling * Vector3.forward);
            Handles.Label(gridOrigin.transform.position + 3 * Vector3.forward, "Grid Height+");
            for (int i = 0; i < MAP_WIDTH + 1; i++)
            {
                Gizmos.DrawLine(gridOrigin.transform.position + new Vector3(i, 0, 0) * Scaling, gridOrigin.transform.position + MAP_HEIGHT * Scaling * Vector3.forward + new Vector3(i, 0, 0) * Scaling);
                Handles.Label(gridOrigin.transform.position + new Vector3(i, 0, 0) * Scaling, i.ToString());
                //for (int j = 1; j < MAP_EXPAND; j++)
                //{
                //    Gizmos.DrawLine(gridOrigin.transform.position + new Vector3(i + j * (1f / MAP_EXPAND), 0, 0), gridOrigin.transform.position + MAP_HEIGHT * Vector3.forward + new Vector3(i + j * (1f / MAP_EXPAND), 0, 0));
                //}
            }
	
            Gizmos.color = new Color(1, 0, 1, 1f);
            //Gizmos.DrawLine(gridOrigin.transform.position, gridOrigin.transform.position + MAP_WIDTH * Vector3.right);
            Handles.Label(gridOrigin.transform.position + 3 * Vector3.right, "Grid Width+");
            for (int i = 0; i < MAP_HEIGHT + 1; i++)
            {
                Gizmos.DrawLine(gridOrigin.transform.position + new Vector3(0,0,i) * Scaling, gridOrigin.transform.position + MAP_WIDTH * Scaling * Vector3.right + new Vector3(0, 0, i) * Scaling);
                Handles.Label(gridOrigin.transform.position + new Vector3(0, 0, i) * Scaling, i.ToString());
                //for (int j = 1; j < MAP_EXPAND; j++)
                //{
                //    Gizmos.DrawLine(gridOrigin.transform.position + new Vector3(0, 0, i + j * (1f / MAP_EXPAND)), gridOrigin.transform.position + MAP_WIDTH * Vector3.right + new Vector3(0, 0, i + j * (1f / MAP_EXPAND)));
                //}
            }
        }
	
        void DrawScaling(Transform gridOrigin) 
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
	
            Vector3 rootPos = gridOrigin.transform.position + new Vector3(MAP_WIDTH * 0.5f, MAN_HEIGHT*0.5f, MAP_HEIGHT * 0.5f) * Scaling;
	
            Gizmos.DrawCube(rootPos, new Vector3(4*MAN_RADIUS,MAN_HEIGHT,4*MAN_RADIUS) * Scaling);
            Gizmos.DrawWireCube(rootPos, new Vector3(4 * MAN_RADIUS, MAN_HEIGHT, 4 * MAN_RADIUS) * Scaling);
            Handles.Label(rootPos + Vector3.up * MAN_HEIGHT * Scaling * 0.55f, MAN_HEIGHT.ToString());
	
            if (!showModel)
            {
                if (go != null) 
                {
                    go.SetActive(false);
                }
	
                GameObject[] modelgo = GameObject.FindGameObjectsWithTag("EditorOnly");
                foreach (var mgo in modelgo)
                { 
                    mgo.SetActive(false);
                    Destroy(mgo);
                }
                return;
            }
            else
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
	
            }
	
            if (humanModel != null && go == null)
            {
                go = Instantiate(humanModel, rootPos - new Vector3(0,MAN_HEIGHT*0.5f,0)  *Scaling, Quaternion.identity);
                go.transform.localScale = Vector3.one * Scaling ;
                go.name = "Scaling Model";
                go.tag = "EditorOnly";
            }
	        
	
        }
	
        void DrawMarkPoints() 
        {
            if (markPoints == null) return;
	
            foreach (var go in markPoints)
            {
                //Debug.Log(go);
                Gizmos.color = new Color(0, 1, 0, 0.4f);
                Gizmos.DrawCube(go, new Vector3(0.01f, 0.01f, 0.01f));
                //Gizmos.DrawWireCube(go * Scaling, new Vector3(0.01f, 0.01f, 0.01f));
                Handles.Label(go,$"({(go.x - gridOrigin.position.x)/Scaling},{(go.z - gridOrigin.position.z)/Scaling},{(go.y - gridOrigin.position.y)/Scaling})");
                //Handles.Label(go,$"({(go/Scaling).x - gridOrigin.position.x},{(go / Scaling).z - gridOrigin.position.z},{(go / Scaling).y - gridOrigin.position.y})");
            }
	
	
            //foreach (var go in validatePoints)
            //{
            //    //Debug.Log(go);
            //    Gizmos.color = new Color(0, 1, 0, 0.4f);
            //    Gizmos.DrawCube(go * Scaling, new Vector3(0.01f, 0.01f, 0.01f));
            //    Handles.Label(go * Scaling, $"({(go / Scaling).x - gridOrigin.position.x},{(go / Scaling).z - gridOrigin.position.z},{(go / Scaling).y - gridOrigin.position.y})");
            //}
	
        }
	
        void DrawTRandom() 
        {
            Gizmos.color = new Color(1f,0.92f,0.016f,0.2f);
	
            Vector3 center = chessboardGenerateCenter.position;
            Gizmos.DrawCube(center, Vector3.one*tRandomOffset*2);
            Gizmos.DrawWireCube(center, Vector3.one * tRandomOffset * 2);
	
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(gameObject.transform.position, Vector3.one * tRandomOffset * 0.1f);
        }
        #endregion
    }
}