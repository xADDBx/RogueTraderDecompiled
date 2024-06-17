using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Cargo;

[AllowedOn(typeof(BlueprintCargo))]
[AllowMultipleComponents]
[TypeId("3b132dd1bc5e46559f7fa8d38ec622f5")]
public class AddFactToCargoOwnerShip : BlueprintComponent
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();
}
