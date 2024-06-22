using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("2b18dfce84724c54903d3f6af08e4fac")]
public class IsDLCEnabled : Condition
{
	[SerializeField]
	private BlueprintDlcRewardReference m_BlueprintDlcReward;

	public BlueprintDlcReward BlueprintDlcReward => m_BlueprintDlcReward;

	protected override string GetConditionCaption()
	{
		return $"{BlueprintDlcReward} DLC is enabled";
	}

	protected override bool CheckCondition()
	{
		return BlueprintDlcReward?.IsActive ?? false;
	}
}
