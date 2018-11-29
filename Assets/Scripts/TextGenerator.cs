using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Text;
using Sirenix.Utilities.Editor;
using System.IO;

public class TextGenerator : SerializedMonoBehaviour {

	public enum Season 
	{
		Summer,
		Fall,
		Winter,
		Spring
	}

	public TextAsset bookIntroduction;
	public TextAsset bookConclusion;
	Dictionary<string, List<string>> allDictionaries = new Dictionary<string, List<string>>();
	Dictionary<string, int> allNames = new Dictionary<string, int>();
	public List<TextAsset> AllDictionaryFiles = new List<TextAsset>();
	public List<CountryData> AllCountries = new List<CountryData>();

	[Title("Formula to be generated", bold: false)]
	[HideLabel]
	[MultiLineProperty(10)]
	[OnInspectorGUI("ShowGenerateButton")]
	public string beastFormula;
	public Season currentSeason = Season.Summer;
	[LabelText("Times To Generate (test)")]
	public int TimesToGenerate = 1;
	[Header("---- Final Book Generation ----")]
	[OnInspectorGUI("ShowGenerateBookButton")]
	public int EntriesInBook = 180;
	public string SectionSeparator;

	string generatedText;

	#if UNITY_EDITOR
	void ShowGenerateButton()
	{
		SirenixEditorGUI.BeginBox();
		if (GUILayout.Button("Generate"))
		{
			Debug.ClearDeveloperConsole();
			for(int i=0; i < TimesToGenerate; i++)
				generatedText = RunGeneration();
		}
		SirenixEditorGUI.MessageBox(generatedText);
		SirenixEditorGUI.EndBox();
	}

	void ShowGenerateBookButton()
	{
		SirenixEditorGUI.BeginBox();
		if (GUILayout.Button("Generate Whole Book"))
		{
			RunGeneration(true);
		}
		SirenixEditorGUI.EndBox();
	}
	#endif



	string RunGeneration(bool generateBook = false)
	{
		allDictionaries.Clear();
		allNames.Clear();
		foreach(TextAsset text in AllDictionaryFiles)
		{
			if (text == null)
				continue;
			Debug.Log("Importing " + text.name);
			TokenDictionary dic = new TokenDictionary();
			dic.fileContents = text.text;
			dic.filePath = text.name;
			dic.ImportJSON();

			foreach (string key in dic.phrases.Keys)
			{
				if (allDictionaries.ContainsKey(key))
				{
					Debug.LogError("Key " + key + " from " + text.name + " has already been added previously");
					continue;
				}
				allDictionaries[key] = dic.phrases[key];
			}
		}
		Debug.Log("Finished importing.");

		if (generateBook)
		{
			beastFormula = "%entry";
			StreamWriter bookWriter = new StreamWriter("Assets/Aviary.txt");
			bookWriter.Write(bookIntroduction.text);

			int generatedEntries = 0;
			currentSeason = Season.Summer;
			// Write season introduction
			bookWriter.Write("\n\n" + SectionSeparator + "\n\n" + currentSeason.ToString().ToUpper() + "\n\n");
			bookWriter.Write(GenerateOneEntry("%seasonLine"));
			for (;generatedEntries < EntriesInBook/4; generatedEntries++)
			{
				bookWriter.Write("\n\n" + SectionSeparator + "\n\n");
				bookWriter.Write(GenerateOneEntry(beastFormula));
			}

			currentSeason = Season.Fall;
			// Write season introduction
			bookWriter.Write("\n\n" + SectionSeparator + "\n\n" + currentSeason.ToString().ToUpper() + "\n\n");
			bookWriter.Write(GenerateOneEntry("%seasonLine"));
			for (;generatedEntries < 2*EntriesInBook/4; generatedEntries++)
			{
				bookWriter.Write("\n\n" + SectionSeparator + "\n\n");
				bookWriter.Write(GenerateOneEntry(beastFormula));
			}

			currentSeason = Season.Winter;
			// Write season introduction
			bookWriter.Write("\n\n" + SectionSeparator + "\n\n" + currentSeason.ToString().ToUpper() + "\n\n");
			bookWriter.Write(GenerateOneEntry("%seasonLine"));
			for (;generatedEntries < 3*EntriesInBook/4; generatedEntries++)
			{
				bookWriter.Write("\n\n" + SectionSeparator + "\n\n");
				bookWriter.Write(GenerateOneEntry(beastFormula));
			}
			
			currentSeason = Season.Spring;
			// Write season introduction
			bookWriter.Write("\n\n" + SectionSeparator + "\n\n" + currentSeason.ToString().ToUpper() + "\n\n");
			bookWriter.Write(GenerateOneEntry("%seasonLine"));
			for (;generatedEntries < EntriesInBook; generatedEntries++)
			{
				bookWriter.Write("\n\n" + SectionSeparator + "\n\n");
				bookWriter.Write(GenerateOneEntry(beastFormula));
			}

			bookWriter.Write(bookConclusion.text);
			bookWriter.Close();

			string result = "Book written to Aviary.txt file with " + generatedEntries + " entries.";
			Debug.Log("Names list has " + allNames.Count + " entries");
			Debug.Log(result);
			return result;
		}
		else
		{
			Debug.Log("Generating text");
			string bird = GenerateOneEntry(beastFormula);
			Debug.Log(bird);
			return bird;			
		}
	}

