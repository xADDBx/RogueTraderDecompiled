using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("4501b249e65032545bdf731dd06d4273")]
[DisplayName("Toggle Party Occluded Object Highlighter")]
public class TogglePartyOccludedObjectHighlighter : GameAction
{
	[SerializeField]
	[Tooltip("Список персонажей, для которых нужно включить/выключить OccludedObjectHighlighter. ActiveUnits включает персонажей и питомцев, PartyCharacters только персонажей.")]
	private Player.CharactersList m_UnitsList;

	[Tooltip("Включить или выключить OccludedObjectHighlighter")]
	public bool Enable = true;

	[Tooltip("Включить обработку питомцев отдельно от списка персонажей")]
	public bool ProcessPetsExplicitly;

	[Tooltip("Включить или выключить OccludedObjectHighlighter для питомцев (если ProcessPetsExplicitly = true)")]
	[ShowIf("ProcessPetsExplicitly")]
	public bool EnableForPets = true;

	[Tooltip("Выводить отладочные сообщения в лог")]
	public bool DebugLog;

	public override string GetCaption()
	{
		if (ProcessPetsExplicitly)
		{
			return (Enable ? "Enable" : "Disable") + " OccludedObjectHighlighter for party, " + (EnableForPets ? "Enable" : "Disable") + " for pets";
		}
		return string.Format("{0} OccludedObjectHighlighter for {1}", Enable ? "Enable" : "Disable", m_UnitsList);
	}

	protected override void RunAction()
	{
		int num = 0;
		if (ProcessPetsExplicitly)
		{
			foreach (BaseUnitEntity item in Game.Instance.Player.Party.ToList())
			{
				num += ProcessUnit(item, Enable);
			}
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if (partyAndPet.IsPet)
				{
					num += ProcessUnit(partyAndPet, EnableForPets);
				}
			}
		}
		else
		{
			List<BaseUnitEntity> list = Game.Instance.Player.GetCharactersList(m_UnitsList).ToList();
			if (list.Count == 0)
			{
				if (DebugLog)
				{
					Element.LogError(this, $"No units found in player party with filter {m_UnitsList}");
				}
				return;
			}
			foreach (BaseUnitEntity item2 in list)
			{
				num += ProcessUnit(item2, Enable);
			}
		}
		if (DebugLog)
		{
			PFLog.Default.Log($"Processed {num} units, OccludedObjectHighlighter was modified");
		}
	}

	private int ProcessUnit(BaseUnitEntity unit, bool enableHighlighter)
	{
		if (unit == null || unit.View == null)
		{
			if (DebugLog)
			{
				Element.LogError(this, "Unit is null or has no view");
			}
			return 0;
		}
		OccludedObjectHighlighter component = unit.View.gameObject.GetComponent<OccludedObjectHighlighter>();
		if (component == null)
		{
			if (DebugLog)
			{
				Element.LogError(this, "Unit " + unit.CharacterName + " has no OccludedObjectHighlighter component");
			}
			return 0;
		}
		component.enabled = enableHighlighter;
		if (enableHighlighter)
		{
			unit.View.SetOccluderColorAndState();
			component.InvalidateRenderers();
		}
		if (DebugLog)
		{
			PFLog.Default.Log(string.Format("{0} OccludedObjectHighlighter for unit {1} (IsPet: {2})", enableHighlighter ? "Enabled" : "Disabled", unit.CharacterName, unit.IsPet));
		}
		return 1;
	}
}
