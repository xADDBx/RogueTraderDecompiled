using System;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/TimeSkip(Filler)")]
[AllowMultipleComponents]
[TypeId("13d9e1df4f8f8b24d9c7e6bceb81711b")]
public class TimeSkip : GameAction
{
	public enum SkipType
	{
		Minutes,
		TimeOfDay
	}

	[SerializeField]
	private SkipType m_Type;

	[SerializeReference]
	public IntEvaluator MinutesToSkip;

	public TimeOfDay TimeOfDay;

	public bool MatchTimeOfDay;

	public SkipType Type => m_Type;

	protected override void RunAction()
	{
		switch (m_Type)
		{
		case SkipType.Minutes:
			Game.Instance.AdvanceGameTime(MinutesToSkip.GetValue().Minutes());
			break;
		case SkipType.TimeOfDay:
			_ = Game.Instance.TimeController.SkipGameTime(TimeOfDay).Hours;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (MatchTimeOfDay)
		{
			Game.Instance.MatchTimeOfDay();
		}
	}

	public override string GetCaption()
	{
		return m_Type switch
		{
			SkipType.Minutes => $"Time Skip ({MinutesToSkip})", 
			SkipType.TimeOfDay => $"Time Skip ({TimeOfDay})", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
