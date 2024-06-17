using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public class EntityFactsManager : IDisposable, IHashable
{
	public class FactsGameStateAdapter : JsonConverter<List<EntityFact>>
	{
		public override bool CanRead => false;

		public override List<EntityFact> ReadJson(JsonReader reader, Type objectType, List<EntityFact> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, List<EntityFact> value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			writer.WriteStartArray();
			foreach (EntityFact item in value.Where(IsItemValid))
			{
				serializer.Serialize(writer, item);
			}
			writer.WriteEndArray();
		}

		public static Hash128 GetHash128(List<EntityFact> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (EntityFact item in obj)
			{
				if (IsItemValid(item))
				{
					Hash128 val = ClassHasher<EntityFact>.GetHash128(item);
					result.Append(ref val);
				}
			}
			return result;
		}

		private static bool IsItemValid(EntityFact obj)
		{
			return obj.Blueprint?.IsGameState() ?? false;
		}
	}

	public interface IFactProcessor
	{
		bool IsActive { get; }

		bool IsSubscribedOnEventBus { get; }

		void Initialize(EntityFactsManager manager);

		bool IsSuitableFact(EntityFact fact);

		EntityFact PrepareFactForAttach(EntityFact fact);

		EntityFact PrepareFactForDetach(EntityFact fact);

		void OnFactDidAttach(EntityFact fact);

		void OnFactWillDetach(EntityFact fact);
	}

	[JsonProperty]
	[HasherCustom(Type = typeof(FactsGameStateAdapter))]
	private readonly List<EntityFact> m_Facts = new List<EntityFact>();

	private readonly EntityFactsCache m_Cache = new EntityFactsCache();

	[CanBeNull]
	private List<IFactProcessor> m_Delegates;

	public IEntity Owner { get; private set; }

	public Entity ConcreteOwner => (Entity)Owner;

	public bool IsSubscribedOnEventBus { get; private set; }

	public List<EntityFact> List => m_Facts;

	[JsonConstructor]
	public EntityFactsManager(Entity owner)
	{
		Owner = owner;
	}

	public List<TFact> GetAll<TFact>() where TFact : EntityFact
	{
		if (typeof(TFact) == typeof(EntityFact))
		{
			return m_Facts as List<TFact>;
		}
		m_Cache.Update<TFact>(m_Facts);
		return m_Cache.Get<TFact>();
	}

	public List<TFact> GetAllNotFromCache<TFact>() where TFact : EntityFact
	{
		m_Cache.Update<TFact>(m_Facts);
		List<TFact> list = new List<TFact>();
		foreach (EntityFact fact in m_Facts)
		{
			if (fact is TFact item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public IEnumerable<TFact> GetAll<TFact>(Func<TFact, bool> pred) where TFact : EntityFact
	{
		return GetAll<TFact>().Where(pred);
	}

	public bool Contains(EntityFact fact)
	{
		return m_Facts.Contains(fact);
	}

	public bool Contains(BlueprintFact blueprint)
	{
		return Contains((EntityFact i) => i.Blueprint == blueprint);
	}

	public bool Contains(Predicate<EntityFact> pred)
	{
		return m_Facts.Find(pred) != null;
	}

	[CanBeNull]
	public TFact Get<TFact>(Func<TFact, bool> pred) where TFact : class
	{
		IList orDefault = m_Cache.GetOrDefault<TFact>(m_Facts);
		for (int i = 0; i < orDefault.Count; i++)
		{
			if (orDefault[i] is TFact val && pred(val))
			{
				return val;
			}
		}
		return null;
	}

	[CanBeNull]
	public TFact Get<TFact>(BlueprintFact blueprint) where TFact : EntityFact
	{
		if (blueprint == null)
		{
			return null;
		}
		IList orDefault = m_Cache.GetOrDefault<TFact>(m_Facts);
		for (int i = 0; i < orDefault.Count; i++)
		{
			if (orDefault[i] is TFact val && val.Blueprint == blueprint)
			{
				return val;
			}
		}
		return null;
	}

	public EntityFact Get(BlueprintFact blueprint)
	{
		return Get<EntityFact>(blueprint);
	}

	public TFact GetNotFromCache<TFact>(BlueprintFact blueprint) where TFact : EntityFact
	{
		if (blueprint == null)
		{
			return null;
		}
		foreach (EntityFact fact in m_Facts)
		{
			if (fact.Blueprint == blueprint)
			{
				return fact as TFact;
			}
		}
		return null;
	}

	[CanBeNull]
	public TFact Add<TFact>([CanBeNull] TFact fact) where TFact : EntityFact
	{
		if (fact == null)
		{
			return null;
		}
		if (fact.Manager != null)
		{
			PFLog.EntityFact.Error("EntityFactsManager.Add: invalid fact state");
			return null;
		}
		if (Owner == null)
		{
			PFLog.EntityFact.Error("EntityFactsManager.Add: missing Owner");
			return null;
		}
		if (!fact.RequiredEntityType.IsAssignableFrom(Owner.GetType()))
		{
			PFLog.EntityFact.Error("EntityFactsManager.Add: invalid Owner type " + Owner.GetType().Name + " (expected " + fact.RequiredEntityType.Name + ")");
			return null;
		}
		if (ConcreteOwner.ForbidFactsAndPartsModifications && Owner.IsInitialized)
		{
			throw new Exception($"Can't add part to constant entity {Owner}");
		}
		EntityFact entityFact = DelegatePrepareFactForAttach(fact);
		if (entityFact == null)
		{
			return null;
		}
		if (m_Facts.HasItem(entityFact))
		{
			return (TFact)entityFact;
		}
		m_Facts.Add(entityFact);
		m_Cache.Add(entityFact);
		entityFact.Attach(this);
		if (entityFact.IsAttached)
		{
			DelegateOnFactDidAttach(entityFact);
		}
		return entityFact as TFact;
	}

	public void Remove(EntityFact fact, bool dispose = true)
	{
		if (fact == null)
		{
			return;
		}
		if (!fact.Activating)
		{
			IEntity owner = fact.Owner;
			if (owner != null && owner.IsPostLoadExecuted)
			{
				fact = DelegatePrepareFactForDetach(fact);
				if (fact != null && m_Facts.Remove(fact))
				{
					m_Cache.Remove(fact);
					DelegateOnFactWillDetach(fact);
					fact.Detach();
					if (dispose)
					{
						fact.Dispose();
					}
				}
				return;
			}
		}
		fact.RemoveWhenActivatedOrPostLoaded();
	}

	public void Remove(BlueprintFact blueprint)
	{
		RemoveAll((EntityFact i) => i.Blueprint == blueprint);
	}

	public void RemoveById(string id)
	{
		Remove(Get((EntityFact i) => string.Equals(i.UniqueId, id)));
	}

	public void RemoveAll<TFact>(Predicate<TFact> pred, bool dispose = true) where TFact : EntityFact
	{
		List<TFact> list = null;
		foreach (EntityFact fact in m_Facts)
		{
			if (fact is TFact val && pred(val))
			{
				if (list == null)
				{
					list = new List<TFact>();
				}
				list.Add(val);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (TFact item in list)
		{
			Remove(item, dispose);
		}
	}

	private bool HasComponent<TComponent>([CanBeNull] Func<EntityFact, EntityFactComponent, TComponent, bool> pred) where TComponent : BlueprintComponent
	{
		bool flag = typeof(IRuntimeEntityFactComponentProvider).IsAssignableFrom(typeof(TComponent));
		foreach (EntityFact fact in m_Facts)
		{
			if (flag)
			{
				foreach (EntityFactComponent component in fact.Components)
				{
					BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
					if (sourceBlueprintComponent is TComponent arg && !sourceBlueprintComponent.Disabled && (pred == null || pred(fact, component, arg)))
					{
						return true;
					}
				}
				continue;
			}
			BlueprintComponent[] componentsArray = fact.Blueprint.ComponentsArray;
			foreach (BlueprintComponent blueprintComponent in componentsArray)
			{
				if (blueprintComponent is TComponent arg2 && !blueprintComponent.Disabled && (pred == null || pred(fact, null, arg2)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasComponent<TComponent>([CanBeNull] Func<EntityFact, bool> pred = null) where TComponent : BlueprintComponent
	{
		return HasComponent((pred != null) ? ((Func<EntityFact, EntityFactComponent, TComponent, bool>)((EntityFact f, EntityFactComponent _, TComponent _) => pred(f))) : null);
	}

	public IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : BlueprintComponent
	{
		return GetComponents<TComponent>(null);
	}

	public IEnumerable<TComponent> GetComponents<TComponent>([CanBeNull] Func<TComponent, bool> pred) where TComponent : BlueprintComponent
	{
		return m_Facts.SelectMany((EntityFact i) => i.GetComponents(pred));
	}

	public T EnsureFactProcessor<T>() where T : class, IFactProcessor, new()
	{
		return ((T)(m_Delegates?.FirstItem((IFactProcessor i) => i.GetType() == typeof(T)))) ?? AddDelegate(new T());
	}

	private T AddDelegate<T>(T @delegate) where T : IFactProcessor
	{
		m_Delegates = m_Delegates ?? new List<IFactProcessor>();
		if (m_Delegates.HasItem(@delegate))
		{
			PFLog.EntityFact.Error("EntityFactsManager.AddDelegate: can't add delegate twice");
			return @delegate;
		}
		T val = m_Delegates.OfType<T>().FirstOrDefault();
		if (val != null)
		{
			PFLog.EntityFact.Error("EntityFactsManager.AddDelegate: has delegate of same type");
			return val;
		}
		m_Delegates.Add(@delegate);
		try
		{
			@delegate.Initialize(this);
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		return @delegate;
	}

	[CanBeNull]
	public IFactProcessor FirstSuitableDelegate(EntityFact fact)
	{
		if (m_Delegates == null)
		{
			return null;
		}
		foreach (IFactProcessor @delegate in m_Delegates)
		{
			if (@delegate.IsSuitableFact(fact))
			{
				return @delegate;
			}
		}
		return null;
	}

	[CanBeNull]
	private EntityFact DelegatePrepareFactForAttach(EntityFact fact)
	{
		EntityFact result = fact;
		try
		{
			IFactProcessor factProcessor = FirstSuitableDelegate(fact);
			if (factProcessor != null)
			{
				result = factProcessor.PrepareFactForAttach(fact);
			}
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		return result;
	}

	[CanBeNull]
	private EntityFact DelegatePrepareFactForDetach(EntityFact fact)
	{
		EntityFact result = fact;
		try
		{
			IFactProcessor factProcessor = FirstSuitableDelegate(fact);
			if (factProcessor != null)
			{
				result = factProcessor.PrepareFactForDetach(fact);
			}
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		return result;
	}

	private void DelegateOnFactDidAttach(EntityFact fact)
	{
		try
		{
			FirstSuitableDelegate(fact)?.OnFactDidAttach(fact);
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	private void DelegateOnFactWillDetach(EntityFact fact)
	{
		try
		{
			FirstSuitableDelegate(fact)?.OnFactWillDetach(fact);
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void ViewDidAttach()
	{
		foreach (EntityFact fact in m_Facts)
		{
			fact.ViewDidAttach();
		}
	}

	public void ViewWillDetach()
	{
		foreach (EntityFact fact in m_Facts)
		{
			fact.ViewWillDetach();
		}
	}

	public void PreSave()
	{
		foreach (EntityFact item in m_Facts.ToTempList())
		{
			if (item.Manager == this)
			{
				item.PreSave();
			}
		}
	}

	public void PrePostLoad(Entity owner)
	{
		Owner = owner;
		foreach (EntityFact fact in m_Facts)
		{
			fact.PrePostLoad(this);
		}
	}

	public void PostLoad()
	{
		m_Cache.Clear();
		foreach (EntityFact item in m_Facts.ToTempList())
		{
			if (item.Blueprint == null && SaveSystemJsonSerializer.SafeSaves)
			{
				PFLog.Default.Error("Fact " + item.GetType().Name + " had no blueprint and was removed!");
				m_Facts.Remove(item);
			}
			else
			{
				item.PostLoad();
			}
		}
	}

	public void DidPostLoad()
	{
		foreach (EntityFact fact in m_Facts)
		{
			fact.DidPostLoad();
		}
	}

	public void ApplyPostLoadFixes()
	{
		foreach (EntityFact item in m_Facts.ToTempList())
		{
			if (!item.IsDisposed)
			{
				item.ApplyPostLoadFixes();
			}
		}
	}

	public void Subscribe()
	{
		if (IsSubscribedOnEventBus)
		{
			return;
		}
		foreach (EntityFact fact in m_Facts)
		{
			if (fact.Manager == this && fact.IsActive && !fact.IsSubscribedOnEventBus)
			{
				fact.Subscribe();
			}
		}
		IsSubscribedOnEventBus = true;
	}

	public void Unsubscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			return;
		}
		foreach (EntityFact fact in m_Facts)
		{
			if (fact.Manager == this && fact.IsActive && fact.IsSubscribedOnEventBus)
			{
				fact.Unsubscribe();
			}
		}
		IsSubscribedOnEventBus = false;
	}

	public void OnHoldingStateChanged()
	{
		foreach (EntityFact fact in m_Facts)
		{
			fact.HoldingStateChanged();
		}
	}

	public void Dispose()
	{
		foreach (EntityFact item in m_Facts.ToTempList())
		{
			if (item.Manager == this)
			{
				item.Dispose();
			}
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = FactsGameStateAdapter.GetHash128(m_Facts);
		result.Append(ref val);
		return result;
	}
}
