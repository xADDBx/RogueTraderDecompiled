using System.Collections.Generic;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class NullifyInformation
{
	public class BuffInformation
	{
		public readonly Sprite Icon;

		public readonly string Name;

		public static BuffInformation Create(BlueprintBuff data)
		{
			return new BuffInformation(data);
		}

		private BuffInformation()
		{
		}

		private BuffInformation(IUIDataProvider data)
		{
			Icon = data.Icon;
			Name = data.Name;
		}
	}

	private int m_DamageChance;

	private readonly List<BuffInformation> m_NullifyBuffList = new List<BuffInformation>();

	public bool HasDamageNullify { get; set; }

	public bool HasDamageChance { get; private set; }

	public int DamageChance
	{
		get
		{
			return m_DamageChance;
		}
		set
		{
			HasDamageChance = true;
			m_DamageChance = value;
		}
	}

	public int DamageNegationRoll { get; set; }

	public IReadOnlyList<BuffInformation> NullifyBuffList => m_NullifyBuffList;

	public static NullifyInformation Create()
	{
		return new NullifyInformation();
	}

	private NullifyInformation()
	{
		HasDamageNullify = false;
		m_DamageChance = 0;
		HasDamageChance = false;
		DamageNegationRoll = 0;
	}

	public void AddNullifyBuff(Buff buff)
	{
		if (buff != null)
		{
			m_NullifyBuffList.Add(BuffInformation.Create(buff.Blueprint));
		}
	}
}
