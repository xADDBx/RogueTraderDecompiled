using System.Collections.Generic;
using Kingmaker.Visual.Particles.SnapController;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesSnapController : SnapControllerBase
{
	[HideInInspector]
	[SerializeField]
	private bool m_useRandomBones;

	[HideInInspector]
	[SerializeField]
	private float m_randomBonesPercent = 1f;

	internal override BoneCollector.Result GetBones(SnapMapBase snapMap, HashSet<FxBone> fxBones)
	{
		return new BoneCollector(snapMap, BonesNames, base.LocatorGroups, ignoreRotatableBones: SnapType == ParticleSnapType.Transforms && !m_IsRotatableCopy, ignoreNonRotatableBones: SnapType == ParticleSnapType.Transforms && m_IsRotatableCopy, ignoreSpecialBones: IgnoreSpecialBones, rotationRootBoneName: Offset.WorldRotationBone, useRandomBonesAmount: SnapType == ParticleSnapType.Transforms && m_useRandomBones, randomPercent: m_randomBonesPercent).Collect(fxBones);
	}
}
