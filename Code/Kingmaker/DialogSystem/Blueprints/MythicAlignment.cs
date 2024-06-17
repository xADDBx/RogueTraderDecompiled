using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.DialogSystem.Blueprints;

[Serializable]
public class MythicAlignment
{
	public BlueprintCharacterClassReference Mythic;

	[AlignmentMask]
	public AlignmentMaskType Alignment;
}
