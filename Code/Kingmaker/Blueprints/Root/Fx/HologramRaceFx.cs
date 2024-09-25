using System;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Fx;

[Serializable]
public class HologramRaceFx
{
	[SerializeField]
	public Skeleton Race;

	public PrefabLink HologramPrefab;
}
