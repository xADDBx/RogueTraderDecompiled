using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Warhammer.SpaceCombat.StarshipLogic;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("292931bf836afcd4a801b15afc72d51c")]
public class StarshipSoulmarksLogic : UnitFactComponentDelegate, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnBasedModeHandler, IRoundStartHandler, ITurnStartHandler, IHashable
{
	public enum MarkMode
	{
		Chaos,
		Faith
	}

	[SerializeField]
	private MarkMode mode;

	[SerializeField]
	[ShowIf("IsFaith")]
	private int healRound;

	[SerializeField]
	[ShowIf("IsFaith")]
	private int healPct;

	[SerializeField]
	private ActionList Actions;

	public bool IsFaith => mode == MarkMode.Faith;

	private StarShipUnitPartDamageCounter DamageCounter => base.Owner.GetOptional<StarShipUnitPartDamageCounter>();

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (mode == MarkMode.Chaos && context.MaybeCaster == base.Owner && context.AbilityBlueprint.GetComponent<AbilityCustomStarshipRam>() != null)
		{
			using (ContextData<MechanicsContext.Data>.Request().Setup(context, context.Caster))
			{
				Actions.Run();
			}
		}
	}

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<StarShipUnitPartDamageCounter>();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<StarShipUnitPartDamageCounter>();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased && mode == MarkMode.Faith)
		{
			DamageCounter.NewCombat();
		}
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		if (isTurnBased && mode == MarkMode.Faith)
		{
			DamageCounter.NewRound();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased || mode != MarkMode.Faith || EventInvokerExtensions.MechanicEntity != base.Owner || DamageCounter.RoundsCounted != healRound)
		{
			return;
		}
		int num = DamageCounter.GetDamageFromStart() * healPct / 100;
		if (num > 0)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
			}
			Rulebook.Trigger(new RuleHealDamage(base.Owner, base.Owner, num));
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
