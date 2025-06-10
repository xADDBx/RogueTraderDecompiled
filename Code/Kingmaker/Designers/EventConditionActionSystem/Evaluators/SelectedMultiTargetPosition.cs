using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("17bf3b3c8da1418eb171721cece28f42")]
public class SelectedMultiTargetPosition : PositionEvaluator
{
	[Tooltip("Set to specify target index, otherwise use last selected target.")]
	[SerializeField]
	private bool m_UseIndex;

	[ShowIf("m_UseIndex")]
	[SerializeField]
	private int m_TargetIndex;

	protected override Vector3 GetValueInternal()
	{
		AbilityMultiTargetSelectionHandler abilityMultiTargetSelectionHandler = Game.Instance.SelectedAbilityHandler?.MultiTargetHandler;
		if (abilityMultiTargetSelectionHandler == null)
		{
			Element.LogError(this, "Multi target selection is not available");
			return Vector3.zero;
		}
		TargetWrapper targetWrapper = (m_UseIndex ? abilityMultiTargetSelectionHandler.GetTargetByIndex(m_TargetIndex) : abilityMultiTargetSelectionHandler.GetLastTarget());
		if (targetWrapper == null)
		{
			Element.LogError(this, "No target was set");
			return Vector3.zero;
		}
		return targetWrapper.Point;
	}

	public override string GetCaption()
	{
		if (!m_UseIndex)
		{
			return "Last Selected Target Position";
		}
		return $"Selected Target #{m_TargetIndex} Position";
	}
}
