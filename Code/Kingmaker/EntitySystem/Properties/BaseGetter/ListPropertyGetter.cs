using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

[Serializable]
[TypeId("60ce9a24a82c445781f21fd666f8fe6d")]
public class ListPropertyGetter : PropertyGetter
{
	private enum Operation
	{
		Sum,
		Min,
		Max,
		Avg,
		AnyZero,
		AllZero
	}

	private enum List
	{
		PCInParty,
		PCAll,
		PCRemote,
		PCDetached,
		PCEx
	}

	[SerializeField]
	private List m_List;

	[SerializeField]
	private Operation m_Operation;

	public PropertyCalculator Value;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{m_List}: {m_Operation} {Value}";
	}

	protected override int GetBaseValue()
	{
		return m_Operation switch
		{
			Operation.Sum => GetList().Sum((Func<MechanicEntity, int>)GetValue), 
			Operation.Min => GetList().Min((Func<MechanicEntity, int>)GetValue), 
			Operation.Max => GetList().Max((Func<MechanicEntity, int>)GetValue), 
			Operation.Avg => (int)GetList().Average((Func<MechanicEntity, int>)GetValue), 
			Operation.AnyZero => GetList().Any((MechanicEntity i) => GetValue(i) == 0) ? 1 : 0, 
			Operation.AllZero => (!GetList().Any((MechanicEntity i) => GetValue(i) != 0)) ? 1 : 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private int GetValue(MechanicEntity currentEntity)
	{
		return Value.GetValue(ContextData<PropertyContextData>.Current.Context.WithCurrentEntity(currentEntity));
	}

	private IEnumerable<MechanicEntity> GetList()
	{
		return m_List switch
		{
			List.PCInParty => GetCompanionList((CompanionState i) => i == CompanionState.InParty), 
			List.PCAll => GetCompanionList((CompanionState i) => i == CompanionState.InParty || i == CompanionState.InPartyDetached || i == CompanionState.Remote), 
			List.PCRemote => GetCompanionList((CompanionState i) => i == CompanionState.Remote), 
			List.PCDetached => GetCompanionList((CompanionState i) => i == CompanionState.InPartyDetached), 
			List.PCEx => GetCompanionList((CompanionState i) => i == CompanionState.ExCompanion), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private IEnumerable<MechanicEntity> GetCompanionList([NotNull] Func<CompanionState, bool> pred)
	{
		return Game.Instance.Player.AllCharacters.Where(delegate(BaseUnitEntity i)
		{
			CompanionState? companionState = i.GetCompanionOptional()?.State;
			if (companionState.HasValue)
			{
				CompanionState valueOrDefault = companionState.GetValueOrDefault();
				return pred(valueOrDefault);
			}
			return false;
		});
	}
}
