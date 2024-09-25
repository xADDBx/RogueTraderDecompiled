using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("29147804bf4e4746bbab4eaf5c22d11e")]
public class RequirementSoulMarkRank : Requirement
{
	public SoulMarkShift SoulMarkRequirement;

	public override bool Check(Colony colony = null)
	{
		return SoulMarkShiftExtension.CheckShiftAtLeast(SoulMarkRequirement);
	}

	public override void Apply(Colony colony = null)
	{
	}
}
