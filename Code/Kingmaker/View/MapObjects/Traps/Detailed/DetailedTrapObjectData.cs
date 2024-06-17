using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps.Detailed;

public class DetailedTrapObjectData : TrapObjectData, IHashable
{
	[JsonProperty]
	private int? m_OverrideDisableDC;

	private new BlueprintTrap Blueprint => (BlueprintTrap)base.Blueprint;

	private new BlueprintTrap OriginalBlueprint => (BlueprintTrap)base.OriginalBlueprint;

	public override int DisableDC
	{
		get
		{
			return m_OverrideDisableDC ?? Blueprint.DisableDC;
		}
		set
		{
			m_OverrideDisableDC = value;
		}
	}

	public override int DisableTriggerMargin => Blueprint.DisableTriggerMargin;

	public override bool IsHiddenWhenInactive => Blueprint.IsHiddenWhenInactive;

	public new DetailedTrapObjectView View => (DetailedTrapObjectView)base.View;

	protected override StatType DisarmSkill => Blueprint.DisarmSkill;

	public DetailedTrapObjectData(DetailedTrapObjectView trapView)
		: base(trapView.UniqueId, trapView.IsInGameBySettings, trapView.Blueprint)
	{
	}

	protected DetailedTrapObjectData(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return DetailedTrapObjectView.CreateView(Blueprint, base.UniqueId, base.ScriptZoneId);
	}

	public override void RunTrapActions()
	{
		Blueprint.TrapActions.Run();
	}

	public override void RunDisableActions(BaseUnitEntity user)
	{
		Blueprint.DisableActions.Run();
		Blueprint.CallComponents(delegate(Experience c)
		{
			Experience.ApplyForSkillCheck(c, user);
		});
	}

	public override bool CanTrigger()
	{
		return Blueprint.TriggerConditions.Check();
	}

	public override bool CanUnitDisable(BaseUnitEntity unit)
	{
		using (ContextData<BlueprintTrap.ElementsData>.Request().Setup(unit, View))
		{
			return Blueprint.DisableConditions.Check();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (m_OverrideDisableDC.HasValue)
		{
			int val2 = m_OverrideDisableDC.Value;
			result.Append(ref val2);
		}
		return result;
	}
}
