using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("c8994546dfb24e6da61004bb7598afda")]
public class RewardSoulMark : Reward
{
	public SoulMarkShift SoulMarkShift;

	public override void ReceiveReward(Colony colony = null)
	{
		SoulMarkShiftExtension.ApplyShift(SoulMarkShift, base.OwnerBlueprint);
	}
}
