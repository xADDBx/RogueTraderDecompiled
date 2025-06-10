using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Squads;

public class PartSquad : BaseUnitPart, IUnitDeathHandler, ISubscriber, IHashable
{
	[JsonProperty]
	private string m_Id;

	private UnitSquad m_Squad;

	public bool SeparateUnitsAfterLeaderDeath;

	public UnitSquad Squad => UpdateSquad();

	public bool IsInSquad => Squad != null;

	public int Count => Squad?.Count ?? 0;

	public BaseUnitEntity this[int index] => Squad[index];

	public string Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			Drop();
			m_Id = value;
			UpdateSquad();
		}
	}

	[CanBeNull]
	public BaseUnitEntity Leader => m_Squad.Leader;

	public bool IsLeader
	{
		get
		{
			if (m_Squad != null)
			{
				return Leader == base.Owner;
			}
			return false;
		}
	}

	public ReadonlyList<UnitReference> Units => m_Squad.Units;

	protected override void OnAttachOrPostLoad()
	{
		UpdateSquad();
	}

	protected override void OnDetach()
	{
		Drop();
	}

	private UnitSquad UpdateSquad()
	{
		if (string.IsNullOrEmpty(Id))
		{
			return null;
		}
		if (m_Squad != null)
		{
			return m_Squad;
		}
		m_Squad = UnitSquad.GetOrCreate(Id);
		m_Squad.Add(base.Owner);
		return m_Squad;
	}

	private void Drop()
	{
		m_Squad?.Remove(base.Owner);
		m_Squad = null;
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity != base.Owner || m_Squad == null || !IsLeader || !SeparateUnitsAfterLeaderDeath)
		{
			return;
		}
		for (int num = m_Squad.Units.Count - 1; num >= 0; num--)
		{
			UnitReference unitReference = m_Squad.Units[num];
			if (!(unitReference == base.Owner))
			{
				unitReference.ToBaseUnitEntity().Remove<PartSquad>();
			}
		}
		m_Id = null;
		base.Owner.ToBaseUnitEntity().Remove<PartSquad>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_Id);
		return result;
	}
}
