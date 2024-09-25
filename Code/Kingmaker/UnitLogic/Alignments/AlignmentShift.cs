using System;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UnitLogic.Alignments;

[Serializable]
public class AlignmentShift
{
	public AlignmentShiftDirection Direction = AlignmentShiftDirection.TrueNeutral;

	[InfoBox("Value in `points`, Value >= 0, 0 means no alignment shift")]
	public int Value;

	public LocalizedString Description;
}
