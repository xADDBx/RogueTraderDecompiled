using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class ParticlesSnapMap : SnapMapBase
{
	private struct LocatorAndParent
	{
		public Transform Locator;

		public Transform Parent;
	}

	[Tooltip("Required only for EquipmentEntity based characters")]
	public CharacterFxBonesMap CharacterFxBonesMap;

	protected override void OnInitialize()
	{
		if (!(CharacterFxBonesMap == null) && !base.Initialized)
		{
			base.SizeScale = CharacterFxBonesMap.SizeScale;
			base.ParticleSizeScale = CharacterFxBonesMap.ParticleSizeScale;
			base.LifetimeScale = CharacterFxBonesMap.LifetimeScale;
			base.RateOverTimeScale = CharacterFxBonesMap.RateOverTimeScale;
			base.BurstScale = CharacterFxBonesMap.BurstScale;
		}
	}
}
