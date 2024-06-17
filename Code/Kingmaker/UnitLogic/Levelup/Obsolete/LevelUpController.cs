using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UnitLogic.Levelup.Obsolete;

public class LevelUpController : ICanConvertPropertiesToReactive
{
	private readonly BaseUnitEntity m_BaseUnit;

	public BaseUnitEntity Preview;

	public LevelUpState State;

	private bool m_RecalculatePreview;

	private Action m_OnCommit;

	private Action m_OnStop;

	private bool m_HasPlan;

	private bool m_PlanChanged;

	private ReactiveCommand m_UpdateCommand;

	public ReactiveCommand UpdateCommand
	{
		get
		{
			if (m_UpdateCommand == null)
			{
				m_UpdateCommand = new ReactiveCommand();
				ObservableExtensions.Subscribe(UpdateCommandOnLateUpdate.ObserveLastValueOnLateUpdate(), delegate
				{
					m_UpdateCommand.Execute();
				});
			}
			return m_UpdateCommand;
		}
	}

	private ReactiveCommand UpdateCommandOnLateUpdate { get; } = new ReactiveCommand();


	private LevelUpController([NotNull] BaseUnitEntity unit, bool autoCommit, LevelUpState.CharBuildMode mode)
	{
		throw new NotSupportedException();
	}

	public static int GetEffectiveLevel(BaseUnitEntity unit)
	{
		return unit.Progression.ExperienceLevel;
	}

	public static bool CanLevelUp(BaseUnitEntity unit)
	{
		return unit?.Progression.CanLevelUp ?? false;
	}

	public static bool NeedToSetName(BaseUnitEntity unit)
	{
		return false;
	}
}
