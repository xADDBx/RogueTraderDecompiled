using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("fad8e98d61f54ac7ac8ce20cc204b9ca")]
[PlayerUpgraderAllowed(false)]
public class ChangeColonyStats : GameAction
{
	[ShowIf("ApplyToSpecificColony")]
	[SerializeField]
	private BlueprintColonyReference m_Colony;

	[HideIf("m_ApplyToAllColonies")]
	[SerializeField]
	private bool m_ApplyToCurrentColony;

	[HideIf("m_ApplyToCurrentColony")]
	[SerializeField]
	private bool m_ApplyToAllColonies;

	[SerializeField]
	private int m_ContentmentModifier;

	[SerializeField]
	private int m_SecurityModifier;

	[SerializeField]
	private int m_EfficiencyModifier;

	private BlueprintColony Colony => m_Colony?.Get();

	private bool ApplyToSpecificColony
	{
		get
		{
			if (!m_ApplyToAllColonies)
			{
				return !m_ApplyToCurrentColony;
			}
			return false;
		}
	}

	public override string GetCaption()
	{
		return "Change colony stats";
	}

	protected override void RunAction()
	{
		ColonyStatModifierType modifierType = ColonyStatModifierType.Other;
		BlueprintScriptableObject blueprintScriptableObject = null;
		ColoniesState coloniesState = Game.Instance.Player.ColoniesState;
		if (Game.Instance.Player.ColoniesState.ColonyContextData.Event != null)
		{
			modifierType = ColonyStatModifierType.Event;
			blueprintScriptableObject = Game.Instance.Player.ColoniesState.ColonyContextData.Event;
		}
		if (m_ApplyToCurrentColony)
		{
			Colony colony = Game.Instance.Player.ColoniesState.ColonyContextData.Colony;
			if (colony != null)
			{
				ApplyModifiers(colony, modifierType, blueprintScriptableObject);
			}
			return;
		}
		if (ApplyToSpecificColony)
		{
			Colony colony2 = coloniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == Colony)?.Colony;
			if (colony2 != null)
			{
				ApplyModifiers(colony2, modifierType, blueprintScriptableObject);
			}
			return;
		}
		if (m_ContentmentModifier != 0)
		{
			coloniesState.ContentmentModifiersForAllColonies.Add(new ColonyStatModifier
			{
				Value = m_ContentmentModifier,
				ModifierType = modifierType,
				Modifier = blueprintScriptableObject
			});
			EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
			{
				h.HandleContentmentInAllColoniesChanged(m_ContentmentModifier);
			});
		}
		if (m_SecurityModifier != 0)
		{
			coloniesState.SecurityModifiersForAllColonies.Add(new ColonyStatModifier
			{
				Value = m_SecurityModifier,
				ModifierType = modifierType,
				Modifier = blueprintScriptableObject
			});
			EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
			{
				h.HandleSecurityInAllColoniesChanged(m_SecurityModifier);
			});
		}
		if (m_EfficiencyModifier != 0)
		{
			coloniesState.EfficiencyModifiersForAllColonies.Add(new ColonyStatModifier
			{
				Value = m_EfficiencyModifier,
				ModifierType = modifierType,
				Modifier = blueprintScriptableObject
			});
			EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
			{
				h.HandleEfficiencyInAllColoniesChanged(m_EfficiencyModifier);
			});
		}
		foreach (ColoniesState.ColonyData colony3 in coloniesState.Colonies)
		{
			ApplyModifiers(colony3.Colony, modifierType, blueprintScriptableObject);
		}
	}

	private void ApplyModifiers(Colony colony, ColonyStatModifierType modifierType, BlueprintScriptableObject colonyModifier)
	{
		if (m_ContentmentModifier != 0)
		{
			colony.ChangeContentment(m_ContentmentModifier, modifierType, colonyModifier);
		}
		if (m_EfficiencyModifier != 0)
		{
			colony.ChangeEfficiency(m_EfficiencyModifier, modifierType, colonyModifier);
		}
		if (m_SecurityModifier != 0)
		{
			colony.ChangeSecurity(m_SecurityModifier, modifierType, colonyModifier);
		}
		EventBus.RaiseEvent(delegate(IColonyStatsHandler h)
		{
			h.HandleColonyStatsChanged();
		});
	}
}
