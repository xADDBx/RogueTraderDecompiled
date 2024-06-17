using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface ICellAbilityHandler : ISubscriber
{
	void HandleCellAbility(List<AbilityTargetUIData> abilityTargets);
}
