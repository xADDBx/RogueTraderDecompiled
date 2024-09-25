using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartActivatableAbility : BaseUnitPart, IHashable
{
	private class ActivatedWithCommandData
	{
		public TimeSpan Time;

		public ActivatableAbility Ability;

		public HashSet<ActivatableAbility> Activated;

		public int GroupSize;

		public bool IsExpired
		{
			get
			{
				if ((Activated?.Count ?? 0) + 1 < GroupSize)
				{
					return Game.Instance.TimeController.GameTime - Time >= 1.Rounds().Seconds;
				}
				return true;
			}
		}

		public bool TryActivate(ActivatableAbility ability)
		{
			if (!IsExpired && Ability.IsActivateWithSameCommand(ability))
			{
				if (ability != Ability)
				{
					Activated = Activated ?? new HashSet<ActivatableAbility>();
					Activated.Add(ability);
				}
				return true;
			}
			return false;
		}

		public bool CanActivate(ActivatableAbility ability)
		{
			if (!IsExpired)
			{
				return Ability.IsActivateWithSameCommand(ability);
			}
			return false;
		}
	}

	private int[] m_GroupsSizeIncreases = new int[EnumUtils.GetMaxValue<ActivatableAbilityGroup>()];

	private readonly List<ActivatedWithCommandData> m_ActivatedWithCommand = new List<ActivatedWithCommandData>();

	public void IncreaseGroupSize(ActivatableAbilityGroup group)
	{
		m_GroupsSizeIncreases[(int)group]++;
	}

	public void DecreaseGroupSize(ActivatableAbilityGroup group)
	{
		m_GroupsSizeIncreases[(int)group]--;
	}

	public int GetGroupSize(ActivatableAbilityGroup group)
	{
		return m_GroupsSizeIncreases[(int)group] + 1;
	}

	public void OnActivatedWithCommand(ActivatableAbility ability)
	{
		int groupSize = GetGroupSize(ability.Blueprint.Group);
		if (groupSize >= 2 && ability.Blueprint.Group == ActivatableAbilityGroup.Judgment)
		{
			ActivatedWithCommandData activatedWithCommandData = m_ActivatedWithCommand.FirstItem((ActivatedWithCommandData i) => i.Ability == ability);
			if (activatedWithCommandData == null)
			{
				activatedWithCommandData = new ActivatedWithCommandData
				{
					Ability = ability
				};
				m_ActivatedWithCommand.Add(activatedWithCommandData);
			}
			activatedWithCommandData.Time = Game.Instance.TimeController.GameTime;
			activatedWithCommandData.Activated?.Clear();
			activatedWithCommandData.GroupSize = groupSize;
		}
	}

	public bool TryActivateWithoutCommand(ActivatableAbility ability)
	{
		foreach (ActivatedWithCommandData item in m_ActivatedWithCommand)
		{
			if (item.TryActivate(ability))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanActivateWithoutCommand(ActivatableAbility ability)
	{
		foreach (ActivatedWithCommandData item in m_ActivatedWithCommand)
		{
			if (item.CanActivate(ability))
			{
				return true;
			}
		}
		return false;
	}

	public void Update()
	{
		m_ActivatedWithCommand.RemoveAll((ActivatedWithCommandData d) => d.IsExpired);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
