using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ILastAbilityWeaponChangeHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleLastAbilityWeaponChange([CanBeNull] ItemEntityWeapon oldWeapon, [CanBeNull] ItemEntityWeapon newWeapon);
}
