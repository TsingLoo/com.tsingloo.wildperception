using UnityEngine;

 namespace WildPerception {	
	public class DynSpownCollider : MonoBehaviour
	{
	
	    public GameObject target;
	
	    public Vector3 localScaler = Vector3.one;
	
	    Vector3 pMax = Vector3.zero;
	    Vector3 pMin = Vector3.zero;
	    Vector3 center = Vector3.zero;
	
	    private Vector3 oldPos;
	    private Quaternion oldQua;
	    public void SpownCollider()
	    { 
	        oldPos = target.transform.position;
	        oldQua = target.transform.rotation;
	
	        target.transform.position = Vector3.zero;
	        target.transform.rotation = Quaternion.identity;
	
	        Bounds bounds = ClacBounds(target);
	
	        BoxCollider collider = gameObject.GetOrAddComponent<BoxCollider>();
	        collider.center = bounds.center;
	        collider.size = bounds.size;
	
	        target.transform.position = oldPos;
	        target.transform.rotation = oldQua;
	
	        gameObject.transform.localScale = localScaler;
	    }
	
	    /// <summary>
	    /// ����Ŀ���Χ��
	    /// </summary>
	    /// <param name="obj"></param>
	    /// <returns></returns>
	    private UnityEngine.Bounds ClacBounds(GameObject obj)
	    {
	        Renderer mesh = obj.GetComponent<Renderer>();
	
	        if (mesh != null)
	        {
	            Bounds b = mesh.bounds;
	            pMax = b.max;
	            pMin = b.min;
	            center = b.center;
	        }
	
	
	        RecursionClacBounds(obj.transform);
	
	        ClacCenter(pMax, pMin, out center);
	
	        Vector3 size = new Vector3(pMax.x - pMin.x, pMax.y - pMin.y, pMax.z - pMin.z);
	        Bounds bound = new Bounds(center, size);
	        bound.size = size;
	
	        //print("size>" + size);
	        bound.extents = size / 2f;
	
	        return bound;
	    }
	    /// <summary>
	    /// �����Χ����������
	    /// </summary>
	    /// <param name="max"></param>
	    /// <param name="min"></param>
	    /// <param name="center"></param>
	    private void ClacCenter(Vector3 max, Vector3 min, out Vector3 center)
	    {
	        float xc = (pMax.x + pMin.x) / 2f;
	        float yc = (pMax.y + pMin.y) / 2f;
	        float zc = (pMax.z + pMin.z) / 2f;
	
	        center = new Vector3(xc, yc, zc);
	
	        //print("center>" + center);
	    }
	    /// <summary>
	    /// �����Χ�ж���
	    /// </summary>
	    /// <param name="obj"></param>
	    private void RecursionClacBounds(Transform obj)
	    {
	        if (obj.transform.childCount <= 0)
	        {
	            return;
	        }
	
	        foreach (Transform item in obj)
	        {
	            Renderer m = item.GetComponent<Renderer>();
	
	
	
	            if (m != null)
	            {
	                Bounds b = m.bounds;
	                if (pMax.Equals(Vector3.zero) && pMin.Equals(Vector3.zero))
	                {
	                    pMax = b.max;
	                    pMin = b.min;
	                }
	
	                if (b.max.x > pMax.x)
	                {
	                    pMax.x = b.max.x;
	                }
	
	                if (b.max.y > pMax.y)
	                {
	                    pMax.y = b.max.y;
	                }
	                if (b.max.z > pMax.z)
	                {
	                    pMax.z = b.max.z;
	                }
	                if (b.min.x < pMin.x)
	                {
	                    pMin.x = b.min.x;
	                }
	
	                if (b.min.y < pMin.y)
	                {
	                    pMin.y = b.min.y;
	                }
	                if (b.min.z < pMin.z)
	                {
	                    pMin.z = b.min.z;
	                }
	            }
	            RecursionClacBounds(item);
	        }
	    }
	}
}
