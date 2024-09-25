using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IAbilitySoundZoneTrigger : ISubscriber
{
	void TriggerSoundZone(MechanicsContext context, GameObject gameObject);
}
