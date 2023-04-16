using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 namespace WildPerception {	
	public class DoNotDestroyMe : MonoBehaviour
	{
	    private void Awake()
	    {
	        DontDestroyOnLoad(gameObject);
	    }
	}
}
