using System;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Flags]
public enum AbilityParameter : uint
{
	None = 0u,
	UnitFact = 8u
}
