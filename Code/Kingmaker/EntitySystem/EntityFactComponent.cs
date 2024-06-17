using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public class EntityFactComponent : IDisposable, IHashable
{
	public virtual Type RequiredEntityType => typeof(IEntity);

	public EntityFact Fact { get; private set; }

	[JsonProperty]
	public string SourceBlueprintComponentName { get; private set; }

	public BlueprintComponent SourceBlueprintComponent { get; private set; }

	public bool IsSubscribedOnEventBus { get; private set; }

	public IEntity Owner => Fact.Owner;

	public bool IsActive
	{
		get
		{
			if (Fact != null)
			{
				return Fact.IsActive;
			}
			return false;
		}
	}

	public bool IsDisposed { get; private set; }

	public virtual IEntity GetSubscribingEntity()
	{
		IEntity entity = (Owner as ItemEntity)?.Wielder;
		return entity ?? Owner;
	}

	[JsonConstructor]
	public EntityFactComponent()
	{
	}

	public virtual void Setup(BlueprintComponent component)
	{
		IsDisposed = false;
		SourceBlueprintComponent = component;
		SourceBlueprintComponentName = component.name;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(SourceBlueprintComponentName))
		{
			return GetType().Name + "[" + SourceBlueprintComponentName + "]";
		}
		return GetType().Name ?? "";
	}

	[NotNull]
	public virtual TData GetData<TData>() where TData : class
	{
		throw new NotImplementedException();
	}

	[NotNull]
	public TData RequestSavableData<TData>() where TData : IEntityFactComponentSavableData, new()
	{
		return Fact.RequestSavableData<TData>(SourceBlueprintComponent);
	}

	[NotNull]
	public TData RequestTransientData<TData>() where TData : IEntityFactComponentTransientData, new()
	{
		return Fact.RequestTransientData<TData>(SourceBlueprintComponent);
	}

	public void Initialize(EntityFact fact)
	{
		Fact = fact;
		try
		{
			OnInitialize();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public virtual void OnFactAttached()
	{
	}

	public virtual void OnFactDetached()
	{
	}

	public void Activate()
	{
		try
		{
			OnActivate();
			OnActivateOrPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		if (Fact == null)
		{
			PFLog.EntityFact.Error("Trying to activate null Fact");
		}
		else if (Fact.IsSubscribedOnEventBus)
		{
			Subscribe();
		}
	}

	public void Deactivate()
	{
		Unsubscribe();
		try
		{
			OnDeactivate();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void Subscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			EventBus.Subscribe(this);
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (IsSubscribedOnEventBus)
		{
			EventBus.Unsubscribe(this);
			IsSubscribedOnEventBus = false;
		}
	}

	public void PostLoad(EntityFact owner)
	{
		Fact = owner;
		try
		{
			OnPostLoad();
			if (Fact.IsActive)
			{
				OnActivateOrPostLoad();
			}
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void ApplyPostLoadFixes()
	{
		try
		{
			OnApplyPostLoadFixes();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
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
	}

	public void Dispose()
	{
		Unsubscribe();
		try
		{
			OnDispose();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		SourceBlueprintComponent = null;
		IsDisposed = true;
	}

	public void Recalculate()
	{
		try
		{
			OnRecalculate();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnActivateOrPostLoad()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnPreSave()
	{
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual void OnApplyPostLoadFixes()
	{
	}

	protected virtual void OnViewDidAttach()
	{
	}

	protected virtual void OnViewWillDetach()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected virtual void OnRecalculate()
	{
		OnDeactivate();
		OnActivate();
		OnActivateOrPostLoad();
	}

	public virtual IDisposable RequestEventContext()
	{
		return null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(SourceBlueprintComponentName);
		return result;
	}
}
public abstract class EntityFactComponent<TEntity> : EntityFactComponent, IHashable where TEntity : IEntity
{
	public sealed override Type RequiredEntityType => typeof(TEntity);

	public new TEntity Owner => (TEntity)base.Owner;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
public abstract class EntityFactComponent<TEntity, TBlueprintComponent> : EntityFactComponent<TEntity>, IHashable where TEntity : Entity where TBlueprintComponent : BlueprintComponent
{
	public new TBlueprintComponent SourceBlueprintComponent => (TBlueprintComponent)base.SourceBlueprintComponent;

	public TBlueprintComponent Settings => SourceBlueprintComponent;

	public override void Setup(BlueprintComponent component)
	{
		if (!(component is TBlueprintComponent))
		{
			LogChannel.System.Error($"EntityFactComponent<{typeof(TEntity).Name}, {typeof(TBlueprintComponent).Name}>.Setup: invalid component type {component.GetType().Name}");
		}
		else
		{
			base.Setup(component);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
