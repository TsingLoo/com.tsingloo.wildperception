using System.Collections.Generic;
using UnityEngine;

 namespace WildPerception {	
	public class PersonBound : MonoBehaviour
	{
	    public int PID;
	    // [SerializeField] Camera cam;
	    
	    Vector3[] points;
	    Vector2[] points2D;
	
	    public Vector3 gridOriginPos;
	    float scaling;
	
	    //��Ҫ��ȡBoxcollier����Ķ���
	    private BoxCollider cube;
	    string tuple_2D = "";
	    string tuple_3D = "";
	
	    DynSpownCollider dsc;
	    //CameraManager cameraManager;

	
	
	    private void Awake()
	    {
	        dsc = gameObject.AddComponent<DynSpownCollider>();
	        dsc.target = gameObject;
	        dsc.SpownCollider();
	    }
	
	    void Start()
	    {
		    cube = gameObject.GetComponent<BoxCollider>();
	        cube.isTrigger = true;
	        scaling = CalibrateTool.Instance.Scaling;
	        //UpdateDataTuple();
	
	        //GetDataTuple_2D();
	        //GetDataTuple_3D();
	        //GenerateCameraX_3D();
	    }
	
	    public void UpdateDataTuple()
	    {
	        points = GetBoxColliderVertexPositions(cube);
	        //GeneratePointByVector3();
	    }
	
	    
	    public string GetDataTuple_2D_Viewport(Camera cam)
	    {
	        Debug.Log(PID.ToString() + " " +   cam.gameObject.name);
	  
	        points2D = ConvertWorldPointsToViewportPoints(points, cam);
	        tuple_2D = "";
	        AddPropertyToTuple(PID.ToString(),ref tuple_2D);
	        for (int i = 0; i < points.Length; i++)
	        {
	            AddPropertyToTuple((points2D[i].x * (CameraManager.RESOLUTION_WIDTH)).ToString(), ref tuple_2D);
	            AddPropertyToTuple(((1 - points2D[i].y) * (CameraManager.RESOLUTION_HEIGHT)).ToString(), ref tuple_2D);
	        }
	        AddPropertyToTuple(((cam.WorldToViewportPoint(gameObject.transform.position).x * CameraManager.RESOLUTION_WIDTH)).ToString(), ref tuple_2D);
	        AddPropertyToTuple(((1 - cam.WorldToViewportPoint(gameObject.transform.position).y) * CameraManager.RESOLUTION_HEIGHT).ToString(), ref tuple_2D);
	        return tuple_2D;
	    }
	
	    public string GetDataTuple_2D(Camera cam)
	    {
	        if (cam == null) return "";
	
	        //Debug.Log(PID.ToString() + " " + cam.gameObject.name);
	
	        points2D = ConvertWorldPointsToViewportPoints(points, cam);
	        tuple_2D = "";
	        AddPropertyToTuple(PID.ToString(), ref tuple_2D);
	        for (int i = 0; i < points.Length; i++)
	        {
	            AddPropertyToTuple((points2D[i].x ).ToString(), ref tuple_2D);
	            AddPropertyToTuple(((points2D[i].y)).ToString(), ref tuple_2D);
	        }
	        AddPropertyToTuple(((cam.WorldToScreenPoint(gameObject.transform.position).x)).ToString(), ref tuple_2D);
	        AddPropertyToTuple(((CameraManager.RESOLUTION_HEIGHT - cam.WorldToScreenPoint(gameObject.transform.position).y) ).ToString(), ref tuple_2D);
	        return tuple_2D;
	    }
	
	    public string GetDataTuple_3D(Camera cam)
	    {
	        tuple_3D = "";
	        AddPropertyToTuple(PID.ToString(), ref tuple_3D);
	
	        //float Ybias = points[0].y;
	        float Ybias = 0;
	

	        for (int i = 0; i < points.Length; i++)
	        {
	            //points[i] = points[i] / scaling;
	
	            //Debug.Log("[IO]This point is " + points[i].ToString());
	            AddPropertyToTuple(((points[i].x - gridOriginPos.x) / scaling).ToString(), ref tuple_3D);
	            //Debug.Log("[IO]This point is " + (points[i].x).ToString());
	            //AddPropertyToTuple((points[i].y - Ybias).ToString(), ref tuple_3D);
	            AddPropertyToTuple(((points[i].z  - gridOriginPos.z) / scaling).ToString(), ref tuple_3D);
	            AddPropertyToTuple(((points[i].y - Ybias - gridOriginPos.y) / scaling).ToString(), ref tuple_3D);
	            //Height 
	            //AddPropertyToTuple(transform.position.y.ToString());
	        }
	
	        //transform.position = transform.position / scaling;

	        AddPropertyToTuple(((transform.position.x  - gridOriginPos.x) / scaling).ToString(), ref tuple_3D);
	        AddPropertyToTuple(((transform.position.z  - gridOriginPos.z) / scaling).ToString(), ref tuple_3D);
	        AddPropertyToTuple(((transform.position.y  - Ybias - gridOriginPos.y) / scaling).ToString(), ref tuple_3D);
	        return tuple_3D;
	    }
	
	
	    void AddPropertyToTuple(string property, ref string tuple) 
	    {
	        tuple = tuple + property + " ";
	    }
	
	    static Vector3[] GetBoxColliderVertexPositions(BoxCollider boxcollider)
	    {
	        var vertices = new Vector3[8];
	        //����4����
	        vertices[0] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, -boxcollider.size.y, boxcollider.size.z) * 0.5f);
	        vertices[1] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, -boxcollider.size.y, boxcollider.size.z) * 0.5f);
	        vertices[2] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, -boxcollider.size.y, -boxcollider.size.z) * 0.5f);
	        vertices[3] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, -boxcollider.size.y, -boxcollider.size.z) * 0.5f);
	        //����4����
	        vertices[4] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, boxcollider.size.y, boxcollider.size.z) * 0.5f);
	        vertices[5] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, boxcollider.size.y, boxcollider.size.z) * 0.5f);
	        vertices[6] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, boxcollider.size.y, -boxcollider.size.z) * 0.5f);
	        vertices[7] = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, boxcollider.size.y, -boxcollider.size.z) * 0.5f);
	
	        return vertices;
	    }
	
	    static Vector2[] ConvertWorldPointsToViewportPoints(Vector3[] worldPoints, Camera cam)
	    {
	        var points2D = new Vector2[worldPoints.Length];
	        for (int i = 0; i < worldPoints.Length; i++)
	        {
	
	            points2D[i] = cam.WorldToViewportPoint(worldPoints[i]);
	            //Debug.Log(cam.WorldToViewportPoint(worldPoints[i]));
	            //Debug.Log(points2D[i]);
	        }
	        return points2D;
	
	    }
	}
}
