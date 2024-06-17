using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public sealed class Buff : UnitFact<BlueprintBuff>, IInitiativeHolder, IFactWithRanks, IBuff, IHashable
{
	public class Data : ContextData<Data>
	{
		public Buff Buff { get; private set; }

		public Data Setup(Buff buff)
		{
			Buff = buff;
			return this;
		}

		protected override void Reset()
		{
			Buff = null;
		}
	}

	[JsonProperty]
	[NotNull]
	private readonly MechanicsContext m_Context;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private int m_DurationInRounds;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private bool m_Expired;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private List<EntityFact> m_StoredFacts;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public BuffEndCondition EndCondition;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public readonly Initiative Initiative = new Initiative();

	private AbstractUnitEntityView m_ParticleEffectOwner;

	private List<GameObject> m_ManagedEffects;

	private List<GameObject> m_TrailFxObjects;

	[JsonProperty]
	public int Rank { get; private set; }

	[JsonProperty]
	public int RoundNumber { get; private set; }

	[JsonProperty]
	public bool PlayedFirstHitSound { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool IsSuppressed { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool DisabledBecauseOfNotCasterTurn { get; set; }

	public int DurationInRounds => m_DurationInRounds;

	public int ExpirationInRounds
	{
		get
		{
			if (!IsPermanent)
			{
				return Math.Max(0, DurationInRounds - RoundNumber);
			}
			return int.MaxValue;
		}
	}

	public bool IsExpired
	{
		get
		{
			if (m_Expired)
			{
				return true;
			}
			if (IsPermanent)
			{
				if (!base.Owner.IsInCombat && !(Game.Instance.CurrentMode == GameModeType.SpaceCombat))
				{
					return EndCondition != BuffEndCondition.RemainAfterCombat;
				}
				return false;
			}
			return ExpirationInRounds <= 0;
		}
	}

	public bool IsPermanent => DurationInRounds == 0;

	public MechanicsContext Context => m_Context;

	public bool IsTraumas => base.Blueprint == Root.WH.BlueprintTraumaRoot.Trauma;

	public bool IsWounds
	{
		get
		{
			if (base.Blueprint != Root.WH.BlueprintTraumaRoot.FreshWound)
			{
				return base.Blueprint == Root.WH.BlueprintTraumaRoot.OldWound;
			}
			return true;
		}
	}

	public bool IsProne => base.Blueprint == Root.WH.ProneRoot.ProneCommonBuff;

	public override MechanicsContext MaybeContext => Context;

	public override string Name
	{
		get
		{
			if (!base.Name.IsNullOrEmpty())
			{
				return base.Name;
			}
			EntityFactSource entityFactSource = base.Sources.FirstOrDefault((EntityFactSource source) => source.Entity is ItemEntity);
			if (!(entityFactSource != null))
			{
				return "";
			}
			return ((ItemEntity)entityFactSource.Entity).Name;
		}
	}

	public override string Description
	{
		get
		{
			string description;
			if (!base.Description.IsNullOrEmpty())
			{
				description = base.Description;
			}
			else
			{
				EntityFactSource entityFactSource = base.Sources.FirstOrDefault((EntityFactSource source) => source.Entity is ItemEntity);
				description = ((entityFactSource != null) ? ((ItemEntity)entityFactSource.Entity).Description : "");
			}
			return UIUtilityTexts.GetLongOrShortText(description, state: false);
		}
	}

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled && !IsSuppressed)
			{
				return !DisabledBecauseOfNotCasterTurn;
			}
			return false;
		}
	}

	public override bool Hidden
	{
		get
		{
			if (!base.Hidden)
			{
				return base.Blueprint.IsHiddenInUI;
			}
			return true;
		}
	}

	Initiative IInitiativeHolder.Initiative => Initiative;

	public BlueprintAbilityFXSettings FXSettings => base.Blueprint.FXSettings;

	public bool ShouldBeDisabledOutOfCasterTurn => base.Blueprint.GetComponent<DisableBuffOutOfCasterTurn>() != null;

	MechanicEntity IBuff.Caster => Context.MaybeCaster ?? base.Owner;

	TargetWrapper IBuff.Target => Context.MainTarget;

	BlueprintAbilityFXSettings IBuff.FXSettings => FXSettings;

	public Buff(BlueprintBuff blueprint, MechanicsContext context, BuffDuration duration)
		: base(blueprint)
	{
		if (blueprint.Cyclical)
		{
			context.UnlinkParent();
		}
		CheckMechanicsContextLoop(context);
		EndCondition = duration.EndCondition;
		SetDuration(duration);
		m_Context = context;
		Rank = 1;
	}

	private Buff(JsonConstructorMark _)
	{
	}

	public override IUIDataProvider SelectUIData(UIDataType type)
	{
		return m_Context.SelectUIData(type);
	}

	private void CheckMechanicsContextLoop(MechanicsContext context)
	{
		while (context.ParentContext != null)
		{
			context = context.ParentContext;
			if (context.AssociatedBlueprint == base.Blueprint)
			{
				PFLog.Default.ErrorWithReport($"Found {base.Blueprint} in ParentContext hierarchy. It's potential death loop game load");
			}
		}
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		IsSuppressed = base.Owner.GetOptional<UnitPartBuffSuppress>()?.IsSuppressed(this) ?? false;
		UpdateCombatInitiative();
	}

	public void UpdateCombatInitiative()
	{
		MechanicEntity mechanicEntity = base.Blueprint.Initiative switch
		{
			BlueprintBuff.InitiativeType.ByCaster => Context.MaybeCaster ?? base.Owner, 
			BlueprintBuff.InitiativeType.ByOwner => base.Owner, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		Initiative initiative = ((TurnController.IsInTurnBasedCombat() && mechanicEntity != null && mechanicEntity.IsInCombat) ? mechanicEntity.Initiative : null);
		Initiative.Value = initiative?.Value ?? 0f;
		Initiative.Order = initiative?.Order ?? 0;
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (initiative != null && initiative.TurnOrderPriority >= currentUnit?.Initiative.TurnOrderPriority)
		{
			Initiative.LastTurn = Game.Instance.TurnController.GameRound;
		}
	}

	protected override void OnComponentsDidActivated()
	{
		SpawnParticleEffect();
	}

	protected override void OnDeactivate()
	{
		ClearParticleEffect();
	}

	protected override void OnDispose()
	{
		ClearParticleEffect();
	}

	public void NextRound()
	{
		RoundNumber++;
		CallComponents(delegate(ITickEachRound l)
		{
			l.OnNewRound();
		});
	}

	public void OnRemove()
	{
		if (m_StoredFacts != null)
		{
			foreach (EntityFact storedFact in m_StoredFacts)
			{
				base.Owner.Facts.Remove(storedFact);
			}
			m_StoredFacts.Clear();
		}
		if ((bool)m_ParticleEffectOwner)
		{
			FxHelper.SpawnFxOnEntity(base.Blueprint.FxOnRemove.Load(), m_ParticleEffectOwner);
		}
		m_ParticleEffectOwner = null;
		CallComponents(delegate(IBuffRemoved l)
		{
			l.OnRemoved();
		});
	}

	public void StoreFact(EntityFact fact)
	{
		if (!base.Owner.Facts.Contains(fact))
		{
			PFLog.Default.Error("Can't store fact from different owner");
			return;
		}
		if (m_StoredFacts == null)
		{
			m_StoredFacts = new List<EntityFact>();
		}
		m_StoredFacts.Add(fact);
	}

	private List<GameObject> SureTrailFxList()
	{
		return m_TrailFxObjects ?? (m_TrailFxObjects = new List<GameObject>());
	}

	public void SpawnParticleEffect()
	{
		if (base.Owner?.View == null || base.Owner.View.Data == null || IsParticleEffectValid())
		{
			return;
		}
		EventBus.RaiseEvent<IBuffEffectHandler>(BuffEffectsSpawnHandler);
		SpawnFxFromBuffComponent();
		if (base.Blueprint?.FxOnStart != null)
		{
			GameObject prefab = base.Blueprint.FxOnStart.Load();
			bool flag = base.Blueprint?.FXSettings?.SoundFXSettings != null;
			GameObject gameObject = FxHelper.SpawnFxOnEntity(prefab, base.Owner.View);
			if (gameObject == null)
			{
				return;
			}
			if (flag && gameObject.TryGetComponent<SoundFx>(out var component))
			{
				component.BlockSoundFXPlaying = true;
			}
			if (!gameObject.TryGetComponent<AutoDestroy>(out var _))
			{
				if (m_ManagedEffects == null)
				{
					m_ManagedEffects = new List<GameObject>(1);
				}
				m_ManagedEffects.Add(gameObject);
			}
		}
		m_ParticleEffectOwner = base.Owner.View;
	}

	public void SpawnFxFromBuffComponent()
	{
		if (!(Context.MainTarget.Entity?.View != null) || !(Context.MaybeCaster?.View != null))
		{
			return;
		}
		foreach (BuffSpawnFx component in base.Blueprint.GetComponents<BuffSpawnFx>())
		{
			component.Spawn(Context, Context.MainTarget, component.DestroyOnDeAttach ? SureTrailFxList() : null);
		}
	}

	private void BuffEffectsSpawnHandler(IBuffEffectHandler h)
	{
		GameObject[] array = h.OnBuffEffectApplied(this);
		if (m_ManagedEffects == null)
		{
			m_ManagedEffects = new List<GameObject>(array.Length);
		}
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null && !gameObject.TryGetComponent<AutoDestroy>(out var _))
			{
				m_ManagedEffects.Add(gameObject);
			}
		}
	}

	private bool IsParticleEffectValid()
	{
		if (m_ManagedEffects == null || m_ManagedEffects.Count == 0)
		{
			return false;
		}
		if (m_ManagedEffects.Any((GameObject effect) => GameObjectsPool.IsInPool(effect)))
		{
			return false;
		}
		if (m_ParticleEffectOwner != base.Owner.View)
		{
			return false;
		}
		return true;
	}

	public void ClearParticleEffect()
	{
		EventBus.RaiseEvent(delegate(IBuffEffectHandler h)
		{
			h.OnBuffEffectRemoved(this);
		});
		if (m_ManagedEffects == null || m_ManagedEffects.Count <= 0)
		{
			return;
		}
		foreach (GameObject managedEffect in m_ManagedEffects)
		{
			FxHelper.Destroy(managedEffect);
		}
		m_ParticleEffectOwner = null;
		m_ManagedEffects.Clear();
	}

	public void Remove()
	{
		base.Manager?.Remove(this);
	}

	public void MarkExpired()
	{
		m_Expired = true;
	}

	public override void RunActionInContext(ActionList action, ITargetWrapper target = null)
	{
		using (ContextData<Data>.Request().Setup(this))
		{
			base.RunActionInContext(action, target);
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		m_StoredFacts?.RemoveAll((EntityFact f) => !base.Owner.Facts.Contains(f));
	}

	protected override void OnDetach()
	{
		base.OnDetach();
		if (m_TrailFxObjects != null)
		{
			foreach (GameObject trailFxObject in m_TrailFxObjects)
			{
				FxHelper.Destroy(trailFxObject);
			}
			m_TrailFxObjects = null;
		}
		m_ParticleEffectOwner = null;
		if (m_ManagedEffects != null && m_ManagedEffects.Count > 0)
		{
			foreach (GameObject managedEffect in m_ManagedEffects)
			{
				FxHelper.Destroy(managedEffect, immediate: true);
			}
			m_ManagedEffects.Clear();
		}
		m_ManagedEffects = null;
	}

	public override int GetRank()
	{
		return Rank;
	}

	public void AddRank(int count = 1)
	{
		if (count <= 0)
		{
			return;
		}
		bool isActive = base.IsActive;
		if (isActive)
		{
			Deactivate();
		}
		Rank = Math.Clamp(Rank + count, 0, base.Blueprint.MaxRank);
		if (isActive)
		{
			Activate();
		}
		if (base.Owner != null)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
			{
				h.HandleBuffRankIncreased(this);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveRank(int count = 1)
	{
		if (count <= 0)
		{
			return;
		}
		bool isActive = base.IsActive;
		if (isActive)
		{
			Deactivate();
		}
		Rank = Math.Clamp(Rank - count, 0, base.Blueprint.MaxRank);
		if (isActive)
		{
			Activate();
		}
		if (Rank <= 0)
		{
			Remove();
		}
		else if (base.Owner != null)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
			{
				h.HandleBuffRankDecreased(this);
			}, isCheckRuntime: true);
		}
	}

	public void SetDuration(BuffDuration duration)
	{
		m_DurationInRounds = duration.Rounds?.Value ?? 0;
	}

	public void Prolong(Rounds? rounds)
	{
		if (IsPermanent)
		{
			return;
		}
		if (!rounds.HasValue || rounds == Rounds.Infinity)
		{
			SetDuration(null);
			return;
		}
		Rounds rounds2 = ExpirationInRounds.Rounds();
		Rounds value = rounds2;
		Rounds? rounds3 = rounds;
		if (value < rounds3)
		{
			IncreaseDuration(rounds - rounds2);
		}
	}

	public void MakePermanent()
	{
		m_DurationInRounds = 0;
	}

	public void IncreaseDuration(Rounds? rounds)
	{
		if (!IsPermanent)
		{
			if (!rounds.HasValue || rounds == Rounds.Infinity)
			{
				SetDuration(null);
				return;
			}
			int value = DurationInRounds.Rounds().Value + rounds.Value.Value;
			SetDuration(new Rounds(value));
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val2);
		result.Append(ref m_DurationInRounds);
		result.Append(ref m_Expired);
		List<EntityFact> storedFacts = m_StoredFacts;
		if (storedFacts != null)
		{
			for (int i = 0; i < storedFacts.Count; i++)
			{
				Hash128 val3 = ClassHasher<EntityFact>.GetHash128(storedFacts[i]);
				result.Append(ref val3);
			}
		}
		result.Append(ref EndCondition);
		Hash128 val4 = ClassHasher<Initiative>.GetHash128(Initiative);
		result.Append(ref val4);
		int val5 = Rank;
		result.Append(ref val5);
		int val6 = RoundNumber;
		result.Append(ref val6);
		bool val7 = PlayedFirstHitSound;
		result.Append(ref val7);
		bool val8 = IsSuppressed;
		result.Append(ref val8);
		bool val9 = DisabledBecauseOfNotCasterTurn;
		result.Append(ref val9);
		return result;
	}
}
