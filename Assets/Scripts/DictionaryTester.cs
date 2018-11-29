using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DictionaryTester : MonoBehaviour {

	[OnInspectorGUI("ShowImportButtons")]
	public TextAsset fileToTest;

	#if UNITY_EDITOR
	void ShowImportButtons()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Import JSON"))
		{
			ImportJSON();
		}
		/*if (GUILayout.Button("Export JSON"))
		{
			ExportJSON();
		}*/
		GUILayout.EndHorizontal();
	}

	public void ImportJSON()
	{
		Debug.Log("Trying to import " + fileToTest.name);
		TokenDictionary dic = new TokenDictionary();
		dic.fileContents = fileToTest.text;
		dic.filePath = fileToTest.name;
		if (dic.ImportJSON())
			Debug.Log("Imported successfully.");
	}

	#endif
}
