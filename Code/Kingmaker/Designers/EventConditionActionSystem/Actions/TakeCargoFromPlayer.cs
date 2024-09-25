using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("3daf38bcdace40f084d93677cd92dba0")]
public class TakeCargoFromPlayer : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_CargoAmount;

	private BlueprintCargo Cargo => m_Cargo.Get();

	public override string GetCaption()
	{
		string arg = ((Cargo == null) ? "NULL" : (Cargo.name ?? ""));
		return $"Takes {m_CargoAmount} {arg} from Player if he has it";
	}

	protected override void RunAction()
	{
		IEnumerable<CargoEntity> cargoEntities = Game.Instance.Player.CargoState.CargoEntities;
		if (cargoEntities.Empty())
		{
			throw new IndexOutOfRangeException("TakeCargoFromPlayer: Can't resolve, Player's cargo is empty");
		}
		IEnumerable<CargoEntity> source = cargoEntities.Where((CargoEntity i) => i.Blueprint == Cargo);
		if (source.Empty())
		{
			throw new IndexOutOfRangeException("TakeCargoFromPlayer: Player doesn't have any " + Cargo.name + " in cargo");
		}
		if (source.ToArray().Length <= m_CargoAmount)
		{
			throw new IndexOutOfRangeException("TakeCargoFromPlayer: Player doesn't have enough " + Cargo.name + " in cargo");
		}
		for (int j = 0; j < m_CargoAmount; j++)
		{
			Game.Instance.Player.CargoState.Remove(source.FirstOrDefault());
		}
	}
}
