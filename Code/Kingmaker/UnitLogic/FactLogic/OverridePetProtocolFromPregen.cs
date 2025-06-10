using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("878d5976cc1b4227836fa6d3b63aa029")]
public class OverridePetProtocolFromPregen : UnitFactComponentDelegate, IPetSetupHandle, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public BlueprintItemEquipmentPetProtocolReference PetProtocolReference;

	void IPetSetupHandle.HandlePetSetup(BaseUnitEntity petEntity)
	{
		if (petEntity.Master == base.Owner && !PetProtocolReference.IsEmpty() && !Game.Instance.Player.Inventory.Contains((BlueprintItemEquipmentPetProtocol)PetProtocolReference) && petEntity.Body.PetProtocol.MaybeItem?.Blueprint != PetProtocolReference.Get())
		{
			petEntity.Body.TryInsertItem(PetProtocolReference.Get(), petEntity.Body.PetProtocol);
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
