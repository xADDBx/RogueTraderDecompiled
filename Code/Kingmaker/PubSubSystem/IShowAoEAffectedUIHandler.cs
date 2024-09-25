using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IShowAoEAffectedUIHandler : ISubscriber
{
	void HandleAoEMove(Vector3 pos, AbilityData ability);

	void HandleAoECancel();
}
