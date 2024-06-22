using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.PubSubSystem;

public interface INetPingActionBarAbility : ISubscriber
{
	void HandlePingActionBarAbility(NetPlayer player, string keyName, Entity characterEntityRef, int slotIndex, WeaponSlotType weaponSlotType);
}
