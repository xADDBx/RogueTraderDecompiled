using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("8b0d4441623a3634586b16435a8da106")]
public abstract class CommandBase : ElementsScriptableObject, IEvaluationErrorHandlingPolicyHolder
{
	public enum EntryFailResult
	{
		RemoveTrack,
		FinishTrack,
		SkipCommand
	}

	[Serializable]
	public class CommandSignalData
	{
		[InspectorReadOnly]
		public string Name;

		[InspectorReadOnly]
		[SerializeField]
		[FormerlySerializedAs("Gate")]
		private GateReference m_Gate;

		public Gate Gate
		{
			get
			{
				return m_Gate?.Get();
			}
			set
			{
				m_Gate = value.ToReference<GateReference>();
			}
		}
	}

	protected const int TakingToLongDefaultSeconds = 20;

	public ConditionsChecker EntryCondition;

	[InfoBox("Что происходит, если EntryCondition не выполнена:\n• RemoveTrack — трек молча удаляется, сигнал на конечный гейт не отправляется. Использовать только когда несколько параллельных треков ведут в один OR-гейт (гейт сработает от другого трека). ВЫЗЫВАЕТ ДЕДЛОК с AND-гейтами!\n• FinishTrack — трек принудительно останавливается и отправляет сигнал на конечный гейт, как будто отработал до конца. Безопасен для AND-гейтов, но немедленно активирует OR-гейт (может оборвать параллельные треки).\n• SkipCommand — пропускается только эта команда, трек продолжает со следующей.")]
	[ShowIf("HasConditions")]
	public EntryFailResult OnFail;

	[SerializeField]
	private EvaluationErrorHandlingPolicy m_EvaluationErrorHandlingPolicy;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy => m_EvaluationErrorHandlingPolicy;

	public bool HasConditions
	{
		get
		{
			if (EntryCondition != null)
			{
				return EntryCondition.HasConditions;
			}
			return false;
		}
	}

	public virtual bool IsContinuous => false;

	protected virtual AbstractUnitEvaluator ControlledUnitEvaluator => null;

	protected abstract void OnRun(CutscenePlayerData player, bool skipping);

	protected abstract void OnSetTime(double time, CutscenePlayerData player);

	public abstract bool IsFinished(CutscenePlayerData player);

	public virtual string GetCaption()
	{
		return GetType().Name;
	}

	public virtual CommandSignalData[] GetExtraSignals()
	{
		return null;
	}

	public virtual string GetWarning()
	{
		return null;
	}

	public virtual bool TrySkip(CutscenePlayerData player)
	{
		return !IsContinuous;
	}

	protected virtual void OnStop(CutscenePlayerData player)
	{
	}

	protected virtual bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		return true;
	}

	[CanBeNull]
	public virtual IEnumerable<AbstractUnitEntity> GetControlledUnits()
	{
		return UnitsFromEvaluator(ControlledUnitEvaluator);
	}

	protected static IEnumerable<AbstractUnitEntity> UnitsFromEvaluator(AbstractUnitEvaluator evaluator)
	{
		if ((bool)evaluator && evaluator.TryGetValue(out var value))
		{
			yield return value;
		}
	}

	public virtual IEnumerable<IEntity> GetAnchorEntities()
	{
		return GetControlledUnits();
	}

	public void Run(CutscenePlayerData player, bool skipping = false)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnRun(player, skipping);
			}
			catch (Exception e)
			{
				OnRunException();
				throw new FailedToRunCutsceneCommandException(player, this, e);
			}
		}
	}

	public void SetTime(double time, CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnSetTime(time, player);
			}
			catch (Exception e)
			{
				throw new FailedToTickCutsceneCommandException(player, this, e);
			}
		}
	}

	public void Stop(CutscenePlayerData player)
	{
		using (player.GetDataScope())
		{
			try
			{
				OnStop(player);
			}
			catch (Exception e)
			{
				throw new FailedToStopCutsceneCommandException(player, this, e);
			}
		}
	}

	public virtual void Interrupt(CutscenePlayerData player)
	{
	}

	public virtual bool TryPrepareForStop(CutscenePlayerData player)
	{
		if (IsContinuous || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	protected virtual void OnRunException()
	{
	}
}
