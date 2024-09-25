using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
public class RampColorPreset : ScriptableObject
{
	[Serializable]
	public class IndexSet
	{
		[SerializeField]
		public string Name = "";

		[SerializeField]
		public int PrimaryIndex;

		[SerializeField]
		public int SecondaryIndex;
	}

	[SerializeField]
	public List<IndexSet> IndexPairs = new List<IndexSet>();

	[HideInInspector]
	public int PrimaryIndex = -1;

	[HideInInspector]
	public int SecondaryIndex = -1;
}
