using System;
using System.Text;
using Code.Enums;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("984ce206d35c47f8adce3d3e47fc10ec")]
public class ContextActionApplyDOT : ContextAction
{
	public DOT Type;

	public ContextValue DamageValue;

	public BuffEndCondition EndCondition = BuffEndCondition.CombatEnd;

	public bool UseRoundsDuration;

	[ShowIf("UseRoundsDuration")]
	public Rounds RoundsDuration;

	public bool UseSavingThrowOverride;

	[ShowIf("UseSavingThrowOverride")]
	public SavingThrowType SavingThrowOverride;

	public bool UseDifficultyOverride;

	[ShowIf("UseDifficultyOverride")]
	public ContextValue DifficultyOverride;

	public bool UsePenetrationOverride;

	[ShowIf("UsePenetrationOverride")]
	public ContextValue PenetrationOverride;

	public override void RunAction()
	{
		if (!(base.Target.Entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		BlueprintBuff blueprintBuff = SelectBuff(Type);
		if (blueprintBuff == null)
		{
			return;
		}
		int damage = DamageValue.Calculate(base.Context);
		SavingThrowType? saveType = (UseSavingThrowOverride ? new SavingThrowType?(SavingThrowOverride) : null);
		int? difficulty = (UseDifficultyOverride ? new int?(DifficultyOverride.Calculate(base.Context)) : null);
		int? penetration = (UsePenetrationOverride ? new int?(PenetrationOverride.Calculate(base.Context)) : null);
		Rounds? rounds = (UseRoundsDuration ? new Rounds?(RoundsDuration) : null);
		BuffDuration duration = new BuffDuration(rounds, EndCondition);
		using (ContextData<DOTLogic.Settings>.Request().Setup(damage, saveType, difficulty, penetration))
		{
			baseUnitEntity.Buffs.Add(blueprintBuff, base.Caster, base.Context, duration)?.TryAddSource(this);
		}
	}

	[CanBeNull]
	public static BlueprintBuff SelectBuff(DOT type)
	{
		return Root.WH.CombatRoot.DOTSettings.FirstItem((BlueprintCombatRoot.DOTEntry i) => i.Type == type)?.Buff;
	}

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Apply DOT: ");
		builder.Append(DamageValue.ToString());
		builder.Append(" [");
		builder.Append(Type.ToString());
		builder.Append("]");
		if (UseRoundsDuration)
		{
			builder.Append(" for ");
			builder.Append(RoundsDuration.ToString());
			builder.Append(" rounds");
		}
		if (EndCondition != 0)
		{
			builder.Append(" before ");
			builder.Append(EndCondition.ToString());
		}
		if (!UseRoundsDuration && EndCondition == BuffEndCondition.RemainAfterCombat)
		{
			builder.Append(" permanently");
		}
		return builder.ToString();
	}
}
