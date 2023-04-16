using UnityEngine;

 namespace WildPerception {	
	public class GetVertex : MonoBehaviour
	{
	
	    void Start()
	    {
	    }
	
	    Vector3[] GetBoxColliderVertex(BoxCollider b) 
	    {
	        Vector3[] verts = new Vector3[8];
	        verts[0] = b.gameObject.transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f);
	        verts[1] = b.gameObject.transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f);
	        verts[2] = b.gameObject.transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f);
	        verts[3] = b.gameObject.transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f);
	        verts[4] = b.gameObject.transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, -b.size.z) * 0.5f);
	        verts[5] = b.gameObject.transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, -b.size.z) * 0.5f);
	        verts[6] = b.gameObject.transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, b.size.z) * 0.5f);
	        verts[7] = b.gameObject.transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, b.size.z) * 0.5f);
	        return verts;
	    }
	
	    void GetMeshVertex() 
	    {
	        Mesh mesh = GetComponent<MeshFilter>().mesh;
	
	        Vector3[] vertices = mesh.vertices;
	
	        foreach (Vector3 vertex in vertices)
	        {
	            Debug.Log(vertex);
	        }
	    }
	
	}
}
