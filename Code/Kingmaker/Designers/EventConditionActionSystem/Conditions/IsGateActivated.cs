using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("04d841c4b838459b930b877555a8b32a")]
public class IsGateActivated : Condition
{
	[SerializeField]
	public GateReference? Gate;

	protected override string GetConditionCaption()
	{
		return "Gate was activated";
	}

	protected override bool CheckCondition()
	{
		if (Gate == null)
		{
			return false;
		}
		CutscenePlayerData cutscenePlayerData = ContextData<NamedParametersContext.ContextData>.Current?.Context.Cutscene;
		if (cutscenePlayerData == null)
		{
			return false;
		}
		Gate gate = Gate.Get();
		return cutscenePlayerData.ActivatedGates.Any((CutscenePlayerGateData data) => data.Gate == gate);
	}
}
