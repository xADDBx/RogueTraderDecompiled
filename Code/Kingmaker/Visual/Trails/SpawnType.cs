using System;

namespace Kingmaker.Visual.Trails;

[Flags]
public enum SpawnType
{
	Invalid = 0,
	Single = 1,
	Double = 2,
	Disabled = 4,
	SingleDisabled = 5,
	DoubleDisabled = 6
}
