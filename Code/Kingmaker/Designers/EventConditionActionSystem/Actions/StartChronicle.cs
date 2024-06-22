using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("1fd3c7ebaa8a499d9e5f7ac9a43bf6af")]
[PlayerUpgraderAllowed(false)]
public class StartChronicle : GameAction
{
	[SerializeField]
	private BlueprintDialogReference m_Chronicle;

	[SerializeField]
	private BlueprintColonyReference m_ColonyBlueprint;

	private BlueprintDialog Chronicle => m_Chronicle?.Get();

	private BlueprintColony ColonyBlueprint => m_ColonyBlueprint?.Get();

	public override string GetCaption()
	{
		return $"Start chronicle {Chronicle}";
	}

	protected override void RunAction()
	{
		if (!Chronicle || ColonyBlueprint == null)
		{
			return;
		}
		Colony colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData colonyData) => colonyData.Colony.Blueprint == ColonyBlueprint)?.Colony;
		if (colony != null)
		{
			colony.AddStartedChronicle(Chronicle);
			EventBus.RaiseEvent(delegate(IColonizationChronicleHandler h)
			{
				h.HandleChronicleStarted(colony, Chronicle);
			});
		}
	}
}
