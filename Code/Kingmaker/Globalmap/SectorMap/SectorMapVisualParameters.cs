using System;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class SectorMapVisualParameters : RegisteredBehaviour
{
	[Serializable]
	public class StarSystemPrefab
	{
		public GameObject ShipMarker;

		public GameObject UnvisitedAndNoPath;

		public GameObject Unvisited;

		public GameObject Visited;

		public GameObject AllActivitiesFinished;
	}

	[SerializeField]
	public WarpPassageVisualParameters WarpPassagesVisualParameters;

	[SerializeField]
	public Transform PlayerShip;

	[SerializeField]
	public float WaitWarpTravelToStartInSeconds;

	[SerializeField]
	public float WaitWarpTravelToEndInSeconds;

	public StarSystemPrefab[] StarSystemPrefabVariants;
}
