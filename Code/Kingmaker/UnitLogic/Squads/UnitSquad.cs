using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Squads;

public class UnitSquad : MechanicEntity, IHashable
{
	private static readonly List<UnitSquad> AllList = new List<UnitSquad>();

	private readonly List<UnitReference> m_Units = new List<UnitReference>();

	private UnitReference m_Leader;

	[JsonProperty]
	public string Id { get; private set; }

	public TargetWrapper CommonTarget { get; set; }

	public static ReadonlyList<UnitSquad> All => AllList;

	[CanBeNull]
	public BaseUnitEntity Leader
	{
		get
		{
			return m_Leader.ToBaseUnitEntity();
		}
		set
		{
			m_Leader = value.FromBaseUnitEntity();
		}
	}

	public ReadonlyList<UnitReference> Units => m_Units.Where((UnitReference x) => x.Entity.ToBaseUnitEntity().IsInGame).ToList();

	public int Count => m_Units.Count;

	[CanBeNull]
	public BaseUnitEntity this[int index] => m_Units[index].Entity.ToBaseUnitEntity();

	public override bool IsInCombat => Units.HasItem((UnitReference i) => i.Entity?.IsInCombat ?? false);

	[NotNull]
	public MechanicEntity InitiativeRoller => (MechanicEntity)(Leader ?? ((object)m_Units.FirstItem((UnitReference i) => i.Entity != null).Entity.ToBaseUnitEntity()) ?? ((object)this));

	public static UnitSquad GetOrCreate(string id)
	{
		return AllList.FirstItem((UnitSquad s) => s.Id.Equals(id)) ?? Entity.Initialize(new UnitSquad(id));
	}

	private UnitSquad(string id)
		: base(id, isInGame: true, BlueprintRoot.Instance.SystemMechanics.EmptyMechanicEntity)
	{
		Id = id;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		AllList.Add(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AllList.Remove(this);
	}

	public void Add(BaseUnitEntity unit)
	{
		if (string.IsNullOrEmpty(Id) || unit.GetSquadOptional()?.Id != Id)
		{
			return;
		}
		UnitReference item = unit.FromBaseUnitEntity();
		if (m_Units.Contains(item))
		{
			PFLog.Default.Error($"Squad already contains unit: {unit}");
			return;
		}
		if (base.Initiative.Empty)
		{
			base.Initiative.Value = unit.Initiative.Value;
			base.Initiative.LastTurn = unit.Initiative.LastTurn;
		}
		m_Units.Add(item);
	}

	public void Remove(BaseUnitEntity unit)
	{
		UnitReference item = unit.FromBaseUnitEntity();
		m_Units.Remove(item);
		if (unit == Leader)
		{
			Leader = null;
		}
		if (m_Units.Empty())
		{
			HandleDestroy();
			Dispose();
		}
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartUnitBrain>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(Id);
		return result;
	}
}
