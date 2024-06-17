using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.Enums;

public static class AlignmentExtension
{
	public static bool HasComponent(this Alignment alignment, AlignmentComponent component)
	{
		if (component != 0)
		{
			return (int)((uint)alignment & (uint)component) > 0;
		}
		return true;
	}

	public static AlignmentShiftDirection ToDirection(this Alignment alignment)
	{
		return alignment switch
		{
			Alignment.LawfulGood => AlignmentShiftDirection.LawfulGood, 
			Alignment.NeutralGood => AlignmentShiftDirection.NeutralGood, 
			Alignment.ChaoticGood => AlignmentShiftDirection.ChaoticGood, 
			Alignment.LawfulNeutral => AlignmentShiftDirection.LawfulNeutral, 
			Alignment.TrueNeutral => AlignmentShiftDirection.TrueNeutral, 
			Alignment.ChaoticNeutral => AlignmentShiftDirection.ChaoticNeutral, 
			Alignment.LawfulEvil => AlignmentShiftDirection.LawfulEvil, 
			Alignment.NeutralEvil => AlignmentShiftDirection.NeutralEvil, 
			Alignment.ChaoticEvil => AlignmentShiftDirection.ChaoticEvil, 
			_ => throw new ArgumentOutOfRangeException("alignment", alignment, null), 
		};
	}

	public static AlignmentMaskType ToMask(this Alignment alignment)
	{
		return alignment switch
		{
			Alignment.LawfulGood => AlignmentMaskType.LawfulGood, 
			Alignment.NeutralGood => AlignmentMaskType.NeutralGood, 
			Alignment.ChaoticGood => AlignmentMaskType.ChaoticGood, 
			Alignment.LawfulNeutral => AlignmentMaskType.LawfulNeutral, 
			Alignment.TrueNeutral => AlignmentMaskType.TrueNeutral, 
			Alignment.ChaoticNeutral => AlignmentMaskType.ChaoticNeutral, 
			Alignment.LawfulEvil => AlignmentMaskType.LawfulEvil, 
			Alignment.NeutralEvil => AlignmentMaskType.NeutralEvil, 
			Alignment.ChaoticEvil => AlignmentMaskType.ChaoticEvil, 
			_ => throw new ArgumentOutOfRangeException("alignment", alignment, null), 
		};
	}

	public static IEnumerable<Alignment> GetAllAlignments(this AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulGood))
		{
			yield return Alignment.LawfulGood;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticGood))
		{
			yield return Alignment.ChaoticGood;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralGood))
		{
			yield return Alignment.NeutralGood;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralEvil))
		{
			yield return Alignment.NeutralEvil;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticEvil))
		{
			yield return Alignment.ChaoticEvil;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulEvil))
		{
			yield return Alignment.LawfulEvil;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticNeutral))
		{
			yield return Alignment.ChaoticNeutral;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulNeutral))
		{
			yield return Alignment.LawfulNeutral;
		}
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.TrueNeutral))
		{
			yield return Alignment.TrueNeutral;
		}
	}

	private static void Add(AlignmentGroup[] groups, AlignmentGroup g)
	{
		Array.Resize(ref groups, groups.Length + 1);
		groups[^1] = g;
	}

	public static AlignmentGroup[] TryGetAlignmentGroup(AlignmentMaskType alignmentMask)
	{
		AlignmentGroup[] array = new AlignmentGroup[0];
		if (HasAnyAlignments(alignmentMask))
		{
			Add(array, AlignmentGroup.Any);
			return array;
		}
		if (HasAllNeutralAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.Neutral);
		}
		if (HasAllEGNeutralAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.EGNeutral);
		}
		if (HasAllLCNeutralAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.LCNeutral);
		}
		if (HasAllGoodAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.Good);
		}
		if (HasAllEvilAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.Evil);
		}
		if (HasAllChaoticAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.Chaotic);
		}
		if (HasAllLawfulAlignment(alignmentMask))
		{
			Add(array, AlignmentGroup.Lawful);
		}
		return array;
	}

	public static bool MaskHasAlignment(AlignmentMaskType alignmentMask, AlignmentMaskType alignment)
	{
		return (alignmentMask & alignment) == alignment;
	}

	private static bool HasAnyAlignments(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralEvil) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticEvil) && MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulEvil) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticNeutral) && MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulNeutral))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.TrueNeutral);
		}
		return false;
	}

	private static bool HasAllGoodAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticGood))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralGood);
		}
		return false;
	}

	private static bool HasAllEvilAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralEvil) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticEvil))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulEvil);
		}
		return false;
	}

	private static bool HasAllChaoticAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticNeutral) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticEvil))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticGood);
		}
		return false;
	}

	private static bool HasAllLawfulAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulEvil))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulNeutral);
		}
		return false;
	}

	private static bool HasAllNeutralAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralEvil) && MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticNeutral) && MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulNeutral))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.TrueNeutral);
		}
		return false;
	}

	private static bool HasAllEGNeutralAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.ChaoticNeutral) && MaskHasAlignment(alignmentMask, AlignmentMaskType.LawfulNeutral))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.TrueNeutral);
		}
		return false;
	}

	private static bool HasAllLCNeutralAlignment(AlignmentMaskType alignmentMask)
	{
		if (MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralGood) && MaskHasAlignment(alignmentMask, AlignmentMaskType.NeutralEvil))
		{
			return MaskHasAlignment(alignmentMask, AlignmentMaskType.TrueNeutral);
		}
		return false;
	}

	public static int GetGoodness(this Alignment a)
	{
		switch (a)
		{
		case Alignment.NeutralGood:
		case Alignment.LawfulGood:
		case Alignment.ChaoticGood:
			return 1;
		case Alignment.TrueNeutral:
		case Alignment.LawfulNeutral:
		case Alignment.ChaoticNeutral:
			return 0;
		case Alignment.NeutralEvil:
		case Alignment.LawfulEvil:
		case Alignment.ChaoticEvil:
			return -1;
		default:
			throw new ArgumentOutOfRangeException("a", a, null);
		}
	}

	public static int GetLawfulness(this Alignment a)
	{
		switch (a)
		{
		case Alignment.LawfulNeutral:
		case Alignment.LawfulGood:
		case Alignment.LawfulEvil:
			return 1;
		case Alignment.TrueNeutral:
		case Alignment.NeutralGood:
		case Alignment.NeutralEvil:
			return 0;
		case Alignment.ChaoticNeutral:
		case Alignment.ChaoticGood:
		case Alignment.ChaoticEvil:
			return -1;
		default:
			throw new ArgumentOutOfRangeException("a", a, null);
		}
	}

	public static int GetDifference(this Alignment a, Alignment b)
	{
		int num = Math.Abs(a.GetGoodness() - b.GetGoodness());
		int num2 = Math.Abs(a.GetLawfulness() - b.GetLawfulness());
		return num + num2;
	}
}
