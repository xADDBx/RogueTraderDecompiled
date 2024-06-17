using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[TypeId("297e255e4286ac747a6a27d405f579d4")]
public class BlueprintShieldType : BlueprintArmorType
{
	[SerializeField]
	private WeaponVisualParameters m_HandVisualParameters;

	public WeaponVisualParameters HandVisualParameters => m_HandVisualParameters;
}
