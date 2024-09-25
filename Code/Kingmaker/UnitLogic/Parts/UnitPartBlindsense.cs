using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartBlindsense : BaseUnitPart, IHashable
{
	private readonly List<Feet> m_Ranges = new List<Feet>();

	public Feet Range { get; private set; }

	public void Add(Feet range)
	{
		m_Ranges.Add(range);
		UpdateRange();
	}

	public void Remove(Feet range)
	{
		m_Ranges.Remove(range);
		if (m_Ranges.Count == 0)
		{
			base.Owner.Remove<UnitPartBlindsense>();
		}
		else
		{
			UpdateRange();
		}
	}

	private void UpdateRange()
	{
		Range = 0.Feet();
		foreach (Feet range in m_Ranges)
		{
			Range = Math.Max(Range.Value, range.Value).Feet();
		}
	}

	public bool Reach(BaseUnitEntity unit)
	{
		float num = base.Owner.Corpulence + unit.Corpulence;
		if (base.Owner.DistanceTo(unit) - num <= Range.Meters)
		{
			return base.Owner.Vision.HasLOS(unit);
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
