using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSummonedMonster : BaseUnitPart, IHashable
{
	[JsonProperty]
	private EntityRef<MechanicEntity> m_Summoner;

	[CanBeNull]
	public MechanicEntity Summoner => m_Summoner;

	[CanBeNull]
	public UnitCommandHandle MoveTo { get; set; }

	public bool IsLinkedToSummoner => m_Summoner.Id != base.Owner.UniqueId;

	public void Init([NotNull] MechanicEntity summoner)
	{
		if (Summoner != null)
		{
			PFLog.Default.Error("Double initialization of UnitPartSummonedMonster");
		}
		m_Summoner = summoner;
	}

	protected override void OnPostLoad()
	{
		if (base.Owner.LifeState.IsDead)
		{
			Game.Instance.EntityDestroyer.Destroy(base.Owner);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<MechanicEntity> obj = m_Summoner;
		Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
