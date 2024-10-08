using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("bed52afe8d7f45f9bb1df104cfddc793")]
public class ShowPartySelection : GameAction
{
	[SerializeField]
	private List<BlueprintUnitReference> m_RequiredCompanions = new List<BlueprintUnitReference>();

	public ActionList ActionsAfterPartySelection;

	public bool ActionsCannotBeCanceled;

	[HideIf("ActionsCannotBeCanceled")]
	public ActionList ActionsIfCanceled;

	public bool ShowRemoteCompanions;

	public override string GetCaption()
	{
		return "Show party selection";
	}

	protected override void RunAction()
	{
		List<BlueprintUnitReference> requiredCompanions = m_RequiredCompanions;
		if (requiredCompanions != null && requiredCompanions.Count > 0)
		{
			EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
			{
				h.HandleSetRequiredUnits(m_RequiredCompanions.Select((BlueprintUnitReference x) => x.Get()).ToList());
			});
		}
		bool needPause = Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap;
		if (needPause)
		{
			Game.Instance.RequestPauseUi(isPaused: true);
		}
		bool allMatchFinishActions = ActionsAfterPartySelection.Actions.All((GameAction a) => ActionsIfCanceled.Actions.Any((GameAction b) => b.GetCaption() == a.GetCaption())) && ActionsIfCanceled.Actions.All((GameAction a) => ActionsAfterPartySelection.Actions.Any((GameAction b) => b.GetCaption() == a.GetCaption()));
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(delegate
			{
				if (needPause)
				{
					Game.Instance.RequestPauseUi(isPaused: false);
				}
				RunAfterPartyActions();
			}, delegate
			{
				if (needPause)
				{
					Game.Instance.RequestPauseUi(isPaused: false);
				}
				if (!ActionsCannotBeCanceled)
				{
					ActionsIfCanceled.Run();
				}
				else
				{
					RunAfterPartyActions();
				}
			}, Game.Instance.LoadedAreaState.Settings.CapitalPartyMode, allMatchFinishActions, !ActionsCannotBeCanceled, ShowRemoteCompanions);
		});
	}

	private void RunAfterPartyActions()
	{
		Game.Instance.Player.FixPartyAfterChange();
		ActionsAfterPartySelection.Run();
	}
}
