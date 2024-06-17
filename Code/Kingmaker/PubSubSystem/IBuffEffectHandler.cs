using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IBuffEffectHandler : ISubscriber
{
	GameObject[] OnBuffEffectApplied(IBuff buff);

	void OnBuffEffectRemoved(IBuff buff);
}
