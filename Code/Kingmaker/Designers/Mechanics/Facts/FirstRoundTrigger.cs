using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b0c346f020454970b8c292b5ea7f454e")]
public class FirstRoundTrigger : UnitFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnBasedModeHandler, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public bool HappenedThisCombat { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			bool val2 = HappenedThisCombat;
			result.Append(ref val2);
			return result;
		}
	}

	public ActionList Actions;

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			return;
		}
		Data data = RequestSavableData<Data>();
		if (data == null || !data.HappenedThisCombat)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.Owner))
			{
				base.Fact.RunActionInContext(Actions, base.Owner);
			}
			RequestSavableData<Data>().HappenedThisCombat = true;
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RequestSavableData<Data>().HappenedThisCombat = false;
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
