using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Common;
using Kingmaker.View.MapObjects.Traps.Simple.Strategies;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps.Simple;

public class SimpleTrapObjectData : TrapObjectData, IHashable
{
	[JsonProperty]
	public new string Name;

	[JsonProperty]
	public int PerceptionDC;

	[JsonProperty]
	public float PerceptionRadius;

	[JsonProperty]
	public BlueprintTrapSettings TrapSettings;

	[JsonProperty]
	private int m_DisableDC;

	public new SimpleTrapObjectView View => (SimpleTrapObjectView)base.View;

	public override int DisableDC
	{
		get
		{
			return m_DisableDC;
		}
		set
		{
			m_DisableDC = value;
		}
	}

	public SimpleTrapObjectInfo Info => View.Info;

	public override bool IsHiddenWhenInactive => !Info.DoNotHideWhenInactive;

	public override int DisableTriggerMargin => BlueprintRoot.Instance.BlueprintTrapSettingsRoot.DisableDCMargin;

	protected override StatType DisarmSkill => Info.DisarmSkill;

	public SimpleTrapObjectData(SimpleTrapObjectView trapView)
		: base(trapView)
	{
	}

	protected SimpleTrapObjectData(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return SimpleTrapObjectView.CreateView(this, base.UniqueId, base.ScriptZoneId);
	}

	public override bool CanTrigger()
	{
		return true;
	}

	public override bool CanUnitDisable(BaseUnitEntity unit)
	{
		return true;
	}

	public override void RunTrapActions()
	{
		SimpleTrapCastSpell.Invoke(this);
		base.IsInGame = false;
		float barkDuration = UIUtility.GetBarkDuration(Info.TrapTriggeredText);
		BaseUnitEntity baseUnitEntity = ContextData<BlueprintTrap.ElementsData>.Current?.TriggeringUnit;
		BarkPlayer.Bark(baseUnitEntity, Info.TrapTriggeredText, barkDuration, playVoiceOver: true, baseUnitEntity);
		ActionsHolder trapActions = View.TrapActions;
		if (trapActions != null && trapActions.HasActions)
		{
			View.TrapActions.Run();
		}
	}

	public override void RunDisableActions(BaseUnitEntity unit)
	{
		GameHelper.GainExperienceForSkillCheck(ExperienceHelper.GetXp(EncounterType.SkillCheck, ExperienceHelper.GetCheckExp(DisableDC, Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0)), unit);
		ActionsHolder disableActions = View.DisableActions;
		if (disableActions != null && disableActions.HasActions)
		{
			using (ContextData<InteractingUnitData>.Request().Setup(unit))
			{
				View.DisableActions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(Name);
		result.Append(ref PerceptionDC);
		result.Append(ref PerceptionRadius);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(TrapSettings);
		result.Append(ref val2);
		result.Append(ref m_DisableDC);
		return result;
	}
}
