using System;
using Kingmaker.ResourceLinks;
using Owlcat.QA.Validation;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class ProjectileHitSettings
{
	public bool FollowTarget;

	[ValidateHasAutoDestroy]
	public PrefabLink HitFx;

	[ValidateHasAutoDestroy]
	public PrefabLink HitSnapFx;

	[ValidateHasAutoDestroy]
	public PrefabLink MissFx;

	[ValidateHasAutoDestroy]
	public PrefabLink MissDecalFx;

	public bool NoHitDecal;

	[FormerlySerializedAs("HitDecalFx")]
	public PrefabLink HitDecal;
}
