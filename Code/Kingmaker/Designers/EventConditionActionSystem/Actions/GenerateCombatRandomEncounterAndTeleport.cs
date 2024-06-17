using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;
using Kingmaker.Globalmap.CombatRandomEncounters;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("585e0999b522401abc033c1e23c651f4")]
public class GenerateCombatRandomEncounterAndTeleport : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaReference m_Area;

	[SerializeField]
	[CanBeNull]
	private BlueprintAreaEnterPointReference m_OverrideEnterPoint;

	[SerializeField]
	[CanBeNull]
	private BlueprintRandomGroupOfUnits.Reference m_OverrideRandomGroup;

	[SerializeField]
	private bool m_SpecifyCoverGroup;

	[SerializeField]
	[ShowIf("m_SpecifyCoverGroup")]
	private EntityReference m_OverrideCoverGroup;

	[SerializeField]
	private bool m_SpecifyTrapGroup;

	[SerializeField]
	[ShowIf("m_SpecifyTrapGroup")]
	private EntityReference m_OverrideTrapGroup;

	[SerializeField]
	private bool m_SpecifyAreaEffectGroup;

	[SerializeField]
	[ShowIf("m_SpecifyAreaEffectGroup")]
	private EntityReference m_OverrideAreaEffectGroup;

	[SerializeField]
	private bool m_SpecifyOtherMapObjectGroup;

	[SerializeField]
	[ShowIf("m_SpecifyOtherMapObjectGroup")]
	private EntityReference m_OverrideOtherMapObjectGroup;

	[SerializeField]
	private BlueprintUnlockableFlagReference m_UnlockFlag;

	public BlueprintUnlockableFlag UnlockFlag => m_UnlockFlag?.Get();

	public override string GetCaption()
	{
		return "Run CombatRandomEncounterGenerator and teleport party to area";
	}

	public override void RunAction()
	{
		Game.Instance.RequestPauseUi(isPaused: true);
		Game.Instance.CombatRandomEncounterController.ActivateCombatRandomEncounter(m_Area, m_OverrideEnterPoint, m_OverrideRandomGroup);
		CombatRandomEncounterState combatRandomEncounterState = Game.Instance.Player.CombatRandomEncounterState;
		BlueprintAreaEnterPoint exitPosition = combatRandomEncounterState.EnterPoint;
		if (m_Area.Get().IsShipArea)
		{
			Game.Instance.LoadArea(exitPosition, AutoSaveMode.AfterEntry);
		}
		else
		{
			EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
			{
				h.HandleCall(delegate
				{
					Game.Instance.RequestPauseUi(isPaused: false);
					Game.Instance.LoadArea(exitPosition, AutoSaveMode.AfterEntry);
				}, delegate
				{
					Game.Instance.RequestPauseUi(isPaused: false);
					Game.Instance.LoadArea(exitPosition, AutoSaveMode.AfterEntry);
				}, Game.Instance.LoadedAreaState.Settings.CapitalPartyMode);
			});
		}
		if (m_SpecifyCoverGroup)
		{
			combatRandomEncounterState.CoverGroup = m_OverrideCoverGroup;
		}
		if (m_SpecifyTrapGroup)
		{
			combatRandomEncounterState.TrapGroup = m_OverrideTrapGroup;
		}
		if (m_SpecifyAreaEffectGroup)
		{
			combatRandomEncounterState.AreaEffectGroup = m_OverrideAreaEffectGroup;
		}
		if (m_SpecifyOtherMapObjectGroup)
		{
			combatRandomEncounterState.OtherMapObjectGroup = m_OverrideOtherMapObjectGroup;
		}
		combatRandomEncounterState.UnlockFlag = UnlockFlag;
	}
}
