
 namespace WildPerception {	//reference: https://blog.csdn.net/KeeeepGO/article/details/88383462?spm=1001.2014.3001.5502
	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
	public class Singleton<T> where T : new()
	{
	    public static T Instance
	    {
	        get
	        {
	            return Nested.instance;
	        }
	    }
	
	    class Nested
	    {
	        static Nested() { }
	
	        internal static readonly T instance = new T();
	    }
	}
}
