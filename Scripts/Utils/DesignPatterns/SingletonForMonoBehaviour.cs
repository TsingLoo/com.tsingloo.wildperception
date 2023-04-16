
 namespace WildPerception {	//reference: https://blog.csdn.net/KeeeepGO/article/details/88383462?spm=1001.2014.3001.5502
	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
	public class SingletonForMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
	    private static T _instance;
	
	    private static object _lock = new object();
	
	    public static T Instance
	    {
	        get
	        {
	            if (applicationIsQuitting)
	            {
	                return null;
	            }
	
	
	            if (_instance == null)
	            {
	                lock (_lock)
	                    {
	                        _instance = (T)FindObjectOfType(typeof(T));
	
	
	                        if (FindObjectsOfType(typeof(T)).Length > 1)
	                        {
	                            return _instance;
	                        }
	
	                        if (_instance == null)
	                        {
	                            GameObject singleton = new GameObject();
	                            _instance = singleton.AddComponent<T>();
	                            singleton.name = "(singleton) " + typeof(T).ToString();
	
	                            DontDestroyOnLoad(singleton);
	                        }
	                    }
	            }
	            return _instance;
	        }
	    }
	
	    private static bool applicationIsQuitting = false;
	
	    public void OnDestroy()
	    {
	        applicationIsQuitting = true;
	    }
	}
}
