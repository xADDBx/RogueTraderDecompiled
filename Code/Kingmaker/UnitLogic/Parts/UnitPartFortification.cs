using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFortification : BaseUnitPart, IHashable
{
	private readonly List<int> m_Values = new List<int>();

	public int Value
	{
		get
		{
			int num = 0;
			foreach (int value in m_Values)
			{
				num = Math.Max(num, value);
			}
			return num;
		}
	}

	public void Add(int value)
	{
		m_Values.Add(value);
	}

	public void Remove(int value)
	{
		m_Values.Remove(value);
		if (m_Values.Empty())
		{
			base.Owner.Remove<UnitPartFortification>();
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
