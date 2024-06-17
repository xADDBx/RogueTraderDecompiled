using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker._TmpTechArt;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponsListDism", order = 1)]
public class WeaponsListDism : ScriptableObject
{
	[Serializable]
	public class PrefabInPair
	{
		public GameObject PrefabPairGo;

		public int PrefabPairInt;
	}

	public bool ForceRagdoll;

	public bool ForceDismemberment;

	public List<PrefabInPair> WeaponsChancesArray = new List<PrefabInPair>();
}
