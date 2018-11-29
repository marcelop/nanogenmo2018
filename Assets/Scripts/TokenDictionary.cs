using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Newtonsoft;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

//[CreateAssetMenu (menuName = "Kitfox/New TokenDictionary", order = 50)]
public class TokenDictionary {// : SerializedScriptableObject {

	public string filePath = "Assets/Data/Text/TokenDictionary.txt";
	public string fileContents = "";
	[OnInspectorGUI("ShowImportButtons")]
	public Dictionary <string, List<string>> phrases = new Dictionary<string, List<string>>();

	#if UNITY_EDITOR
	void ShowImportButtons()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Import JSON"))
		{
			ImportJSON();
		}
		if (GUILayout.Button("Export JSON"))
		{
			ExportJSON();
		}
		GUILayout.EndHorizontal();
	}

	public bool ImportJSON()
	{
		Dictionary <string, List<string>> importedPhrases;
		try
		{
			string contents = "";
			if (fileContents == "")
				contents = File.ReadAllText(filePath);
			else
				contents = fileContents;

			//Debug.Log(contents);
			importedPhrases = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary <string, List<string>>>(contents);
		}
		catch(System.Exception e) { Debug.LogError("Failed to import: " + e); return false; }

		if (importedPhrases != null)
		{
			phrases = importedPhrases;
			//EditorUtility.SetDirty(this);
			//AssetDatabase.SaveAssets();
			//Debug.Log("Imported Successfully.");
			return true;
		}

		return false;
	}

	void ExportJSON()
	{
		Debug.Log("Exporting JSON");
		StreamWriter writer = new StreamWriter(filePath);
		writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(phrases, Newtonsoft.Json.Formatting.Indented));
		writer.Close();

		Debug.Log("Exported Successfully.");
	}

	#endif
}
