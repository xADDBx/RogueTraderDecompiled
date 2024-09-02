using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Warhammer.SpaceCombat;

public interface IWeaponSlotHandler : ISubscriber
{
	void HandleActiveWeaponIndexChanged(WeaponSlot weaponSlot);
}
