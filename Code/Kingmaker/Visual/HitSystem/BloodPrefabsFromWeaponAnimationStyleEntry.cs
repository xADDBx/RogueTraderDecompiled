using System;
using Kingmaker.View.Animation;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class BloodPrefabsFromWeaponAnimationStyleEntry
{
	public WeaponAnimationStyle WeaponAnimationStyle;

	public bool BillboardBlood;

	public bool DirectionalBlood;

	public BloodPrefabsFromWeaponAnimationStyleEntry()
	{
		BillboardBlood = true;
		DirectionalBlood = false;
	}

	public DamageHitSettings ToDamageHitSettings(HitSystemRoot hitSystemRoot)
	{
		return new DamageHitSettings
		{
			BillboardBlood = BillboardBlood,
			DirectionalBlood = DirectionalBlood,
			FollowTarget = hitSystemRoot.DefaultDamage.FollowTarget,
			HitLevel = hitSystemRoot.DefaultDamage.HitLevel,
			MinorAdditionalHits = hitSystemRoot.DefaultDamage.MinorAdditionalHits
		};
	}
}
