using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("b787865cc7284095914908b39df0a903")]
public class AddAbilitiesToCurrentWeapon : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, AddAbilitiesToCurrentWeapon>, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			ReapplyFactsForActiveWeapons();
		}

		protected override void OnDeactivate()
		{
			ReapplyFactsForActiveWeapons();
		}

		private void ReapplyFactsForActiveWeapons()
		{
			foreach (HandSlot hand in base.Owner.Body.Hands)
			{
				if (hand.Active)
				{
					hand.MaybeWeapon?.ReapplyFactsForWielder();
				}
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private WeaponClassification m_WeaponClassification;

	[SerializeField]
	private WeaponAbility[] m_WeaponAbilities;

	public IReadOnlyList<WeaponAbility> WeaponAbilities => m_WeaponAbilities;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}

	public bool ShouldAddAbilities(ItemEntityWeapon weapon)
	{
		return weapon.Blueprint.Classification == m_WeaponClassification;
	}
}
