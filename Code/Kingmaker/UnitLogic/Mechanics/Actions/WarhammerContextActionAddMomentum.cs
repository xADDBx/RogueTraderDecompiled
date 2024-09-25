using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("bd0e309dfe894411be45495422163e9c")]
public class WarhammerContextActionAddMomentum : ContextAction
{
	public int Multiplier;

	public ContextValue Value;

	public bool ToPlayer;

	[HideIf("ToPlayer")]
	public bool ToCaster;

	[HideIf("ToPlayer")]
	public AdditionalCalculations AdditionalCalculations;

	[ShowIf("HasVariableRange")]
	public int Range;

	[ShowIf("HasVariableAdditionalBonus")]
	public ContextValue AdditionalBonus;

	private bool HasVariableRange
	{
		get
		{
			if (!ToPlayer)
			{
				return AdditionalCalculations == AdditionalCalculations.BonusForEnemiesInRange;
			}
			return false;
		}
	}

	private bool HasVariableAdditionalBonus
	{
		get
		{
			if (!ToPlayer)
			{
				return AdditionalCalculations == AdditionalCalculations.BonusForEnemiesInRange;
			}
			return false;
		}
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.MaybeCaster;
		MechanicEntity mechanicEntity = (ToPlayer ? Game.Instance.Player.MainCharacterEntity : (ToCaster ? caster : base.Target.Entity));
		if (caster == null || mechanicEntity == null)
		{
			return;
		}
		int num = Value.Calculate(base.Context) * Multiplier;
		if (!ToPlayer && AdditionalCalculations == AdditionalCalculations.BonusForEnemiesInRange)
		{
			num += Game.Instance.State.AllUnits.Count((AbstractUnitEntity p) => p.DistanceToInCells(caster) <= Range && p.IsEnemy(caster)) * AdditionalBonus.Calculate(base.Context);
		}
		int value = num;
		RuleReason reason = base.Context;
		Rulebook.Trigger(RulePerformMomentumChange.CreateCustom(mechanicEntity, value, in reason));
	}

	public override string GetCaption()
	{
		return "Adds momentum";
	}
}
