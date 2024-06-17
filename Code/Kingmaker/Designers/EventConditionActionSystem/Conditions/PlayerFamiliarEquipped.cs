using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("9e5c08d53b134164a66a585f2c7fedfd")]
public class PlayerFamiliarEquipped : AbstractFamiliarEquipped
{
	[SerializeField]
	private BlueprintUnit.Reference m_Blueprint;

	public new BlueprintUnit Unit => m_Blueprint?.Get();

	protected override BaseUnitEntity Leader => Game.Instance.Player.MainCharacterEntity;

	protected override string GetConditionCaption()
	{
		if (Unit != null)
		{
			return $"Player has equipped {Unit} familiar";
		}
		return "Player has no equipped familiar";
	}
}
