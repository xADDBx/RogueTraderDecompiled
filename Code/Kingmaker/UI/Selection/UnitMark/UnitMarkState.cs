using System;

namespace Kingmaker.UI.Selection.UnitMark;

[Flags]
public enum UnitMarkState
{
	None = 0,
	MouseHovered = 1,
	DialogCurrentSpeaker = 2,
	Highlighted = 4,
	Selected = 8,
	CurrentTurn = 0x10,
	CastingSpell = 0x20,
	IsInCombat = 0x40,
	IsInAoEPattern = 0x80,
	GamepadSelected = 0x100
}
