using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("bed52afe8d7f45f9bb1df104cfddc793")]
public class ShowPartySelection : GameAction
{
	[SerializeField]
	private List<BlueprintUnitReference> m_RequiredCompanions = new List<BlueprintUnitReference>();

	public ActionList ActionsAfterPartySelection;

	public ActionList ActionsIfCanceled;

	public override string GetCaption()
	{
		return "Show party selection";
	}

	public override void RunAction()
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
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(delegate
			{
				Game.Instance.Player.FixPartyAfterChange();
				if (needPause)
				{
					Game.Instance.RequestPauseUi(isPaused: false);
				}
				ActionsAfterPartySelection.Run();
			}, delegate
			{
				if (needPause)
				{
					Game.Instance.RequestPauseUi(isPaused: false);
				}
				ActionsIfCanceled.Run();
			}, Game.Instance.LoadedAreaState.Settings.CapitalPartyMode);
		});
	}
}
