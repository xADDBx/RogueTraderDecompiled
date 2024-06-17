using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("5d13a597de91e4746b804f8233518523")]
public class ContextActionApplyBuff : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BuffEndCondition BuffEndCondition = BuffEndCondition.CombatEnd;

	public bool Permanent;

	[ShowIf("IsCustomDuration")]
	public ContextDurationValue DurationValue;

	public bool ToCaster;

	public bool AsChild;

	[HideIf("Permanent")]
	public bool SameDuration;

	[Tooltip("Change only if you want Ranks to be more than 1")]
	public ContextValue Ranks;

	public ActionList ActionsOnApply;

	public ActionList ActionsOnImmune;

	private bool IsCustomDuration
	{
		get
		{
			if (!Permanent)
			{
				return !SameDuration;
			}
			return false;
		}
	}

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetCaption()
	{
		string text = "Apply" + (AsChild ? " child" : "") + " Buff" + (ToCaster ? " to caster" : "") + ": " + (Buff.NameSafe() ?? "???");
		if (Permanent)
		{
			return text + " (permanent)";
		}
		string text2 = (SameDuration ? "same duration" : DurationValue.ToString());
		return text + " (for " + text2 + ")";
	}

	public override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			PFLog.Default.Error(this, "Unable to apply buff: no context found");
			return;
		}
		MechanicEntity buffTarget = GetBuffTarget(mechanicsContext);
		if (buffTarget == null)
		{
			PFLog.Default.Error(this, "Can't apply buff: target is null");
			return;
		}
		int count = CalculateRank(mechanicsContext);
		BuffDuration duration = new BuffDuration(CalculateDuration(mechanicsContext), BuffEndCondition);
		Buff buff = buffTarget.Buffs.Add(Buff, mechanicsContext, duration);
		if (buff == null)
		{
			using (base.Context.GetDataScope(buffTarget.ToITargetWrapper()))
			{
				ActionsOnImmune?.Run();
				return;
			}
		}
		buff.AddRank(count);
		if (buff.FirstSource == null)
		{
			AreaEffectEntity areaEffectEntity = ContextData<AreaEffectContextData>.Current?.Entity;
			if (areaEffectEntity != null)
			{
				if (!(base.Caster is StarshipEntity) && buffTarget is StarshipEntity)
				{
					return;
				}
				buff.AddSource(areaEffectEntity);
			}
			else
			{
				buff.AddSource(base.Context.AssociatedBlueprint);
			}
		}
		if (AsChild)
		{
			Buff buff2 = ContextData<Kingmaker.UnitLogic.Buffs.Buff.Data>.Current?.Buff;
			if (buff2 != null)
			{
				if (buff2.Owner == buff.Owner)
				{
					buff2.StoreFact(buff);
				}
				else
				{
					PFLog.Default.Error(mechanicsContext.AssociatedBlueprint, "Parent and child buff must have one owner (" + mechanicsContext.AssociatedBlueprint.name + ")");
				}
			}
		}
		BlueprintAbility blueprintAbility = base.Context.AssociatedBlueprint as BlueprintAbility;
		if (blueprintAbility?.GetComponent<WarhammerConcentrationAbility>() != null)
		{
			base.Context.MaybeCaster?.GetOrCreate<WarhammerUnitPartConcentrationController>()?.NewEntry(buff, blueprintAbility);
		}
		using (base.Context.GetDataScope(buffTarget.ToITargetWrapper()))
		{
			ActionsOnApply?.Run();
		}
	}

	private int CalculateRank(MechanicsContext context)
	{
		if (!Buff.HasRanks)
		{
			return 0;
		}
		int num = Math.Max(Ranks?.Calculate(context) ?? 0, 1);
		if (num == 1)
		{
			return 0;
		}
		return num - 1;
	}

	public MechanicEntity GetBuffTarget(MechanicsContext context)
	{
		if (!ToCaster)
		{
			return base.Target.Entity;
		}
		return context.MaybeCaster;
	}

	private Rounds? CalculateDuration(MechanicsContext context)
	{
		if (Permanent)
		{
			return null;
		}
		if (SameDuration)
		{
			Buff buff = ContextData<Kingmaker.UnitLogic.Buffs.Buff.Data>.Current?.Buff;
			if (buff == null || !buff.IsPermanent)
			{
				return buff?.ExpirationInRounds.Rounds();
			}
			return null;
		}
		return DurationValue.Calculate(context);
	}
}
