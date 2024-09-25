using System;

namespace Kingmaker.Visual.CharacterSystem;

[Flags]
public enum BodyPartType : long
{
	Belt = 1L,
	Brows = 2L,
	Cuffs = 4L,
	Eyes = 8L,
	Feet = 0x10L,
	Forearms = 0x20L,
	Hands = 0x40L,
	Head = 0x80L,
	Helmet = 0x100L,
	KneeCops = 0x200L,
	LowerLegs = 0x400L,
	MaskBottom = 0x800L,
	Goggles = 0x1000L,
	RingLeft = 0x2000L,
	RingRight = 0x4000L,
	Skirt = 0x8000L,
	Spaulders = 0x10000L,
	Torso = 0x20000L,
	UpperArms = 0x40000L,
	UpperLegs = 0x80000L,
	MaskTop = 0x100000L,
	LowerLegsExtra = 0x200000L,
	TorsoExtra = 0x400000L,
	FacialHair = 0x800000L,
	HighCollar = 0x1000000L,
	Lashes = 0x2000000L,
	LowerArmsExtra = 0x4000000L,
	Ears = 0x8000000L,
	HeadBottom = 0x10000000L,
	SpaulderL = 0x20000000L,
	SpaulderR = 0x40000000L,
	CuffL = 0x80000000L,
	CuffR = 0x100000000L,
	Hoses = 0x200000000L,
	Teeth = 0x400000000L,
	Cape = 0x8000000000L,
	Augment1 = 0x800000000L
}
