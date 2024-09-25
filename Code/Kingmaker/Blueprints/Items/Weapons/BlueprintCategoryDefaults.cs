using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Weapons;

[TypeId("238c275f9755b7a4db43bd59772029d4")]
public class BlueprintCategoryDefaults : BlueprintScriptableObject
{
	[Serializable]
	public class CategoryDefaultEntry
	{
		public WeaponCategory Key;

		[SerializeField]
		[FormerlySerializedAs("DefaultWeapon")]
		private BlueprintItemWeaponReference m_DefaultWeapon;

		public BlueprintItemWeapon DefaultWeapon => m_DefaultWeapon?.Get();
	}

	public CategoryDefaultEntry[] Entries;
}
