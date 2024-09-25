using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SolarSystemGeneratorConfig", menuName = "Techart/Space/SolarSystemGeneratorConfig", order = 1)]
public class SolarSystemGeneratorConfig : ScriptableObject
{
	[Serializable]
	public class StellarBody
	{
		[SerializeField]
		public string StellarBodyName = "";

		[SerializeField]
		[Tooltip("Randomize only if > 1")]
		public List<GameObject> BodyVisualsToRandom;

		[SerializeField]
		[Tooltip("Randomize only if any value != 1")]
		public StellarBodyScale Scale;

		[SerializeField]
		public float PersonalOrbitOffset;

		[SerializeField]
		public OrbitAngle OrbitsRandomAngle;

		[SerializeField]
		public DistancesOrbits DistancesBetweenOrbits;

		[Space(10f)]
		[SerializeField]
		public List<StellarBody> Satellites;
	}

	[Serializable]
	public class OrbitAngle
	{
		[SerializeField]
		[Range(-30f, 30f)]
		public float AngleMin;

		[SerializeField]
		[Range(-30f, 30f)]
		public float AngleMax;
	}

	[Serializable]
	public class StellarBodyScale
	{
		[SerializeField]
		[Range(0f, 30f)]
		public float Min = 1f;

		[SerializeField]
		[Range(0f, 30f)]
		public float Max = 1f;
	}

	[Serializable]
	public class DistancesOrbits
	{
		[SerializeField]
		[Range(-30f, 30f)]
		public float Min = 5f;

		[SerializeField]
		[Range(-30f, 30f)]
		public float Max = 5f;
	}

	[SerializeField]
	public List<StellarBody> StellarBodies = new List<StellarBody>();
}
