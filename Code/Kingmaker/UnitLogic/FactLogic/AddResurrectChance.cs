using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("47f44a33d13b4aab8e2a36070bc24f5f")]
public class AddResurrectChance : UnitFactComponentDelegate, IHashable
{
	public class ResurrectChanceUnitPart : BaseUnitPart, IHashable
	{
		public int ResurrectChance;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private ContextValue m_ResurrectChance;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<ResurrectChanceUnitPart>().ResurrectChance = m_ResurrectChance.Calculate(base.Context);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<ResurrectChanceUnitPart>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
