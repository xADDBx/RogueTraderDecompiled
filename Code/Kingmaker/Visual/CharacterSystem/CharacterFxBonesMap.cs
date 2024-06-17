using System.Collections.Generic;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterFxBonesMap : ScriptableObject
{
	public float ParticleSizeScale = 1f;

	public float SizeScale = 1f;

	public float LifetimeScale = 1f;

	public float RateOverTimeScale = 1f;

	public float BurstScale = 1f;

	public List<FxBone> Bones = new List<FxBone>();
}
