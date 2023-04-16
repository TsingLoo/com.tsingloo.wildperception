using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

 namespace WildPerception {	public static class UtilExtension
	{
	  
	
	
	    public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
	    {
	        /// <summary>
	        /// 扩展字典类中的TryGetValue方法
	        /// 可以直接通过给出key返回value,而不是像原方法一样返回bool值
	        /// </summary>
	        /// <typeparam name="TKey"></typeparam>
	        /// <typeparam name="TValue"></typeparam>
	        /// <param name="dict"></param>
	        /// <param name="key"></param>
	        /// <returns></returns>
	
	        TValue value;
	        dict.TryGetValue(key, out value);
	
	        return value;
	    }
	
	    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
	    {
	        T component = obj.GetComponent<T>();
	        if (component == null)
	        {
	            return obj.AddComponent<T>();
	        }
	
	        return component;
	    }
	
	    public static void SafeSetActive(UnityEngine.Object obj, bool active) 
	    {
	        if (obj != null)
	        {
	            if (obj is GameObject)
	            {
	                ((GameObject)obj).SetActive(active);
	            }
	            else if (obj is Component)
	            {
	                ((GameObject)obj).gameObject.SetActive(active);
	            }
	        }
	    }
	}
}
