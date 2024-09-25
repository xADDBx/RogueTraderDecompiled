using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem;

public abstract class EntityFactsProcessor : EntityFactsManager.IFactProcessor
{
	public EntityFactsManager Manager { get; private set; }

	public bool IsActive { get; protected set; } = true;


	public bool IsSubscribedOnEventBus { get; protected set; } = true;


	public void Initialize(EntityFactsManager manager)
	{
		Manager = manager;
	}

	public abstract bool IsSuitableFact(EntityFact fact);

	public abstract EntityFact PrepareFactForAttach(EntityFact fact);

	public abstract EntityFact PrepareFactForDetach(EntityFact fact);

	public abstract void OnFactDidAttach(EntityFact fact);

	public abstract void OnFactWillDetach(EntityFact fact);

	public abstract void OnFactDidDetached(EntityFact fact);

	protected virtual void OnDispose()
	{
	}

	public void Dispose()
	{
		try
		{
			OnDispose();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}
}
public abstract class EntityFactsProcessor<TFact> : EntityFactsProcessor where TFact : EntityFact
{
	public List<TFact> RawFacts => base.Manager?.GetAll<TFact>();

	[CanBeNull]
	public TFact Get(BlueprintFact blueprint)
	{
		if (RawFacts == null)
		{
			return null;
		}
		foreach (TFact rawFact in RawFacts)
		{
			if (rawFact.Blueprint == blueprint)
			{
				return rawFact;
			}
		}
		return null;
	}

	public void SetActive(bool active)
	{
		if (base.IsActive == active)
		{
			return;
		}
		base.IsActive = active;
		foreach (TFact item in RawFacts.ToTempList())
		{
			if (active && !item.IsActive)
			{
				item.Activate();
			}
			else if (!active && item.IsActive)
			{
				item.Deactivate();
			}
		}
	}

	public void SetActiveForce(bool active)
	{
		base.IsActive = active;
		foreach (TFact item in RawFacts.ToTempList())
		{
			if (active && !item.IsActive)
			{
				item.Activate();
			}
			else if (!active && item.IsActive)
			{
				item.Deactivate();
			}
		}
	}

	public void SetSubscribedOnEventBus(bool subscribed)
	{
		if (base.IsSubscribedOnEventBus == subscribed)
		{
			return;
		}
		base.IsSubscribedOnEventBus = subscribed;
		foreach (TFact item in RawFacts.ToTempList())
		{
			if (subscribed && !item.IsSubscribedOnEventBus)
			{
				item.Subscribe();
			}
			else if (!subscribed && item.IsSubscribedOnEventBus)
			{
				item.Unsubscribe();
			}
		}
	}

	public override bool IsSuitableFact(EntityFact fact)
	{
		return fact is TFact;
	}

	public sealed override EntityFact PrepareFactForAttach(EntityFact fact)
	{
		if (fact is TFact fact2)
		{
			return PrepareFactForAttach(fact2);
		}
		return null;
	}

	public sealed override EntityFact PrepareFactForDetach(EntityFact fact)
	{
		if (fact is TFact fact2)
		{
			return PrepareFactForDetach(fact2);
		}
		return null;
	}

	public sealed override void OnFactDidAttach(EntityFact fact)
	{
		if (fact is TFact fact2)
		{
			OnFactDidAttach(fact2);
		}
		EventBus.RaiseEvent(delegate(IFactCollectionUpdatedHandler h)
		{
			h.HandleFactCollectionUpdated(this);
		});
	}

	public sealed override void OnFactWillDetach(EntityFact fact)
	{
		if (fact is TFact fact2)
		{
			OnFactWillDetach(fact2);
		}
	}

	public sealed override void OnFactDidDetached(EntityFact fact)
	{
		if (fact is TFact fact2)
		{
			OnFactDidDetached(fact2);
		}
		EventBus.RaiseEvent(delegate(IFactCollectionUpdatedHandler h)
		{
			h.HandleFactCollectionUpdated(this);
		});
	}

	protected abstract TFact PrepareFactForAttach(TFact fact);

	protected abstract TFact PrepareFactForDetach(TFact fact);

	protected abstract void OnFactDidAttach(TFact fact);

	protected abstract void OnFactWillDetach(TFact fact);

	protected abstract void OnFactDidDetached(TFact fact);

	public IEnumerable<T> SelectComponents<T>() where T : class
	{
		foreach (TFact rawFact in RawFacts)
		{
			foreach (EntityFactComponent component in rawFact.Components)
			{
				T val = (component as T) ?? (component.SourceBlueprintComponent as T);
				if (val != null)
				{
					yield return val;
				}
			}
		}
	}

	public bool Contains(BlueprintFact fact)
	{
		return RawFacts.HasItem((TFact i) => i.Blueprint == fact);
	}

	public bool Contains(TFact fact)
	{
		return RawFacts.HasItem(fact);
	}

	public void Remove(EntityFact fact)
	{
		base.Manager?.Remove(fact);
	}

	public void Remove(BlueprintFact blueprint)
	{
		EntityFact fact = base.Manager?.List.FirstItem((EntityFact i) => i.Blueprint == blueprint);
		base.Manager?.Remove(fact);
	}
}
