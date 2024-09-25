using System;

namespace Kingmaker.UnitLogic.Alignments;

[Flags]
public enum AlignmentMaskType
{
	LawfulGood = 1,
	NeutralGood = 2,
	ChaoticGood = 4,
	LawfulNeutral = 8,
	TrueNeutral = 0x10,
	ChaoticNeutral = 0x20,
	LawfulEvil = 0x40,
	NeutralEvil = 0x80,
	ChaoticEvil = 0x100,
	Good = 7,
	Evil = 0x1C0,
	Lawful = 0x49,
	Chaotic = 0x124,
	Any = 0x1FF,
	None = 0
}
