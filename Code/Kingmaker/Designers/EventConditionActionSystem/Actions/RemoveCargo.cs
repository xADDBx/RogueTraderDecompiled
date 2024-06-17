using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("44344b3713d94f1d93f6397345c67794")]
[PlayerUpgraderAllowed(false)]
public class RemoveCargo : GameAction
{
	[SerializeField]
	[ShowIf("ByBlueprint")]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	[HideIf("ByBlueprint")]
	private ItemsItemOrigin m_OriginType;

	[SerializeField]
	private bool m_ByBlueprint;

	public bool ByBlueprint => m_ByBlueprint;

	public BlueprintCargo Cargo => m_Cargo?.Get();

	public override string GetCaption()
	{
		if (!m_ByBlueprint)
		{
			return $"Remove cargo {m_OriginType}";
		}
		return $"Remove cargo {Cargo}";
	}

	public override void RunAction()
	{
		if (m_ByBlueprint)
		{
			if (Cargo == null)
			{
				PFLog.Default.Error("Cargo is not set");
			}
			else
			{
				Game.Instance.Player.CargoState.Remove(Cargo);
			}
			return;
		}
		CargoEntity cargoEntity = Game.Instance.Player.CargoState.Get(m_OriginType, (CargoEntity entity) => entity.IsFull).FirstOrDefault();
		if (cargoEntity == null)
		{
			PFLog.Default.Error($"No full cargo with type {m_OriginType} to remove");
		}
		else
		{
			Game.Instance.Player.CargoState.Remove(cargoEntity);
		}
	}
}
