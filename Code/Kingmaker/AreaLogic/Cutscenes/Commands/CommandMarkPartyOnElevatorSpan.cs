using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("6e990ca6d8d542743ac9c572e869c9d7")]
public class CommandMarkPartyOnElevatorSpan : CommandBase
{
	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(ExceptThese))
		{
			if (!(item2.GetValue() is BaseUnitEntity item))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {item2} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (!list.Contains(characters))
			{
				characters?.Features.OnElevator.Retain();
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(ExceptThese))
		{
			if (!(item2.GetValue() is BaseUnitEntity item))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {item2} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (!list.Contains(characters))
			{
				characters?.Features.OnElevator.Release();
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "Mark party <b>on elevator</b>";
	}
}
