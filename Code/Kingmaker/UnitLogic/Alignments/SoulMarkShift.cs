using System;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UnitLogic.Alignments;

[Serializable]
public class SoulMarkShift
{
	public SoulMarkDirection Direction;

	[InfoBox("Value in `points`, Value >= 0, 0 means no alignment shift or lets you check by rank")]
	[HideIf("m_checkByValueUnavailable")]
	public int Value;

	[HideIf("m_checkByRankUnavailable")]
	public bool CheckByRank;

	[InfoBox("Value in `rank`, Value >= 1 and <= 5, 0 means no rank in alignment")]
	[ShowIf("CheckByRank")]
	[HideIf("m_checkByRankUnavailable")]
	public int Rank;

	[HideIf("NoShift")]
	public LocalizedString Description;

	private bool NoShift => Direction == SoulMarkDirection.None;

	private bool m_checkByValueUnavailable
	{
		get
		{
			if (!NoShift)
			{
				return CheckByRank;
			}
			return true;
		}
	}

	private bool m_checkByRankUnavailable
	{
		get
		{
			if (!NoShift)
			{
				if (CheckByValue)
				{
					return !CheckByRank;
				}
				return false;
			}
			return true;
		}
	}

	public bool CheckByValue => Value > 0;

	public bool Empty
	{
		get
		{
			if ((CheckByValue || CheckByRank) && (!CheckByRank || Rank != 0))
			{
				return NoShift;
			}
			return true;
		}
	}
}
