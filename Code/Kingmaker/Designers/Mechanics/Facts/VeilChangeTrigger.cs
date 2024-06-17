using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("25e8cd92c4864e0eab9af83d27a65b52")]
public class VeilChangeTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleCalculateVeilCount>, IRulebookHandler<RuleCalculateVeilCount>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public int OldVeil { get; set; }
	}

	public ActionList ActionsOnVeilChange;

	public ActionList ActionsOnMoreVeil;

	public ActionList ActionsOnLessVeil;

	public ActionList ActionsOnUnchaingedVeil;

	public bool AssingVeilChangeInitiatorAsTarget;

	public void OnEventAboutToTrigger(RuleCalculateVeilCount evt)
	{
		RequestTransientData<ComponentData>().OldVeil = Game.Instance.TurnController.VeilThicknessCounter.Value;
	}

	public void OnEventDidTrigger(RuleCalculateVeilCount evt)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (evt.Result == componentData.OldVeil)
		{
			base.Fact.RunActionInContext(ActionsOnUnchaingedVeil, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
			return;
		}
		base.Fact.RunActionInContext(ActionsOnVeilChange, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
		if (evt.Result > componentData.OldVeil)
		{
			base.Fact.RunActionInContext(ActionsOnMoreVeil, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
		}
		if (evt.Result < componentData.OldVeil)
		{
			base.Fact.RunActionInContext(ActionsOnLessVeil, AssingVeilChangeInitiatorAsTarget ? evt.InitiatorUnit : base.Owner);
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
