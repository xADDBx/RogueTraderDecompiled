using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

public class EtudesSystem : Entity, IUnlockHandler, ISubscriber, IUnlockValueHandler, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, IPartyHandler, IAreaHandler, IQuestObjectiveHandler, ITimeOfDayChangedHandler, ISummonPoolHandler, ISubscriber<IMechanicEntity>, IAreaPartHandler, IAdditiveAreaSwitchHandler, IUnitCombatHandler, IHashable
{
	public class EtudesDataGameStateAdapter : DictionaryConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			writer.WriteStartArray();
			foreach (KeyValuePair<BlueprintEtude, EtudeState> item in ((Dictionary<BlueprintEtude, EtudeState>)value)?.Where(IsItemValid))
			{
				serializer.Serialize(writer, item);
			}
			writer.WriteEndArray();
		}

		public static Hash128 GetHash128(Dictionary<BlueprintEtude, EtudeState> obj)
		{
			int val = 0;
			foreach (KeyValuePair<BlueprintEtude, EtudeState> item in obj.Where(IsItemValid))
			{
				val ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key).GetHashCode();
				EtudeState obj2 = item.Value;
				val ^= EnumHasher<EtudeState>.GetHash128(ref obj2).GetHashCode();
			}
			Hash128 result = default(Hash128);
			result.Append(ref val);
			return result;
		}

		private static bool IsItemValid(KeyValuePair<BlueprintEtude, EtudeState> item)
		{
			return item.Key?.IsGameState() ?? false;
		}
	}

	public enum EtudeState
	{
		Unknown,
		Started,
		Completed,
		PreStarted,
		PreCompleted
	}

	private const bool ReplayLogStackTrace = false;

	[JsonProperty]
	[HasherCustom(Type = typeof(EtudesDataGameStateAdapter))]
	private readonly Dictionary<BlueprintEtude, EtudeState> m_EtudesData = new Dictionary<BlueprintEtude, EtudeState>();

	[JsonProperty]
	private readonly Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude> m_HeldConflictingGroups = new Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude>();

	private bool m_IsUpdateForUnload;

	private BlueprintAreaPart m_AreaPartBeingLoaded;

	private BlueprintAreaPart m_AreaPartBeingExited;

	public EtudesTree Etudes { get; private set; }

	public bool ConditionsDirty { get; private set; }

	public BlueprintAreaPart LoadEtudesForAreaPart
	{
		get
		{
			object obj;
			if (!m_IsUpdateForUnload)
			{
				obj = SimpleBlueprintExtendAsObject.Or(m_AreaPartBeingLoaded, null);
				if (obj == null)
				{
					return Game.Instance.CurrentlyLoadedAreaPart;
				}
			}
			else
			{
				obj = null;
			}
			return (BlueprintAreaPart)obj;
		}
	}

	public BlueprintAreaPart AreaPartBeingExited => m_AreaPartBeingExited;

	public bool EtudeIsNotStarted(BlueprintEtude etude)
	{
		return !m_EtudesData.ContainsKey(etude);
	}

	public bool EtudeIsCompleted(BlueprintEtude etude)
	{
		if (!m_EtudesData.TryGetValue(etude, out var value))
		{
			return false;
		}
		if (value != EtudeState.Completed)
		{
			return value == EtudeState.PreCompleted;
		}
		return true;
	}

	public bool EtudeIsPreCompleted(BlueprintEtude etude)
	{
		if (!m_EtudesData.TryGetValue(etude, out var value))
		{
			return false;
		}
		return value == EtudeState.PreCompleted;
	}

	public EtudeState GetSavedState(BlueprintEtude bp)
	{
		m_EtudesData.TryGetValue(bp, out var value);
		return value;
	}

	protected EtudesSystem(JsonConstructorMark _)
		: base(_)
	{
	}

	public EtudesSystem()
		: base("etudes_system", isInGame: true)
	{
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		Etudes = Facts.EnsureFactProcessor<EtudesTree>();
	}

	public void MarkConditionsDirty()
	{
		ConditionsDirty = true;
	}

	public void ClearConditionsDirty()
	{
		ConditionsDirty = false;
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		Etudes = Facts.EnsureFactProcessor<EtudesTree>();
		Etudes.RestoreTreeStructure();
		FixupActorChanges();
	}

	protected override void OnDidPostLoad()
	{
		Etudes.FixupEtudesTree(this);
		Etudes.CheckEtudeHierarchyForErrors();
	}

	public void OnAreaBeginUnloading()
	{
		if (!Game.Instance.IsUnloading)
		{
			m_IsUpdateForUnload = true;
			UpdateEtudes();
			m_IsUpdateForUnload = false;
		}
	}

	public BlueprintEtude GetConflictingGroupTask(BlueprintEtudeConflictingGroup conflictingGroup)
	{
		if (!conflictingGroup)
		{
			return null;
		}
		return m_HeldConflictingGroups.Get(conflictingGroup);
	}

	public void SetConflictingGroupTask(BlueprintEtudeConflictingGroup conflictingGroup, Etude e)
	{
		if ((bool)conflictingGroup)
		{
			if (e != null)
			{
				m_HeldConflictingGroups[conflictingGroup] = e.Blueprint;
			}
			else
			{
				m_HeldConflictingGroups.Remove(conflictingGroup);
			}
		}
	}

	public void StartEtude(BlueprintEtude bp)
	{
		switch (GetSavedState(bp))
		{
		case EtudeState.Started:
			PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: already started");
			return;
		case EtudeState.Completed:
			PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: already completed");
			return;
		}
		if (!bp.Parent.IsEmpty())
		{
			switch (GetSavedState(bp.Parent.Get()))
			{
			case EtudeState.Unknown:
			case EtudeState.PreStarted:
				if (bp.StartsParent)
				{
					StartEtude(bp.Parent);
					if (GetSavedState(bp) == EtudeState.Started)
					{
						return;
					}
				}
				if (GetSavedState(bp.Parent) != EtudeState.Started)
				{
					m_EtudesData[bp] = EtudeState.PreStarted;
					GameHistoryLog.Instance.EtudeEvent(null, "Etude[" + bp.NameSafe() + "]:PreStarted");
					PFLog.Etudes.Log(bp, $"Starting etude {bp}: parent not started, marking prestart");
					return;
				}
				break;
			case EtudeState.Completed:
			case EtudeState.PreCompleted:
				PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: parent already completed");
				return;
			}
		}
		StartEtudeInternal(bp);
	}

	public void StartEtudeImmediately(BlueprintEtude bp)
	{
		StartEtude(bp);
		UpdateEtudes();
	}

	private void StartEtudeInternal(BlueprintEtude bp)
	{
		EtudeState savedState = GetSavedState(bp);
		if (savedState == EtudeState.Completed)
		{
			return;
		}
		PFLog.Etudes.Log("Starting etude: " + bp.name);
		if (Etudes.Get(bp) != null)
		{
			PFLog.Etudes.Error(bp, $"Cannot start etude {bp}: already started");
			return;
		}
		Etude etude = (bp.Parent.IsEmpty() ? null : Etudes.Get(bp.Parent.Get()));
		if (etude != null && etude.CompletionInProgress)
		{
			PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: parent is CompletionInProgress");
			return;
		}
		Facts.Add(new Etude(bp, etude));
		if (savedState == EtudeState.PreCompleted)
		{
			MarkEtudeCompleted(bp);
			return;
		}
		m_EtudesData[bp] = EtudeState.Started;
		foreach (BlueprintEtudeReference item in bp.StartsWith)
		{
			if (!item.IsEmpty())
			{
				StartEtudeInternal(item.Get());
			}
		}
		foreach (KeyValuePair<BlueprintEtude, EtudeState> item2 in m_EtudesData.Where((KeyValuePair<BlueprintEtude, EtudeState> p) => p.Key.Parent.Is(bp) && p.Value == EtudeState.PreStarted).ToTempList())
		{
			StartEtudeInternal(item2.Key);
		}
		MarkConditionsDirty();
	}

	public void MarkEtudeCompleted(BlueprintEtude bp)
	{
		PFLog.Etudes.Log("Completing etude: " + bp.name);
		Etude etude = Etudes.Get(bp);
		m_EtudesData[bp] = ((etude == null) ? EtudeState.PreCompleted : EtudeState.Completed);
		if (bp.CompletesParent && !bp.Parent.IsEmpty())
		{
			MarkEtudeCompleted(bp.Parent.Get());
			return;
		}
		if (etude != null)
		{
			etude.MarkCompleted();
		}
		else
		{
			GameHistoryLog.Instance.EtudeEvent(null, "Etude[" + bp.NameSafe() + "]:PreCompleted");
		}
		MarkConditionsDirty();
	}

	public void InternalMarkCompleted(BlueprintEtude bp)
	{
		m_EtudesData[bp] = EtudeState.Completed;
	}

	public void ForceUpdateEtudes(BlueprintAreaPart areaPartBeingLoaded)
	{
		m_AreaPartBeingLoaded = areaPartBeingLoaded;
		UpdateEtudes();
		m_AreaPartBeingLoaded = null;
	}

	public void UpdateEtudes()
	{
		PFLog.Etudes.Log("Updating etude system");
		ClearConditionsDirty();
		HashSet<BlueprintAreaMechanics> equals = (m_AreaPartBeingLoaded ? null : GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).ToHashSet());
		Etudes.MaybeDeactivateCompletedEtudes();
		Etudes.SelectPlayingEtudes();
		if (m_AreaPartBeingLoaded != null)
		{
			return;
		}
		if (!GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).ToHashSet().SetEquals(equals))
		{
			PFLog.Etudes.Log("Etude system causing mechanics reload");
			Game.ReloadAreaMechanic(clearFx: false);
			LoadingProcess.Instance.StartLoadingProcess(delegate
			{
				EventBus.RaiseEvent(delegate(IEtudesUpdateHandler h)
				{
					h.OnEtudesUpdate();
				});
			}, LoadingProcessTag.ReloadMechanics);
		}
		else
		{
			EventBus.RaiseEvent(delegate(IEtudesUpdateHandler h)
			{
				h.OnEtudesUpdate();
			});
		}
	}

	public void FixupActorChanges()
	{
		foreach (KeyValuePair<BlueprintEtudeConflictingGroup, BlueprintEtude> item in m_HeldConflictingGroups.ToTempList())
		{
			if (!item.Value.ConflictingGroups.HasReference(item.Key))
			{
				SetConflictingGroupTask(item.Key, null);
				PFLog.Etudes.Log($"Fixed conflicting group {item.Key}: no longer held by {item.Value}");
			}
		}
		foreach (Etude rawFact in Etudes.RawFacts)
		{
			if (!rawFact.IsPlaying)
			{
				continue;
			}
			foreach (BlueprintEtudeConflictingGroupReference conflictingGroup in rawFact.Blueprint.ConflictingGroups)
			{
				BlueprintEtudeConflictingGroup blueprintEtudeConflictingGroup = conflictingGroup.Get();
				if (blueprintEtudeConflictingGroup != null)
				{
					BlueprintEtude conflictingGroupTask = GetConflictingGroupTask(blueprintEtudeConflictingGroup);
					if (conflictingGroupTask == null || conflictingGroupTask.Priority < rawFact.Blueprint.Priority)
					{
						PFLog.Etudes.Log($"Fixed conflicting group {blueprintEtudeConflictingGroup}: should be held by {rawFact} (@{rawFact.Blueprint.Priority})");
						SetConflictingGroupTask(blueprintEtudeConflictingGroup, rawFact);
					}
				}
			}
		}
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		MarkConditionsDirty();
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		MarkConditionsDirty();
	}

	public void HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		MarkConditionsDirty();
	}

	public void HandleRecruit()
	{
		MarkConditionsDirty();
	}

	public void HandleUnrecruit()
	{
		MarkConditionsDirty();
	}

	public void HandleAddCompanion()
	{
		MarkConditionsDirty();
	}

	public void HandleCompanionActivated()
	{
		MarkConditionsDirty();
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		MarkConditionsDirty();
	}

	public void HandleCapitalModeChanged()
	{
	}

	public string GetDebugInfo(BlueprintEtude bp)
	{
		Etude etude = Etudes.Get(bp);
		EtudeState savedState = GetSavedState(bp);
		return $"[{bp.name}] is {savedState}: IsStarted={etude?.IsAttached} IsPlaying={etude?.IsPlaying} Completion={etude?.CompletionInProgress}";
	}

	public IEnumerable<BlueprintAreaMechanics> GetActiveAdditionalMechanics(BlueprintArea area)
	{
		foreach (Etude rawFact in Etudes.RawFacts)
		{
			if (!rawFact.IsPlaying)
			{
				continue;
			}
			foreach (BlueprintAreaMechanicsReference addedAreaMechanic in rawFact.Blueprint.AddedAreaMechanics)
			{
				if (!addedAreaMechanic.IsEmpty() && addedAreaMechanic.Get().Area.Is(area))
				{
					yield return addedAreaMechanic.Get();
				}
			}
		}
	}

	private IEnumerable<BlueprintEtude> GetEtudesByState(EtudeState state)
	{
		return from x in m_EtudesData
			where x.Value == state
			select x.Key;
	}

	public IEnumerable<BlueprintEtude> GetStartedEtudes()
	{
		return GetEtudesByState(EtudeState.Started);
	}

	public IEnumerable<BlueprintEtude> GetCompletedEtudes()
	{
		return GetEtudesByState(EtudeState.Completed);
	}

	public void OnAreaDidLoad()
	{
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void OnTimeOfDayChanged()
	{
		MarkConditionsDirty();
	}

	public void HandleUnitAdded(ISummonPool pool)
	{
		MarkConditionsDirty();
	}

	public void HandleUnitRemoved(ISummonPool pool)
	{
		MarkConditionsDirty();
	}

	public void HandleLastUnitRemoved(ISummonPool pool)
	{
	}

	public void OnAreaPartChanged(BlueprintAreaPart previous)
	{
		m_AreaPartBeingExited = SimpleBlueprintExtendAsObject.Or(previous, Game.Instance.CurrentlyLoadedArea);
		UpdateEtudes();
		m_AreaPartBeingExited = null;
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		if (!Game.Instance.IsUnloading)
		{
			m_IsUpdateForUnload = true;
			UpdateEtudes();
			m_IsUpdateForUnload = false;
		}
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleUnitJoinCombat()
	{
		MarkConditionsDirty();
	}

	public void HandleUnitLeaveCombat()
	{
		MarkConditionsDirty();
	}

	public void UnstartEtude(BlueprintEtude bp, bool markPreStarted = false)
	{
		if (bp.IsReadOnly)
		{
			PFLog.Default.Error($"Cannot unstart etude {this} as it is read-only.");
			return;
		}
		Etude etude = Etudes.Get(bp);
		if (etude == null)
		{
			m_EtudesData.Remove(bp);
		}
		else
		{
			foreach (Etude item in etude.Children.ToTempList())
			{
				UnstartEtude(item.Blueprint, markPreStarted: true);
			}
		}
		foreach (KeyValuePair<BlueprintEtude, EtudeState> item2 in m_EtudesData.Where((KeyValuePair<BlueprintEtude, EtudeState> p) => p.Value == EtudeState.Completed).ToTempList())
		{
			if (IsDescendant(bp, item2.Key))
			{
				m_EtudesData[item2.Key] = EtudeState.PreCompleted;
			}
		}
		if (etude != null)
		{
			Etudes.Remove(etude);
			m_EtudesData.Remove(bp);
		}
		if (markPreStarted)
		{
			m_EtudesData[bp] = EtudeState.PreStarted;
		}
		static bool IsDescendant(BlueprintEtude a, BlueprintEtude b)
		{
			if (!b.Parent.Is(a))
			{
				if (!b.Parent.IsEmpty())
				{
					return IsDescendant(a, b.Parent);
				}
				return false;
			}
			return true;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = EtudesDataGameStateAdapter.GetHash128(m_EtudesData);
		result.Append(ref val2);
		Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude> heldConflictingGroups = m_HeldConflictingGroups;
		if (heldConflictingGroups != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<BlueprintEtudeConflictingGroup, BlueprintEtude> item in heldConflictingGroups)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val4);
				Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Value);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		return result;
	}
}
