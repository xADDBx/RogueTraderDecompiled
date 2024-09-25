using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("41eebb0155a1c8b4da2a075f27e56850")]
public class CommandMarkPartyHiddenSpan : CommandBase
{
	[SerializeField]
	private Player.CharactersList m_UnitsList;

	public bool NoFadeOut;

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
				characters?.Features.Hidden.Retain();
				if (NoFadeOut && characters?.View?.Fader != null)
				{
					characters.View.Fader.Visible = false;
					characters.View.Fader.FastForward();
				}
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
				characters?.Features.Hidden.Release();
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
		return "Mark party <b>hidden</b>";
	}
}
