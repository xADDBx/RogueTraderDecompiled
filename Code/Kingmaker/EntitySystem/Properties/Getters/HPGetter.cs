using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("9cb6f49c104fb044db79e499b6fcd002")]
public class HPGetter : MechanicEntityPropertyGetter
{
	public enum HPType
	{
		BaseHP,
		TemporaryHP,
		Sum
	}

	public bool HPPercent;

	public bool MaxHP;

	public HPType HealthType;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!HPPercent)
		{
			if (!MaxHP)
			{
				return "Hit Points of " + FormulaTargetScope.Current;
			}
			return "Maximum Hit Points of " + FormulaTargetScope.Current;
		}
		return "Hit Points percent of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		PartHealth healthOptional = base.CurrentEntity.GetHealthOptional();
		if (healthOptional == null)
		{
			return 0;
		}
		int num = 0;
		if (MaxHP)
		{
			return healthOptional.MaxHitPoints;
		}
		switch (HealthType)
		{
		case HPType.BaseHP:
			num = healthOptional.HitPointsLeft;
			break;
		case HPType.TemporaryHP:
			num = healthOptional.TemporaryHitPoints;
			break;
		case HPType.Sum:
			num = healthOptional.HitPointsLeft + healthOptional.TemporaryHitPoints;
			break;
		default:
			return 0;
		}
		if (HPPercent)
		{
			num = (int)Math.Floor((float)num * 100f / (float)healthOptional.MaxHitPoints);
		}
		return num;
	}
}
