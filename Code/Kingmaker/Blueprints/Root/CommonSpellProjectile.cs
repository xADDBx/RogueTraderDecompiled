using System;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CommonSpellProjectile : IComparable<CommonSpellProjectile>
{
	public SpellSchool School;

	public SpellDescriptorWrapper Descriptors;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintProjectileReference m_Projectile;

	public BlueprintProjectile Projectile => m_Projectile?.Get();

	public int CompareTo(CommonSpellProjectile other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = other.Descriptors.Value.CompareTo(Descriptors.Value);
		if (num != 0)
		{
			return num;
		}
		return other.School.CompareTo(School);
	}
}
