using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSaddled : BaseUnitPart, IHashable
{
	[JsonProperty]
	private EntityPartRef<BaseUnitEntity, UnitPartRider> m_RiderRef;

	public BaseUnitEntity Rider => m_RiderRef.Entity;

	public void Initialize([NotNull] UnitPartRider rider)
	{
		m_RiderRef = rider;
	}

	public void Clear()
	{
		m_RiderRef = default(EntityPartRef<BaseUnitEntity, UnitPartRider>);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityPartRef<BaseUnitEntity, UnitPartRider> obj = m_RiderRef;
		Hash128 val2 = StructHasher<EntityPartRef<BaseUnitEntity, UnitPartRider>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
