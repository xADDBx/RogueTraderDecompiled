using System;
using System.Collections.Generic;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[CreateAssetMenu(menuName = "Blueprints/Area/SpaceCombat/SpaceCombatBackgroundComposerConfig")]
public class SpaceCombatBackgroundComposerConfigs : ScriptableObject
{
	[Serializable]
	public class StarProperties
	{
		public PrefabLink Prefab;

		public float Scale;
	}

	[Serializable]
	public class PlanetObjectProperties
	{
		public PrefabLink Prefab;

		public float Scale;

		public float AngleOnOrbit;

		public float OffsetY;
	}

	[Serializable]
	public class SpaceObjectProperties
	{
		public PrefabLink Prefab;

		public Vector3 Position;

		public Quaternion Rotation;

		public float Scale = 1f;
	}

	[Header("StarSystem properties")]
	[Range(10f, 30f)]
	[SerializeField]
	public float SystemRadius;

	[SerializeField]
	public Vector3 SystemOffset;

	[Header("SkyDome")]
	public MaterialLink SkyDomeMaterial;

	[Space(10f)]
	[Header("Star and Planets")]
	[SerializeField]
	public StarProperties Star;

	[Header("Properties of instanced\nplanets prefabs can\nbe set in realtime")]
	[SerializeField]
	public List<PlanetObjectProperties> Planets = new List<PlanetObjectProperties>();

	[Header("Properties of instanced\nobject prefab can\nbe set in realtime")]
	[Header("SpaceObject (Creature DLC Test)")]
	[SerializeField]
	public SpaceObjectProperties SpaceObject = new SpaceObjectProperties();
}
