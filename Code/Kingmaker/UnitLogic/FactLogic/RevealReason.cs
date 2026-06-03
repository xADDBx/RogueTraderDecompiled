using System;

namespace Kingmaker.UnitLogic.FactLogic;

[Flags]
public enum RevealReason : byte
{
	None = 0,
	Movement = 1,
	Attack = 2,
	ReceiveDamage = 4
}
