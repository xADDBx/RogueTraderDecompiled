using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartPropheticIntervention : BaseUnitPart, IHashable
{
	public class PropheticInterventionEntry : IHashable
	{
		[JsonProperty]
		private EntityRef<UnitEntity> m_DeadTargetRef;

		[JsonProperty]
		private Vector3 m_OldPosition;

		[JsonProperty]
		private bool m_IsOldPositionSet;

		[JsonProperty(PropertyName = "DeadTarget")]
		private UnitEntity m_DeadTarget;

		[JsonProperty]
		public int WoundsBeforeLastAttack { get; set; }

		public UnitEntity DeadTarget
		{
			get
			{
				return m_DeadTargetRef;
			}
			set
			{
				m_DeadTargetRef = value;
			}
		}

		public CustomGridNodeBase OldPosition
		{
			[CanBeNull]
			get
			{
				if (!m_IsOldPositionSet)
				{
					return null;
				}
				return m_OldPosition.GetNearestNodeXZUnwalkable();
			}
			set
			{
				m_OldPosition = value.Vector3Position;
				m_IsOldPositionSet = true;
			}
		}

		public void OnPostLoad()
		{
			if (m_DeadTarget != null)
			{
				m_DeadTargetRef = new EntityRef<UnitEntity>(m_DeadTarget);
				m_DeadTarget = null;
				PFLog.Default.Log($"Convert DeadTarget property to ref. DeadTarget={DeadTarget}");
			}
			if (!m_IsOldPositionSet && m_DeadTargetRef.Entity != null)
			{
				m_OldPosition = m_DeadTargetRef.Entity.Position;
				m_IsOldPositionSet = true;
				PFLog.Default.Log($"Convert OldPosition property to Vector3. Defaulting to DeadTarget position {m_OldPosition}. DeadTarget={DeadTarget}");
			}
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityRef<UnitEntity> obj = m_DeadTargetRef;
			Hash128 val = StructHasher<EntityRef<UnitEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref m_OldPosition);
			result.Append(ref m_IsOldPositionSet);
			int val2 = WoundsBeforeLastAttack;
			result.Append(ref val2);
			Hash128 val3 = ClassHasher<UnitEntity>.GetHash128(m_DeadTarget);
			result.Append(ref val3);
			return result;
		}
	}

	[JsonProperty]
	public List<PropheticInterventionEntry> Entries = new List<PropheticInterventionEntry>();

	public void NewEntry(UnitEntity deadTarget, int woundsBeforeLastAttack, CustomGridNodeBase oldPosition)
	{
		PropheticInterventionEntry item = new PropheticInterventionEntry
		{
			DeadTarget = deadTarget,
			WoundsBeforeLastAttack = woundsBeforeLastAttack,
			OldPosition = oldPosition
		};
		Entries.Add(item);
	}

	public void RemoveEntry(UnitEntity deadTarget)
	{
		Entries.RemoveAll((PropheticInterventionEntry p) => p.DeadTarget == deadTarget);
	}

	public bool HasEntry(UnitEntity deadTarget)
	{
		return Entries.Any((PropheticInterventionEntry p) => p.DeadTarget == deadTarget);
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		foreach (PropheticInterventionEntry entry in Entries)
		{
			entry.OnPostLoad();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<PropheticInterventionEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<PropheticInterventionEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
