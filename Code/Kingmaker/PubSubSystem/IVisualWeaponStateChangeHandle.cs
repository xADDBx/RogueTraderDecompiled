using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IVisualWeaponStateChangeHandle : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState weaponVisualState, GameObject visualModel);
}
