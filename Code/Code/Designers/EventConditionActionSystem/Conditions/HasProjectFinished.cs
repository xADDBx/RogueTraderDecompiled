using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using UnityEngine;

namespace Code.Designers.EventConditionActionSystem.Conditions;

[TypeId("bc2d43173eaf4f59950b23969511be3b")]
public class HasProjectFinished : Condition
{
	[SerializeField]
	private BlueprintColonyProjectReference m_Project;

	private BlueprintColonyProject Project => m_Project?.Get();

	protected override string GetConditionCaption()
	{
		return "Has project " + Project?.Name + " finished";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Projects.FirstOrDefault((ColonyProject proj) => proj.Blueprint == Project && proj.IsFinished) != null) != null;
	}
}
