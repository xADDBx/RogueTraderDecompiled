using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.Timer;

public class PlayerTimer : ITimer, IHashable
{
	public enum ScopeType
	{
		Area,
		Combat,
		CombatTurn
	}

	[JsonProperty]
	private readonly float m_Duration;

	[JsonProperty]
	private readonly ScopeType m_Scope;

	[JsonProperty]
	private readonly ActionsHolderReference m_Actions;

	[JsonProperty]
	private readonly BlueprintPlayerTimer m_Blueprint;

	[JsonProperty]
	[CanBeNull]
	private readonly MechanicEntity m_MechanicEntityData;

	[JsonProperty]
	[CanBeNull]
	private readonly BaseUnitEntity m_InteractingUnitData;

	[JsonProperty]
	[CanBeNull]
	private readonly MechanicsContext m_Context;

	[JsonProperty]
	[CanBeNull]
	private readonly TargetWrapper m_CurrentTarget;

	[JsonProperty]
	private readonly EntityFactRef m_FactRef;

	[JsonProperty]
	private float m_TimeLeft;

	[JsonProperty]
	private bool m_IsPaused;

	public float TimeLeft => m_TimeLeft;

	public float Duration => m_Duration;

	public BlueprintPlayerTimer Blueprint => m_Blueprint;

	public ScopeType Scope => m_Scope;

	[CanBeNull]
	public EntityFact SourceFact => m_FactRef.Fact;

	[CanBeNull]
	public MechanicsContext SourceContext => m_Context;

	public bool IsPaused
	{
		get
		{
			return m_IsPaused;
		}
		set
		{
			m_IsPaused = value;
		}
	}

	public event Action Stopped;

	[JsonConstructor]
	public PlayerTimer(JsonConstructorMark _)
	{
	}

	public PlayerTimer([NotNull] ActionsHolder actions, float duration, ScopeType scope, [NotNull] BlueprintPlayerTimer timerBp)
	{
		m_Actions = actions.ToReference<ActionsHolderReference>();
		m_TimeLeft = (m_Duration = duration);
		m_Scope = scope;
		m_Blueprint = timerBp;
		m_MechanicEntityData = MechanicEntityData.CurrentEntity;
		m_InteractingUnitData = ContextData<InteractingUnitData>.Current?.Unit;
		m_FactRef = new EntityFactRef(ContextData<FactData>.Current?.Fact as EntityFact);
		MechanicsContext.Data current = ContextData<MechanicsContext.Data>.Current;
		m_Context = current?.Context;
		m_CurrentTarget = current?.CurrentTarget;
	}

	public bool Tick()
	{
		if (m_IsPaused)
		{
			return false;
		}
		m_TimeLeft -= Game.Instance.TimeController.DeltaTime;
		if (m_TimeLeft > 0f)
		{
			return false;
		}
		RunCallback();
		return true;
	}

	private void RunCallback()
	{
		try
		{
			using ((m_MechanicEntityData != null) ? ContextData<MechanicEntityData>.Request().Setup(m_MechanicEntityData) : null)
			{
				using ((m_InteractingUnitData != null) ? ContextData<InteractingUnitData>.Request().Setup(m_InteractingUnitData) : null)
				{
					using ((!m_FactRef.IsEmpty) ? ContextData<FactData>.Request().Setup(m_FactRef.Fact) : null)
					{
						using (m_Context?.GetDataScope(m_CurrentTarget))
						{
							m_Actions.Get().Run();
						}
					}
				}
			}
		}
		finally
		{
			Stop();
		}
	}

	public void Stop()
	{
		this.Stopped?.Invoke();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		float val = m_Duration;
		result.Append(ref val);
		ScopeType val2 = m_Scope;
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_Actions);
		result.Append(ref val3);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Blueprint);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<MechanicEntity>.GetHash128(m_MechanicEntityData);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<BaseUnitEntity>.GetHash128(m_InteractingUnitData);
		result.Append(ref val6);
		Hash128 val7 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val7);
		Hash128 val8 = ClassHasher<TargetWrapper>.GetHash128(m_CurrentTarget);
		result.Append(ref val8);
		EntityFactRef obj = m_FactRef;
		Hash128 val9 = StructHasher<EntityFactRef>.GetHash128(ref obj);
		result.Append(ref val9);
		result.Append(ref m_TimeLeft);
		result.Append(ref m_IsPaused);
		return result;
	}
}
