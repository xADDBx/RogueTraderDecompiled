using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("f9ec281be18644538e5cc20e80b54642")]
public class CheckOnColony : Condition
{
	[SerializeField]
	private bool m_CheckOnCurrentColony;

	[SerializeField]
	[HideIf("m_CheckOnCurrentColony")]
	private BlueprintColonyReference m_Colony;

	private BlueprintColony Colony => m_Colony?.Get();

	protected override string GetConditionCaption()
	{
		return "Check if on colony";
	}

	protected override bool CheckCondition()
	{
		if (!m_CheckOnCurrentColony)
		{
			return Game.Instance.ColonizationController.CheckOnColony(Colony);
		}
		Colony colony = Game.Instance.Player.ColoniesState.ColonyContextData.Colony;
		if (colony != null)
		{
			return Game.Instance.ColonizationController.CheckOnColony(colony.Blueprint);
		}
		return false;
	}
}
