using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[AllowMultipleComponents]
[TypeId("340d225347d5002409757c2be255cd50")]
public class DeathActions : UnitFactComponentDelegate, IUnitHandler<EntitySubscriber>, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitHandler, EntitySubscriber>, IHashable
{
	public ActionList Actions;

	public bool CheckResource;

	public bool OnlyOnParty;

	[ShowIf("CheckResource")]
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath()
	{
		if ((!OnlyOnParty || Game.Instance.Player.PartyAndPets.Contains(base.Owner)) && (!CheckResource || (CheckResource && (bool)Resource && base.Owner.AbilityResources.GetResourceAmount(Resource) > 0)))
		{
			base.Fact.RunActionInContext(Actions);
			if (CheckResource && (bool)Resource)
			{
				base.Owner.AbilityResources.Spend(Resource, 1);
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
