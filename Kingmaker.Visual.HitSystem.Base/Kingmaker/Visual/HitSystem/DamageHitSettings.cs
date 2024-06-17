using System;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class DamageHitSettings
{
	public bool FollowTarget;

	public HitLevel HitLevel = HitLevel.Major;

	public bool MinorAdditionalHits = true;

	public bool BillboardBlood = true;

	public bool DirectionalBlood;
}
