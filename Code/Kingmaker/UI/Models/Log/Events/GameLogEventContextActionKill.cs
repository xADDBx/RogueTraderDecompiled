using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventContextActionKill : GameLogEvent<GameLogEventContextActionKill>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUIContextActionKillHandler, ISubscriber
	{
		public void HandleOnContextActionKill(MechanicEntity caster, MechanicEntity target, BlueprintMechanicEntityFact blueprint, RulePerformSavingThrow rule)
		{
			if (caster is BaseUnitEntity caster2 && target is BaseUnitEntity baseUnitEntity && blueprint != null)
			{
				AddEvent(new GameLogEventContextActionKill(caster2, baseUnitEntity, blueprint, baseUnitEntity.Health.HitPointsLeft, rule));
			}
		}
	}

	public readonly UnitReference Caster;

	public readonly UnitReference Target;

	public readonly BlueprintMechanicEntityFact Blueprint;

	[CanBeNull]
	public readonly RulePerformSavingThrow RulePerformSavingThrow;

	public readonly int Damage;

	private GameLogEventContextActionKill(IAbstractUnitEntity caster, IAbstractUnitEntity target, BlueprintMechanicEntityFact blueprint, int damage, RulePerformSavingThrow rule)
	{
		Caster = UnitReference.FromIAbstractUnitEntity(caster);
		Target = UnitReference.FromIAbstractUnitEntity(target);
		Blueprint = blueprint;
		Damage = damage;
		RulePerformSavingThrow = rule;
	}
}
