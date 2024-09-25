using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("59303f79a33946b0a3cb099809b0804b")]
public class ColonyProjectFinished : Condition
{
	[SerializeField]
	private BlueprintColonyProjectReference m_Project;

	private BlueprintColonyProject Project => m_Project?.Get();

	protected override string GetConditionCaption()
	{
		return "Project " + Project.Name + " is finished";
	}

	protected override bool CheckCondition()
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			if (colony.Colony.Projects.Any((ColonyProject project) => project.Blueprint == Project && project.IsFinished))
			{
				return true;
			}
		}
		return false;
	}
}
