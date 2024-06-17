using Kingmaker.Blueprints.Items.Weapons;

namespace Kingmaker.UnitLogic.ActivatableAbilities;

public interface IActivatableAbilitySpendResourceLogic
{
	void OnAbilityTurnOn();

	void OnStart();

	void OnNewRound();

	void OnAttack(BlueprintItemWeapon weapon);

	void OnHit();

	void OnCrit();

	void ManualSpendResource();
}
