using System;

namespace Kingmaker.Sound;

[Flags]
public enum TriggerType
{
	None = 0,
	AreaLoad = 1,
	AreaUnload = 2,
	ZoneEntered = 4,
	ZoneExited = 8,
	ParticleAnimation = 0x10,
	Enabled = 0x20,
	Disabled = 0x40
}
