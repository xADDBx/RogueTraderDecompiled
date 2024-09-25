using System;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedExample : MonoBehaviour
{
	[Serializable]
	public class IntroSentence
	{
		public EnemyType enemyType;

		public string sentence;
	}

	[Serializable]
	public class Spawner
	{
		public string name;

		public Vector2 position;
	}

	private static AdvancedExample instance;

	public List<Enemy> enemies;

	public List<IntroSentence> introSentences = new List<IntroSentence>();

	public List<Spawner> spawners = new List<Spawner>();

	public static AdvancedExample Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<AdvancedExample>();
			}
			return instance;
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("Select the AdvancedExample scene object to visualize the table in the editor");
	}
}
