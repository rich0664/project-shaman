using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class Serializer {

	public static BinaryFormatter binaryFormatter = new BinaryFormatter();

	public static void SaveObj(String key, object obj){
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, obj);
		string data = System.Convert.ToBase64String(memoryStream.ToArray());
		PlayerPrefs.SetString(key, data);
	}

	public static object LoadObj(string key){
		if(!PlayerPrefs.HasKey(key))
			return null;
		string data = PlayerPrefs.GetString(key);
		if(data == string.Empty)
			return null;
		MemoryStream memoryStream = new MemoryStream(System.Convert.FromBase64String(data));
		return binaryFormatter.Deserialize(memoryStream);
	}



}
