using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("a27392a3622347b98de33636db16f1db")]
public class PlayerHasCargoInCargoInventory : PropertyGetter
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_CargoAmount;

	private BlueprintCargo Cargo => m_Cargo.Get();

	protected override int GetBaseValue()
	{
		IEnumerable<CargoEntity> cargoEntities = Game.Instance.Player.CargoState.CargoEntities;
		if (cargoEntities.Empty())
		{
			return 0;
		}
		IEnumerable<CargoEntity> source = cargoEntities.Where((CargoEntity i) => i.Blueprint == Cargo);
		if (source.Empty())
		{
			return 0;
		}
		if (source.ToArray().Length > m_CargoAmount)
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = ((Cargo == null) ? "NULL" : (Cargo.name ?? ""));
		return "Player has " + text + " in his Cargo Inventory";
	}
}
