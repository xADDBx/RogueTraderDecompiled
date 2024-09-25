using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("5515c32f67244b15a1c95d2f7628b596")]
public class AddPlayerLeaveCombatTrigger : EntityFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool InCombat;
	}

	public ActionList Actions;

	protected override void OnActivateOrPostLoad()
	{
		RequestTransientData<ComponentData>().InCombat = Game.Instance.Player.IsInCombat;
	}

	public void HandleUnitJoinCombat()
	{
		RequestTransientData<ComponentData>().InCombat |= Game.Instance.Player.IsInCombat;
	}

	public void HandleUnitLeaveCombat()
	{
		if (RequestTransientData<ComponentData>().InCombat && !Game.Instance.Player.MainCharacter.Entity.IsInCombat)
		{
			if (base.Fact.MaybeContext != null)
			{
				base.Fact.RunActionInContext(Actions);
			}
			else
			{
				Actions.Run();
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
