using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Alignments;

public class PartUnitSoulMark : BaseUnitPart, IHashable
{
	public class DelayedSoulMarkShift : IHashable
	{
		[JsonProperty]
		public SoulMarkDirection Direction;

		[JsonProperty]
		public int Value;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref Direction);
			result.Append(ref Value);
			return result;
		}
	}

	[JsonProperty]
	[CanBeNull]
	private List<DelayedSoulMarkShift> m_DelayedShifts = new List<DelayedSoulMarkShift>();

	public int GetDelayedShiftValue(SoulMarkDirection direction)
	{
		if (m_DelayedShifts == null || m_DelayedShifts.All((DelayedSoulMarkShift shift) => shift.Direction != direction))
		{
			return 0;
		}
		return m_DelayedShifts.First((DelayedSoulMarkShift shift) => shift.Direction == direction).Value;
	}

	public void AddDelayedShiftValue(SoulMarkDirection direction, int value)
	{
		if (m_DelayedShifts == null)
		{
			m_DelayedShifts = new List<DelayedSoulMarkShift>();
			m_DelayedShifts.Add(new DelayedSoulMarkShift
			{
				Direction = direction,
				Value = value
			});
			return;
		}
		DelayedSoulMarkShift delayedSoulMarkShift = m_DelayedShifts.FirstOrDefault((DelayedSoulMarkShift shift) => shift.Direction == direction);
		if (delayedSoulMarkShift == null)
		{
			m_DelayedShifts.Add(new DelayedSoulMarkShift
			{
				Direction = direction,
				Value = value
			});
		}
		else
		{
			delayedSoulMarkShift.Value += value;
		}
	}

	public void FreeDelayedShift(SoulMarkDirection direction)
	{
		if (m_DelayedShifts != null && !m_DelayedShifts.All((DelayedSoulMarkShift shift) => shift.Direction != direction))
		{
			m_DelayedShifts.First((DelayedSoulMarkShift shift) => shift.Direction == direction).Value = 0;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<DelayedSoulMarkShift> delayedShifts = m_DelayedShifts;
		if (delayedShifts != null)
		{
			for (int i = 0; i < delayedShifts.Count; i++)
			{
				Hash128 val2 = ClassHasher<DelayedSoulMarkShift>.GetHash128(delayedShifts[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
