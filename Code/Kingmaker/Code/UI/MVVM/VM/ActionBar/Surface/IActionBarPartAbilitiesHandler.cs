using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public interface IActionBarPartAbilitiesHandler : ISubscriber
{
	void MoveSlot(Ability sourceAbility, int targetIndex);

	void MoveSlot(MechanicActionBarSlot sourceSlot, int sourceIndex, int targetIndex);

	void DeleteSlot(int sourceIndex);

	void ChooseAbilityToSlot(int targetIndex);

	void SetMoveAbilityMode(bool on);
}
