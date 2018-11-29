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

[CreateAssetMenu (menuName = "New CountryData", order = 50)]
public class CountryData : SerializedScriptableObject {

	public string countryName;
	public string countryCitizens;
	public string countryType;
	public string countryBiome;
	public string countrySymbol;
	public List<string> countryAdj = new List<string>();
}
