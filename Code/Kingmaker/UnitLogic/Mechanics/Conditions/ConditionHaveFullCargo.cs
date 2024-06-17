using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("b3b94398160449b89bb3382f426c8518")]
public class ConditionHaveFullCargo : Condition
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

	public ItemsItemOrigin Origin => m_OriginType;

	protected override string GetConditionCaption()
	{
		if (!m_ByBlueprint)
		{
			return $"{m_OriginType}";
		}
		return $"{Cargo}";
	}

	protected override bool CheckCondition()
	{
		if (!m_ByBlueprint)
		{
			return Game.Instance.Player.CargoState.Get(m_OriginType).Any((CargoEntity x) => x.IsFull);
		}
		if (Cargo != null)
		{
			return Game.Instance.Player.CargoState.Get(Cargo).Any((CargoEntity x) => x.IsFull);
		}
		PFLog.Default.Error("Cargo is not set");
		return false;
	}
}
