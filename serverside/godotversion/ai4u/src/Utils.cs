using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Utils
{
	
	public static string ListToPythonList<U>(List<U> list) 
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("[");
		var i = 0;
		foreach(U obj in list)
		{
			if (i > 0)
			{
				sb.Append(",");
			}
			sb.Append( obj.ToString() );
			i++;
		}
		return sb.ToString();
	}
	
	public static string DictToPythonDict<T, U>(Dictionary<T, U>  dict)
	{
		StringBuilder r = new StringBuilder();
		r.Append("{");
		int i = 0;
		foreach(KeyValuePair<T, U> p in dict)
		{
			T key = p.Key;
			U dvalue = p.Value;
			if (i > 0) 
			{
				r.Append(";");
			}
			r.Append("\"");
			r.Append(key.ToString());
			r.Append("\":\"");
			r.Append(dvalue.ToString());
			r.Append("\"");
			i++;
		}
		r.Append("}");
		return r.ToString();
	}
}
