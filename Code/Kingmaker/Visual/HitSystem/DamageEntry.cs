using System;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class DamageEntry
{
	public DamageType Type;

	public HitCollection Hits;

	public AkSwitchReference SoundSwitch;
}