	// Local variables 
	CountryData currentCountry;
	string currentName;
	Dictionary<string, List<string>> replacementsUsed = new Dictionary<string, List<string>>();
	public string GenerateOneEntry(string formula)
	{
		StringBuilder stringBuilder = new StringBuilder(formula);

		// Pick local variables for this entry
		currentCountry = AllCountries[Random.Range(0, AllCountries.Count)];
		currentName = string.Empty;
		replacementsUsed.Clear();

		int count = 0;
		bool isNameReady = false;
		while (stringBuilder.ToString().Contains("%") || stringBuilder.ToString().Contains("$"))
		{
			string currentStr = stringBuilder.ToString();
			currentStr = currentStr.Replace("\r", " ");
			currentStr = currentStr.Replace("\n", " ");
			string[] words = currentStr.Split(' ');
			string token = string.Empty;
			string modifiedToken = string.Empty;
			bool isLocalVariable = false;
			bool usingModifiedToken = false;
			bool shouldCapitalizeFirstLetter = false;
			bool shouldCapitalizeAllFirstLetters = false;

			foreach (string word in words)
			{
				if (word.StartsWith("%"))
				{
					token = word;
					break;
				}
				if (word.StartsWith("$"))
				{
					isLocalVariable = true;
					token = word;
					break;
				}
			}

			token = token.Replace("%", "");
			token = token.Replace("$", "");
			token = token.Replace(".", "");
			token = token.Replace(",", "");
			token = token.Replace(":", "");
			token = token.Replace("!", "");
			token = token.Replace(")", "");
			token = token.Replace("?", "");
			token = token.Replace("'s", "");

			// Limit amount of times tried
			count++;
			if (count > 30)
			{
				Debug.LogError("Could not find replacement for " + token);
				break;
			}

			if (isLocalVariable)
			{
				string replacement = FindReplacementForLocalVariable(token);
				int tokenIndex = currentStr.IndexOf("$" + token);
				stringBuilder.Remove(tokenIndex, token.Length + 1);
				stringBuilder.Insert(tokenIndex, replacement);
				if (currentName.Contains("$"))
					currentName = currentName.Replace("$" + token, replacement);
				count = 0;
				continue;
			}
			else
			{
				// Add another bird's name
				if (token == "name2")
				{
					// Restart from scratch if there's no name2 available
					if (allNames.Count == 0)
					{
						Debug.LogError("Restaring since there's still no names to pull from for token %name2");
						return GenerateOneEntry(formula);
					}

					string replacement = new List<string>(allNames.Keys)[Random.Range(0, allNames.Count)];
					int tokenIndex = currentStr.IndexOf("%" + token);
					stringBuilder.Remove(tokenIndex, token.Length + 1);
					stringBuilder.Insert(tokenIndex, replacement);
					count = 0;
					continue;
				}
			}

			string currentTokenPrefix = "%";
			if (token.StartsWith("^^"))
			{
				currentTokenPrefix = "%^^";
				shouldCapitalizeAllFirstLetters = true;
				token = token.Replace("^^", "");
			}
			else if (token.StartsWith("^"))
			{
				currentTokenPrefix = "%^";
				shouldCapitalizeFirstLetter = true;
				token = token.Replace("^", "");
			}

			if (!allDictionaries.ContainsKey(token))
			{
				if (token.StartsWith("season"))
					modifiedToken = token + currentSeason.ToString();
				else
				{
					// Look for biome key
					modifiedToken = token + CapitalizeFirstLetter(currentCountry.countryBiome);
				}
				usingModifiedToken = true;
			}

			if (usingModifiedToken && !allDictionaries.ContainsKey(modifiedToken))
			{
				Debug.LogError("Could not find token " + token + " or " + modifiedToken);
				break;
			}
			else
			{
				string replacement = string.Empty;
				int tokenIndex = 0;
				if (usingModifiedToken)
					replacement = allDictionaries[modifiedToken][Random.Range(0, allDictionaries[modifiedToken].Count)];
				else
					replacement = allDictionaries[token][Random.Range(0, allDictionaries[token].Count)];

				// Process capitalization
				if (shouldCapitalizeFirstLetter)
					replacement = CapitalizeFirstLetter(replacement);
				else if (shouldCapitalizeAllFirstLetters)
					replacement = CapitalizeAllFirstLetters(replacement);

				// Overwrite replacement with name if it's the case
				if (!isNameReady && token == "name" && currentName != string.Empty)
					replacement = currentName;

				// Make sure that this replacement is unique in this entry
				if (token != "name" && token != "bird" && token != "seems")
				{
					if (replacementsUsed.ContainsKey(token) && replacementsUsed[token].Contains(replacement))
					{
						Debug.LogWarning("tried to use same replacement for token " + token + " : " + replacement);
						continue;
					}
					if (!replacementsUsed.ContainsKey(token))
						replacementsUsed[token] = new List<string>(new string []{ replacement });
					else
						replacementsUsed[token].Add(replacement);
				}
				
				tokenIndex = currentStr.IndexOf(currentTokenPrefix + token);
				stringBuilder.Remove(tokenIndex, token.Length + currentTokenPrefix.Length);
				stringBuilder.Insert(tokenIndex, replacement);
				//Debug.Log("replaced, string is now " + stringBuilder.ToString());
				count = 0;

				if (!isNameReady)
				{
					if (token == "name" && currentName == string.Empty)
						currentName = replacement;
					if (currentName.Contains("%"))
						currentName = currentName.Replace(currentTokenPrefix + token, replacement);
					else
					{
						// Name is ready
						currentName = currentName.Replace(" + ", "").Replace(" +", "").Replace("+ ", "");

						if (!allNames.ContainsKey(currentName) && currentName != string.Empty)
						{
							isNameReady = true;
							allNames[currentName] = 0;
						}
						else
						{
							if (currentName != string.Empty)
							{
								Debug.LogError("Found repeated name: " + currentName);
								stringBuilder.Replace(currentName, "%name");
								currentName = string.Empty;
							}
						}
					}
				}
			}
		}

		// Glue all words that need glueing
		stringBuilder.Replace(" + ", "").Replace(" +", "").Replace("+ ", "");
		// Change "a" to "an" before vowels
		stringBuilder.Replace(" a a", " an a").Replace(" a e", " an e").Replace(" a i", " an i").Replace(" a o", " an o").Replace(" a u", " an u");
		stringBuilder.Replace(" a A", " an A").Replace(" a E", " an E").Replace(" a I", " an I").Replace(" a O", " an O").Replace(" a U", " an U");
		stringBuilder.Replace(" A a", " An a").Replace(" A e", " An e").Replace(" A i", " An i").Replace(" A o", " An o").Replace(" A u", " An u");
		stringBuilder.Replace(" A A", " An A").Replace(" A E", " An E").Replace(" A I", " An I").Replace(" A O", " An O").Replace(" A U", " An U");
		stringBuilder.Replace("\na a", "\nan a").Replace("\na e", "\nan e").Replace("\na i", "\nan i").Replace("\na o", "\nan o").Replace("\na u", "\nan u");
		stringBuilder.Replace("\na A", "\nan A").Replace("\na E", "\nan E").Replace("\na I", "\nan I").Replace("\na O", "\nan O").Replace("\na U", "\nan U");
		stringBuilder.Replace("\nA a", "\nAn a").Replace("\nA e", "\nAn e").Replace("\nA i", "\nAn i").Replace("\nA o", "\nAn o").Replace("\nA u", "\nAn u");
		stringBuilder.Replace("\nA A", "\nAn A").Replace("\nA E", "\nAn E").Replace("\nA I", "\nAn I").Replace("\nA O", "\nAn O").Replace("\nA U", "\nAn U");
		return stringBuilder.ToString();
	}

