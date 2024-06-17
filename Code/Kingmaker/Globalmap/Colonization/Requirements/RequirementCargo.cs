using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("d35d0b0ef76746a5a3521dbfd22c654c")]
public class RequirementCargo : Requirement
{
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_Count = 1;

	public BlueprintCargo Cargo => m_Cargo?.Get();

	public int Count => m_Count;

	public override bool Check(Colony colony = null)
	{
		if (Cargo == null)
		{
			return true;
		}
		return Game.Instance.Player.CargoState.Get(Cargo).ToList().Count >= m_Count;
	}

	public override void Apply(Colony colony = null)
	{
		if (Cargo != null)
		{
			for (int i = 0; i < m_Count; i++)
			{
				Game.Instance.Player.CargoState.Remove(Cargo);
			}
		}
	}
}
