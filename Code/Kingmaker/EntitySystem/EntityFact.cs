using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public class EntityFact : IDisposable, IUIDataProvider, IEntityFact, IHashable
{
	private static class ComponentsDataHasher
	{
		public static Hash128 GetHash128(Dictionary<string, List<IEntityFactComponentSavableData>> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (KeyValuePair<string, List<IEntityFactComponentSavableData>> item in obj)
			{
				Hash128 val = StringHasher.GetHash128(item.Key);
				result.Append(ref val);
				for (int i = 0; i < item.Value.Count; i++)
				{
					Hash128 val2 = item.Value[i].GetHash128();
					result.Append(ref val2);
				}
			}
			return result;
		}
	}

	public struct ComponentEnumerator<TComponent> : IEnumerator<TComponent>, IEnumerator, IDisposable where TComponent : BlueprintComponent
	{
		private List<BlueprintComponent> m_Components;

		private Func<TComponent, bool> m_Predicate;

		private int m_Index;

		public TComponent Current => (TComponent)m_Components[m_Index];

		object IEnumerator.Current => m_Components[m_Index];

		public ComponentEnumerator(List<BlueprintComponent> components, Func<TComponent, bool> pred)
		{
			m_Index = -1;
			m_Components = components;
			m_Predicate = pred;
		}

		public bool MoveNext()
		{
			if (m_Components == null)
			{
				return false;
			}
			if (m_Predicate == null)
			{
				m_Index++;
				return m_Index < m_Components.Count;
			}
			m_Index++;
			while (m_Index < m_Components.Count)
			{
				if (m_Predicate((TComponent)m_Components[m_Index]))
				{
					return true;
				}
				m_Index++;
			}
			return false;
		}

		public void Reset()
		{
			m_Index = -1;
		}

		public void Dispose()
		{
			Reset();
			m_Components = null;
		}
	}

	public struct ComponentsEnumerable<TComponent> where TComponent : BlueprintComponent
	{
		private List<BlueprintComponent> m_SourceList;

		private Func<TComponent, bool> m_Predicate;

		public ComponentsEnumerable(List<BlueprintComponent> sourceList, Func<TComponent, bool> pred)
		{
			m_SourceList = sourceList;
			m_Predicate = pred;
		}

		public ComponentEnumerator<TComponent> GetEnumerator()
		{
			return new ComponentEnumerator<TComponent>(m_SourceList, m_Predicate);
		}

		public bool Any(Func<TComponent, bool> pred = null)
		{
			if (pred == null)
			{
				GetEnumerator().MoveNext();
			}
			using (ComponentEnumerator<TComponent> componentEnumerator = GetEnumerator())
			{
				while (componentEnumerator.MoveNext())
				{
					TComponent current = componentEnumerator.Current;
					if (pred(current))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[HasherCustom(Type = typeof(ComponentsDataHasher))]
	private Dictionary<string, List<IEntityFactComponentSavableData>> m_ComponentsData;

	[JsonProperty]
	private readonly List<EntityFactComponent> m_Components = new List<EntityFactComponent>();

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	private List<EntityFactSource> m_Sources;

	private Dictionary<string, List<IEntityFactComponentTransientData>> m_ComponentsTransientData;

	private bool m_RemoveWhenActivatedOrPostLoaded;

	[CanBeNull]
	private (EntityFactComponent Runtime, BlueprintComponent Component)[] m_AllComponentsCache;

	private EntityRef m_CachedOwner;

	protected readonly CountableFlag m_IsReapplying = new CountableFlag();

	private Dictionary<Type, List<BlueprintComponent>> m_ComponentsByType = new Dictionary<Type, List<BlueprintComponent>>();

	public virtual Type RequiredEntityType => EntityInterfacesHelper.EntityInterface;

	[JsonProperty]
	public string UniqueId { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, PropertyName = "Source")]
	[CanBeNull]
	[GameStateIgnore]
	private EntityFactSource ObsoleteSourceConverter
	{
		get
		{
			return null;
		}
		set
		{
			if (value != null && !value.Equals(EntityFactSource.Empty))
			{
				(m_Sources ?? (m_Sources = new List<EntityFactSource>())).Add(value);
			}
		}
	}

	[JsonProperty]
	public BlueprintFact Blueprint { get; protected set; }

	[JsonProperty]
	public bool IsActive { get; private set; }

	public EntityFactsManager Manager { get; protected set; }

	public bool IsSubscribedOnEventBus { get; private set; }

	public bool Activating { get; private set; }

	public bool Deactivating { get; private set; }

	public bool IsDisposed { get; private set; }

	public bool SuppressActivationOnAttach { get; set; }

	public bool IsReapplying => m_IsReapplying;

	public ReadonlyList<EntityFactSource> Sources => m_Sources;

	public EntityFactSource FirstSource => m_Sources?.Get(0);

	[CanBeNull]
	public EntityFact SourceFact => FirstSource?.Fact;

	[CanBeNull]
	public IItemEntity SourceItem => FirstSource?.Entity as IItemEntity;

	[CanBeNull]
	public BlueprintAbility SourceAbilityBlueprint => null;

	public List<EntityFactComponent> Components => m_Components;

	public bool IsAttached => Manager != null;

	public virtual bool IsEnabled => true;

	public IEntity Owner => Manager?.Owner ?? m_CachedOwner.Entity;

	public IEntity IOwner => Owner;

	public bool Active => IsActive;

	[CanBeNull]
	public virtual MechanicsContext MaybeContext => null;

	private bool AllowActivate
	{
		get
		{
			if (IsEnabled)
			{
				IEntity owner = Owner;
				if (owner != null && !owner.IsDisposingNow)
				{
					EntityFactsManager.IFactProcessor factProcessor = Manager?.FirstSuitableDelegate(this);
					return factProcessor == null || factProcessor.IsActive;
				}
			}
			return false;
		}
	}

	private bool AllowSubscribe
	{
		get
		{
			IEntity owner = Owner;
			if (owner != null && !owner.IsDisposingNow)
			{
				EntityFactsManager.IFactProcessor factProcessor = Manager?.FirstSuitableDelegate(this);
				return factProcessor == null || factProcessor.IsSubscribedOnEventBus;
			}
			return false;
		}
	}

	private ReadonlyList<(EntityFactComponent Runtime, BlueprintComponent Component)> AllComponentsCache => m_AllComponentsCache ?? (m_AllComponentsCache = (from i in m_Components.Select((EntityFactComponent i) => (i: i, SourceBlueprintComponent: i.SourceBlueprintComponent)).Concat<(EntityFactComponent, BlueprintComponent)>(from i in Blueprint.ComponentsArray
			where !m_Components.HasItem((EntityFactComponent ii) => ii.SourceBlueprintComponent == i)
			select ((EntityFactComponent, BlueprintComponent i))(null, i: i))
		where !i.Item2.Disabled
		select i).ToArray());

	public virtual string Name => SelectUIData(UIDataType.Name)?.Name ?? "";

	public virtual string Description => SelectUIData(UIDataType.Description)?.Description ?? "";

	public virtual Sprite Icon => SelectUIData(UIDataType.Icon)?.Icon;

	public virtual string NameForAcronym => SelectUIData(UIDataType.NameForAcronym)?.NameForAcronym ?? "";

	[JsonConstructor]
	public EntityFact()
	{
	}

	public EntityFact(BlueprintFact fact)
		: this()
	{
		Setup(fact);
	}

	private void Setup(BlueprintFact blueprint)
	{
		IsDisposed = false;
		Blueprint = blueprint;
		List<BlueprintComponent> list = TempList.Get<BlueprintComponent>();
		Blueprint.CollectComponents(list);
		m_ComponentsByType.Clear();
		foreach (BlueprintComponent component in list)
		{
			if (!component.Disabled)
			{
				Type type = component.GetType();
				while (type != typeof(BlueprintComponent))
				{
					if (!m_ComponentsByType.TryGetValue(type, out var value))
					{
						value = new List<BlueprintComponent>();
						m_ComponentsByType.Add(type, value);
					}
					value.Add(component);
					type = type.BaseType;
				}
			}
			if (component is IRuntimeEntityFactComponentProvider runtimeEntityFactComponentProvider && !m_Components.HasItem((EntityFactComponent i) => i.SourceBlueprintComponentName == component.name) && !component.Disabled)
			{
				EntityFactComponent entityFactComponent = runtimeEntityFactComponentProvider.CreateRuntimeFactComponent();
				entityFactComponent.Setup(component);
				AddComponent(entityFactComponent);
			}
		}
	}

	protected bool AddComponent<TComponent>(TComponent component) where TComponent : EntityFactComponent
	{
		if (m_Components.HasItem(component))
		{
			PFLog.EntityFact.Error($"EntityFact.AddComponent: can't add component twice {component}");
			return false;
		}
		if (component.IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.AddComponent: can't add active component {component}");
			return false;
		}
		m_Components.Add(component);
		component.Initialize(this);
		if (IsActive)
		{
			try
			{
				component.Activate();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
		return true;
	}

	public override string ToString()
	{
		return ((Blueprint == null) ? GetType().Name : (GetType().Name + "[" + Blueprint.name + "]")) + "#" + (object)Owner;
	}

	public IEntity GetSubscribingEntity()
	{
		IEntity entity = (Manager?.Owner as ItemEntity)?.Wielder;
		IEntity entity2 = entity;
		if (entity2 == null)
		{
			EntityFactsManager manager = Manager;
			if (manager == null)
			{
				return null;
			}
			entity2 = manager.Owner;
		}
		return entity2;
	}

	public virtual int GetRank()
	{
		return 1;
	}

	public ComponentsEnumerable<TComponent> SelectComponents<TComponent>() where TComponent : BlueprintComponent
	{
		if (!m_ComponentsByType.TryGetValue(typeof(TComponent), out var value))
		{
			return new ComponentsEnumerable<TComponent>(null, null);
		}
		return new ComponentsEnumerable<TComponent>(value, null);
	}

	public ComponentsEnumerable<TComponent> SelectComponents<TComponent>(Func<TComponent, bool> pred) where TComponent : BlueprintComponent
	{
		if (!m_ComponentsByType.TryGetValue(typeof(TComponent), out var value))
		{
			return new ComponentsEnumerable<TComponent>(null, null);
		}
		return new ComponentsEnumerable<TComponent>(value, pred);
	}

	public IEnumerable<BlueprintComponentAndRuntime<TComponent>> SelectComponentsWithRuntime<TComponent>() where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component2 in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component2.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent component && !sourceBlueprintComponent.Disabled)
			{
				yield return new BlueprintComponentAndRuntime<TComponent>(component, component2);
			}
		}
	}

	public IEnumerable<BlueprintComponentAndRuntime<TComponent>> SelectComponentsWithRuntime<TComponent>(Func<TComponent, EntityFactComponent, bool> pred) where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent val && !sourceBlueprintComponent.Disabled && pred(val, component))
			{
				yield return new BlueprintComponentAndRuntime<TComponent>(val, component);
			}
		}
	}

	[CanBeNull]
	public TComponent GetComponent<TComponent>() where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent result && !sourceBlueprintComponent.Disabled)
			{
				return result;
			}
		}
		BlueprintComponent[] componentsArray = Blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (!(blueprintComponent is IRuntimeEntityFactComponentProvider) && blueprintComponent is TComponent result2 && !blueprintComponent.Disabled)
			{
				return result2;
			}
		}
		return null;
	}

	public BlueprintComponentAndRuntime<TComponent> GetComponentWithRuntime<TComponent>() where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component2 in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component2.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent component && !sourceBlueprintComponent.Disabled)
			{
				return new BlueprintComponentAndRuntime<TComponent>(component, component2);
			}
		}
		return default(BlueprintComponentAndRuntime<TComponent>);
	}

	public EntityFactComponentsEnumerator<TComponent> GetComponents<TComponent>([CanBeNull] Func<TComponent, bool> pred) where TComponent : BlueprintComponent
	{
		return new EntityFactComponentsEnumerator<TComponent>(this, pred);
	}

	public void CallComponents<TComponent>(Action<TComponent> action) where TComponent : class
	{
		if (action == null)
		{
			PFLog.Default.ErrorWithReport("Calling components of type " + typeof(TComponent).Name + " for null action.");
			return;
		}
		foreach (var item3 in AllComponentsCache)
		{
			EntityFactComponent item = item3.Runtime;
			BlueprintComponent item2 = item3.Component;
			TComponent val = (item as TComponent) ?? (item2 as TComponent);
			if (!item2.Disabled && val != null)
			{
				using (item?.RequestEventContext())
				{
					action(val);
				}
			}
		}
	}

	public void CallComponentsWithRuntime<TComponent>(Action<TComponent, EntityFactComponent> action) where TComponent : class
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (!sourceBlueprintComponent.Disabled && sourceBlueprintComponent is TComponent arg)
			{
				using (component.RequestEventContext())
				{
					action(arg, component);
				}
			}
		}
	}

	public void Reapply()
	{
		if (Owner == null || Owner.IsDisposingNow)
		{
			return;
		}
		try
		{
			m_IsReapplying.Retain();
			MaybeContext?.Recalculate();
			if (IsActive && !Deactivating && !Activating)
			{
				Deactivate();
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
	}

	public virtual void RunActionInContext(ActionList actions, MechanicEntity target)
	{
		RunActionInContext(actions, target.ToITargetWrapper());
	}

	public virtual void RunActionInContext(ActionList actions, ITargetWrapper target = null)
	{
		if (actions == null)
		{
			return;
		}
		if (MaybeContext == null)
		{
			PFLog.Default.ErrorWithReport("There is no Context in " + GetType().Name + ": '" + Name + "' [" + UniqueId + "]. Blueprint: '" + Blueprint?.name + "' [" + Blueprint?.AssetGuid + "]");
		}
		using (ContextData<FactData>.Request().Setup(this))
		{
			using (MaybeContext?.GetDataScope(target))
			{
				actions.Run();
			}
		}
	}

	protected virtual bool SupportsMultipleSources()
	{
		return false;
	}

	protected void AddSource(EntityFactSource source)
	{
		List<EntityFactSource> sources = m_Sources;
		if (sources == null || !sources.HasItem(source))
		{
			if (!m_Sources.Empty() && !SupportsMultipleSources())
			{
				PFLog.Default.ErrorWithReport($"EntityFact.AddSource ({Blueprint}): !m_Sources.Empty() && !SupportsMultipleSources()");
			}
			(m_Sources ?? (m_Sources = new List<EntityFactSource>())).Add(source);
		}
	}

	public void AddSource([NotNull] EntityFact fact, BlueprintComponent component = null)
	{
		AddSource(new EntityFactSource(fact, component));
	}

	public void AddSource(UnitCondition unitCondition)
	{
		AddSource(new EntityFactSource(unitCondition));
	}

	public void AddSource(Etude etude)
	{
		AddSource(new EntityFactSource(etude));
	}

	public void AddSource(Entity entity)
	{
		AddSource(new EntityFactSource(entity));
	}

	public void AddSource(BlueprintScriptableObject blueprint)
	{
		AddSource(new EntityFactSource(blueprint));
	}

	public void TryAddSource(Element element)
	{
		ICutscenePlayerData current = CutscenePlayerDataScope.Current;
		if (current != null)
		{
			AddSource(new EntityFactSource((CutscenePlayerData)current));
			return;
		}
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext != null)
		{
			AddSource(new EntityFactSource(mechanicsContext.AssociatedBlueprint));
		}
		else if (element.Owner is BlueprintScriptableObject blueprint)
		{
			AddSource(new EntityFactSource(blueprint));
		}
	}

	public void AddSource(BlueprintScriptableObject blueprint, int pathRank)
	{
		AddSource(new EntityFactSource(blueprint, pathRank));
	}

	public void AddSource([NotNull] EntityPart part)
	{
		throw new NotImplementedException("Entity.AddSource(EntityPart): not implemented yet");
	}

	protected void RemoveSource(EntityFactSource source)
	{
		m_Sources?.Remove(source);
		if (m_Sources.Empty())
		{
			m_Sources = null;
		}
	}

	public bool IsFrom([NotNull] EntityFact fact, [CanBeNull] BlueprintComponent component = null)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(fact, component)) ?? false;
	}

	public bool IsFrom([NotNull] Entity entity)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(entity)) ?? false;
	}

	public bool IsFrom(UnitCondition unitCondition)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(unitCondition)) ?? false;
	}

	public bool IsFrom(Etude etude)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(etude)) ?? false;
	}

	[NotNull]
	public TData RequestSavableData<TData>(BlueprintComponent component) where TData : IEntityFactComponentSavableData, new()
	{
		List<IEntityFactComponentSavableData> list = m_ComponentsData?.Get(component.name);
		TData val = (TData)list.FirstItem((IEntityFactComponentSavableData i) => i is TData);
		if (val == null)
		{
			if (m_ComponentsData == null)
			{
				m_ComponentsData = new Dictionary<string, List<IEntityFactComponentSavableData>>();
			}
			val = new TData();
			if (list == null)
			{
				list = new List<IEntityFactComponentSavableData>();
				m_ComponentsData.Add(component.name, list);
			}
			list.Add(val);
		}
		return val;
	}

	[NotNull]
	public TData RequestSavableData<TData>(EntityFactComponent component) where TData : IEntityFactComponentSavableData, new()
	{
		return RequestSavableData<TData>(component.SourceBlueprintComponent);
	}

	[NotNull]
	public TData RequestTransientData<TData>(BlueprintComponent component) where TData : IEntityFactComponentTransientData, new()
	{
		List<IEntityFactComponentTransientData> list = m_ComponentsTransientData?.Get(component.name);
		TData val = (TData)list.FirstItem((IEntityFactComponentTransientData i) => i is TData);
		if (val == null)
		{
			if (m_ComponentsTransientData == null)
			{
				m_ComponentsTransientData = new Dictionary<string, List<IEntityFactComponentTransientData>>();
			}
			val = new TData();
			if (list == null)
			{
				list = new List<IEntityFactComponentTransientData>();
				m_ComponentsTransientData.Add(component.name, list);
			}
			list.Add(val);
		}
		return val;
	}

	[NotNull]
	public TData RequestTransientData<TData>(EntityFactComponent component) where TData : IEntityFactComponentTransientData, new()
	{
		return RequestTransientData<TData>(component.SourceBlueprintComponent);
	}

	public void Attach([NotNull] EntityFactsManager manager)
	{
		if (Manager != null)
		{
			PFLog.EntityFact.Error($"EntityFact.Attach: already attached to manager ({this})");
			return;
		}
		Manager = manager;
		if (UniqueId.IsNullOrEmpty())
		{
			UniqueId = Uuid.Instance.CreateString();
		}
		try
		{
			OnAttach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			if (!component.RequiredEntityType.IsAssignableFrom(Owner.GetType()))
			{
				PFLog.EntityFact.Error("EntityFact.Attach: invalid component required type {0} (target is {1})", component.RequiredEntityType.Name, Owner.GetType().Name);
			}
			try
			{
				component.OnFactAttached();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
		}
		EntityFactService.Instance.Register(this);
		if (!SuppressActivationOnAttach)
		{
			Activate();
		}
	}

	public void Detach()
	{
		EntityFactService.Instance.Unregister(this);
		if (Manager == null)
		{
			PFLog.EntityFact.Error($"EntityFact.Detach: isn't attached to entity ({this})");
			return;
		}
		if (IsActive)
		{
			Deactivate();
		}
		foreach (EntityFactComponent component in m_Components)
		{
			if (!component.RequiredEntityType.IsAssignableFrom(Owner.GetType()))
			{
				PFLog.EntityFact.Error("EntityFact.Attach: invalid component required type {0} (target is {1})", component.RequiredEntityType.Name, Owner.GetType().Name);
			}
			try
			{
				component.OnFactDetached();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
		try
		{
			OnDetach();
		}
		catch (Exception ex2)
		{
			PFLog.EntityFact.Exception(ex2);
		}
		m_CachedOwner = Manager.ConcreteOwner.Ref;
		Manager = null;
	}

	public void Activate()
	{
		if (!AllowActivate)
		{
			return;
		}
		if (!IsAttached)
		{
			PFLog.EntityFact.Error($"EntityFact.Activate: invalid state ({this})");
			return;
		}
		if (IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.Activate: already active ({this})");
			return;
		}
		IsActive = true;
		try
		{
			Activating = true;
			try
			{
				OnActivate();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
			try
			{
				MaybeContext?.Recalculate();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
			using (ContextData<FactData>.Request().Setup(this))
			{
				foreach (EntityFactComponent component in m_Components)
				{
					try
					{
						component.Activate();
					}
					catch (Exception ex3)
					{
						PFLog.EntityFact.Exception(ex3);
					}
				}
			}
			try
			{
				OnComponentsDidActivated();
			}
			catch (Exception ex4)
			{
				PFLog.EntityFact.Exception(ex4);
			}
			if (Manager.IsSubscribedOnEventBus)
			{
				Subscribe();
			}
		}
		catch (Exception ex5)
		{
			PFLog.EntityFact.Exception(ex5);
		}
		finally
		{
			Activating = false;
		}
		if (m_RemoveWhenActivatedOrPostLoaded)
		{
			m_RemoveWhenActivatedOrPostLoaded = false;
			Manager.Remove(this);
		}
	}

	public void Deactivate()
	{
		if (!IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.Deactivate: is not active ({this})");
		}
		else
		{
			if (Deactivating)
			{
				return;
			}
			if (Activating)
			{
				PFLog.EntityFact.ErrorWithReport("EntityFact.Deactivate: invoked from Activate");
			}
			Deactivating = true;
			Unsubscribe();
			using (ContextData<FactData>.Request().Setup(this))
			{
				foreach (EntityFactComponent component in m_Components)
				{
					try
					{
						component.Deactivate();
					}
					catch (Exception ex)
					{
						PFLog.EntityFact.Exception(ex);
					}
				}
			}
			try
			{
				OnDeactivate();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
			IsActive = false;
			Deactivating = false;
		}
	}

	public void UpdateIsActive()
	{
		if (Active && !IsEnabled)
		{
			Deactivate();
		}
		if (!Active && IsEnabled)
		{
			Activate();
		}
	}

	public void Subscribe()
	{
		if (!AllowSubscribe)
		{
			return;
		}
		if (!IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.TurnOn: invalid state ({this})");
		}
		else
		{
			if (IsSubscribedOnEventBus)
			{
				return;
			}
			EventBus.Subscribe(this);
			foreach (EntityFactComponent component in m_Components)
			{
				try
				{
					component.Subscribe();
				}
				catch (Exception ex)
				{
					PFLog.EntityFact.Exception(ex);
				}
			}
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			return;
		}
		EventBus.Unsubscribe(this);
		foreach (EntityFactComponent component in m_Components)
		{
			try
			{
				component.Unsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
		IsSubscribedOnEventBus = false;
	}

	public void ViewDidAttach()
	{
		try
		{
			OnViewDidAttach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			component.ViewDidAttach();
		}
	}

	public void ViewWillDetach()
	{
		try
		{
			OnViewWillDetach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			component.ViewWillDetach();
		}
	}

	public void PreSave()
	{
		try
		{
			OnPreSave();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void PrePostLoad(EntityFactsManager manager)
	{
		Manager = manager;
		try
		{
			OnPrePostLoad();
			EntityFactService.Instance.Register(this);
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void PostLoad()
	{
		try
		{
			OnPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		List<BlueprintComponent> list = TempList.Get<BlueprintComponent>();
		Blueprint.CollectComponents(list);
		foreach (EntityFactComponent component in m_Components)
		{
			if (!component.SourceBlueprintComponentName.IsNullOrEmpty())
			{
				string componentName = component.SourceBlueprintComponentName;
				BlueprintComponent blueprintComponent = list.FirstOrDefault((BlueprintComponent i) => i.name == componentName);
				if (blueprintComponent != null)
				{
					component.Setup(blueprintComponent);
					if (component.SourceBlueprintComponent == null)
					{
						component.Dispose();
					}
				}
				else
				{
					PFLog.EntityFact.Warning($"EntityFact.PostLoad: can't find source component {componentName} ({this})");
					component.Dispose();
				}
			}
			if (!component.IsDisposed)
			{
				component.PostLoad(this);
				if (component.SourceBlueprintComponent.Disabled)
				{
					component.Deactivate();
					component.Dispose();
				}
			}
		}
		m_Components.RemoveAll((EntityFactComponent i) => i.IsDisposed);
		Setup(Blueprint);
		try
		{
			OnComponentsDidPostLoad();
		}
		catch (Exception ex2)
		{
			PFLog.EntityFact.Exception(ex2);
		}
		if (m_RemoveWhenActivatedOrPostLoaded)
		{
			m_RemoveWhenActivatedOrPostLoaded = false;
			Manager.Remove(this);
		}
	}

	public void ApplyPostLoadFixes()
	{
		foreach (EntityFactComponent component in Components)
		{
			if (component.SourceBlueprintComponent.Disabled)
			{
				PFLog.EntityFact.Log($"Removing disabled component: {component.SourceBlueprintComponent} {Blueprint.NameSafe()})");
				component.Deactivate();
				component.Dispose();
			}
			else
			{
				component.ApplyPostLoadFixes();
			}
			if (IsDisposed)
			{
				return;
			}
		}
		Components.RemoveAll((EntityFactComponent i) => i.IsDisposed);
		Setup(Blueprint);
		try
		{
			OnApplyPostLoadFixes();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void DidPostLoad()
	{
		try
		{
			OnDidPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void HoldingStateChanged()
	{
		try
		{
			OnHoldingStateChanged();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void Dispose()
	{
		bool flag = Owner?.IsDisposingNow ?? false;
		if (IsAttached && !flag)
		{
			Detach();
		}
		else
		{
			EntityFactService.Instance.Unregister(this);
			Unsubscribe();
		}
		try
		{
			OnDispose();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			try
			{
				component.Dispose();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
		}
		IsDisposed = true;
	}

	public void RemoveWhenActivatedOrPostLoaded()
	{
		if (!Activating)
		{
			IEntity owner = Owner;
			if (owner != null && owner.IsPostLoadExecuted)
			{
				PFLog.EntityFact.ErrorWithReport("EntityFact.RemoveWhenActivationEnded: invoked, but fact doesn't activating now");
				return;
			}
		}
		m_RemoveWhenActivatedOrPostLoaded = true;
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnComponentsDidActivated()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnViewDidAttach()
	{
	}

	protected virtual void OnViewWillDetach()
	{
	}

	protected virtual void OnPreSave()
	{
	}

	protected virtual void OnPrePostLoad()
	{
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual void OnDidPostLoad()
	{
	}

	protected virtual void OnApplyPostLoadFixes()
	{
	}

	protected virtual void OnHoldingStateChanged()
	{
	}

	protected virtual void OnComponentsDidPostLoad()
	{
	}

	protected virtual void OnAttach()
	{
	}

	protected virtual void OnDetach()
	{
	}

	protected virtual void OnDispose()
	{
	}

	public virtual bool IsEqual(EntityFact fact)
	{
		return this == fact;
	}

	[CanBeNull]
	public virtual IUIDataProvider SelectUIData(UIDataType type)
	{
		IUIDataProvider iUIDataProvider = Blueprint as IUIDataProvider;
		switch (type)
		{
		case UIDataType.Name:
			if (!string.IsNullOrEmpty(iUIDataProvider?.Name))
			{
				return iUIDataProvider;
			}
			break;
		case UIDataType.Description:
			if (!string.IsNullOrEmpty(iUIDataProvider?.Description))
			{
				return iUIDataProvider;
			}
			break;
		case UIDataType.Icon:
			if (iUIDataProvider?.Icon != null)
			{
				return iUIDataProvider;
			}
			break;
		case UIDataType.NameForAcronym:
			if (!string.IsNullOrEmpty(iUIDataProvider?.NameForAcronym))
			{
				return iUIDataProvider;
			}
			break;
		}
		return null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ComponentsDataHasher.GetHash128(m_ComponentsData);
		result.Append(ref val);
		List<EntityFactComponent> components = m_Components;
		if (components != null)
		{
			for (int i = 0; i < components.Count; i++)
			{
				Hash128 val2 = ClassHasher<EntityFactComponent>.GetHash128(components[i]);
				result.Append(ref val2);
			}
		}
		List<EntityFactSource> sources = m_Sources;
		if (sources != null)
		{
			for (int j = 0; j < sources.Count; j++)
			{
				Hash128 val3 = ClassHasher<EntityFactSource>.GetHash128(sources[j]);
				result.Append(ref val3);
			}
		}
		result.Append(UniqueId);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val4);
		bool val5 = IsActive;
		result.Append(ref val5);
		return result;
	}
}
public abstract class EntityFact<TEntity> : EntityFact, IHashable where TEntity : IEntity
{
	public override Type RequiredEntityType => typeof(TEntity);

	public new TEntity Owner => (TEntity)base.Owner;

	static EntityFact()
	{
		GenericStaticTypesHolder.Add(typeof(EntityFact<TEntity>));
	}

	public EntityFact()
	{
	}

	public EntityFact(BlueprintFact fact)
		: base(fact)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