	string FindReplacementForLocalVariable (string token)
	{
		string replacement = token;

		if (token == "countryName")
			replacement = currentCountry.countryName;
		else if (token == "countryCitizens")
			replacement = currentCountry.countryCitizens;
		else if (token == "countryType")
			replacement = currentCountry.countryType;
		else if (token == "biome")
			replacement = currentCountry.countryBiome;
		else if (token == "countrySymbol")
			replacement = currentCountry.countrySymbol;
		else if (token == "countryAdj")
			replacement = currentCountry.countryAdj[Random.Range(0, currentCountry.countryAdj.Count)];

		if (replacement == token)
			Debug.LogError("Invalid local variable $" + token);

		return replacement;
	}

	string CapitalizeFirstLetter(string input)
	{
		if (input.Length <= 1)
			return input.ToUpper();
		string firstLetter = input.Substring(0,1);
		if (firstLetter == "%")
			firstLetter = "%^";
		return firstLetter.ToUpper() + input.Substring(1, input.Length-1);
	}

	string CapitalizeAllFirstLetters(string input)
	{
		if (input.Length <= 1)
			return input.ToUpper();

		StringBuilder result = new StringBuilder();
		string[] words = input.Split(' ');
		for (int i=0; i<words.Length; i++)
		{
			words[i] = CapitalizeFirstLetter(words[i]);
			result.Append(" " + words[i]);
		}
		result.Remove(0,1);

		return result.ToString();
	}
}
