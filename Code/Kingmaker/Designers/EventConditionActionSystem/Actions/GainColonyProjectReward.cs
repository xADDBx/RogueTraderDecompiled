using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("4d115ca35e2e4ff2b95e14860b1def39")]
public class GainColonyProjectReward : GameAction
{
	[SerializeField]
	private BlueprintColonyProjectReference m_Project;

	private BlueprintColonyProject Project => m_Project?.Get();

	public override string GetCaption()
	{
		return "Gain project " + Project.Name + " reward despite its status";
	}

	protected override void RunAction()
	{
		foreach (ColoniesState.ColonyData colony2 in Game.Instance.Player.ColoniesState.Colonies)
		{
			Colony colony = colony2.Colony;
			ColonyProject colonyProject = colony.Projects.FirstOrDefault((ColonyProject project) => project.Blueprint == Project);
			if (colonyProject == null)
			{
				continue;
			}
			{
				foreach (Reward component in colonyProject.Blueprint.GetComponents<Reward>())
				{
					component.ReceiveReward(colony);
				}
				break;
			}
		}
	}
}
