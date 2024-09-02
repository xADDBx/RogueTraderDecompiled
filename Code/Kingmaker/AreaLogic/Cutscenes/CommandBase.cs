using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
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
	public virtual IAbstractUnitEntity GetControlledUnit()
	{
		return null;
	}

	[CanBeNull]
	public virtual IAbstractUnitEntity GetAnchorUnit()
	{
		return GetControlledUnit();
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
