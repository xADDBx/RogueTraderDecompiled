using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("d984fa9890f14bc7bae8d1da6f3a51bd")]
public class RemoveSelfAfterDelay : EntityFactComponentDelegate, IRoundStartHandler, ISubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int RoundsPassed;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref RoundsPassed);
			return result;
		}
	}

	[InfoBox("Wait Delay rounds before run Actions (0 means trigger on first New Round event)")]
	public int DelayInRounds;

	public void HandleRoundStart(bool isTurnBased)
	{
		if (isTurnBased && (!(base.Owner is MechanicEntity mechanicEntity) || mechanicEntity.Initiative.Empty))
		{
			SavableData savableData = RequestSavableData<SavableData>();
			savableData.RoundsPassed++;
			if (savableData.RoundsPassed == DelayInRounds + 1)
			{
				base.Owner.Facts.Remove(base.Fact);
			}
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
