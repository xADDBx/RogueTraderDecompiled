using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Formations;

public class PartyFormationAuto : IPartyFormation, IImmutablePartyFormation
{
	private readonly List<KeyValuePair<EntityRef<AbstractUnitEntity>, Vector2>> m_Positions = new List<KeyValuePair<EntityRef<AbstractUnitEntity>, Vector2>>();

	public bool InvalidTank { get; set; }

	public bool Dirty { get; set; }

	public float Length
	{
		get
		{
			Vector2 value = m_Positions.Get(0).Value;
			float num = 0f;
			for (int i = 1; i < m_Positions.Count; i++)
			{
				float sqrMagnitude = (value - m_Positions[i].Value).sqrMagnitude;
				num = Math.Max(num, sqrMagnitude);
			}
			return num;
		}
	}

	public AbstractUnitEntity Tank => m_Positions.FirstItem().Key;

	public Vector2 GetOffset(int _, AbstractUnitEntity unit)
	{
		KeyValuePair<EntityRef<AbstractUnitEntity>, Vector2> keyValuePair = m_Positions.FirstItem((KeyValuePair<EntityRef<AbstractUnitEntity>, Vector2> i) => i.Key == unit);
		if (keyValuePair.Key == null)
		{
			PFLog.Default.ErrorWithReport($"PartyFormationAuto.GetOffset: {unit} not in formation");
		}
		return keyValuePair.Value;
	}

	public void SetOffset(int _, AbstractUnitEntity unit, Vector2 pos)
	{
		KeyValuePair<EntityRef<AbstractUnitEntity>, Vector2> keyValuePair = new KeyValuePair<EntityRef<AbstractUnitEntity>, Vector2>(unit, pos);
		for (int i = 0; i < m_Positions.Count; i++)
		{
			if (m_Positions[i].Key == unit)
			{
				m_Positions[i] = keyValuePair;
				return;
			}
		}
		m_Positions.Add(keyValuePair);
	}

	public void SetOffset(AbstractUnitEntity unit, Vector2 pos)
	{
		SetOffset(0, unit, pos);
	}

	public void Clear()
	{
		InvalidTank = false;
		m_Positions.Clear();
	}
}
