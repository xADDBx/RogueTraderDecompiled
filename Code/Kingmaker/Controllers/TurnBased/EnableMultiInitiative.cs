using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[Serializable]
[TypeId("62eaff56c12a477e97986b3780f11dd2")]
public class EnableMultiInitiative : MechanicEntityFactComponentDelegate, IHashable
{
	public bool TurnPerEnemy;

	[HideIf("TurnPerEnemy")]
	public int AdditionalTurns = 1;

	protected override void OnActivateOrPostLoad()
	{
		if (TurnPerEnemy)
		{
			base.Owner.GetOrCreate<PartMultiInitiative>().SetupByEnemiesCount();
		}
		else
		{
			base.Owner.GetOrCreate<PartMultiInitiative>().Setup(AdditionalTurns);
		}
		base.Owner.Initiative.Clear();
		EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
		{
			h.HandleInitiativeChanged();
		});
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<PartMultiInitiative>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
