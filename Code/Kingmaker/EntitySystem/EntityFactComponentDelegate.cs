using System;
using System.Collections.Generic;
using System.Reflection;
using Code.GameCore.ElementsSystem;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[TypeId("7fee73fef2544a278ec147707330d9a7")]
public abstract class EntityFactComponentDelegate<TEntity> : BlueprintComponent, IRuntimeEntityFactComponentProvider, ISubscriber, IOverrideOnActivateMethod, IHashable where TEntity : Entity
{
	protected class ComponentEventContext : ContextData<ComponentEventContext>
	{
		private ComponentRuntime m_Runtime;

		public static ComponentRuntime CurrentRuntime => ContextData<ComponentEventContext>.Current?.m_Runtime ?? throw new Exception("ComponentRuntime is unavailable in current context");

		public ComponentEventContext Setup(ComponentRuntime runtime)
		{
			m_Runtime = runtime;
			return this;
		}

		protected override void Reset()
		{
			m_Runtime = null;
		}
	}

	public class ComponentRuntime : EntityFactComponent<TEntity>, ISubscriptionProxy, IHashable
	{
		private EntityFactComponentDelegate<TEntity> Delegate => (EntityFactComponentDelegate<TEntity>)base.SourceBlueprintComponent;

		public FeatureParam Param => (base.Fact as Feature)?.Param;

		public bool IsReapplying => (base.Fact as Feature)?.IsReapplying ?? false;

		public override void Setup(BlueprintComponent component)
		{
			if (!(component is EntityFactComponentDelegate<TEntity>))
			{
				LogChannel.System.Error("EntityFactComponentDelegate<{0}>.Runtime.Setup: invalid component type {1}", typeof(TEntity).Name, component.GetType().Name);
			}
			else
			{
				base.Setup(component);
			}
		}

		public override void OnFactAttached()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnFactAttached();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		public override void OnFactDetached()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnFactDetached();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnInitialize()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnInitialize();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnActivate()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnActivate();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnActivateOrPostLoad()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnActivateOrPostLoad();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnDeactivate()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnDeactivate();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnPostLoad()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnPostLoad();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnApplyPostLoadFixes()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnApplyPostLoadFixes();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnViewDidAttach()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnViewDidAttach();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnViewWillDetach()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnViewWillDetach();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnDispose()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnDispose();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		[Obsolete]
		protected override void OnRecalculate()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				using (RequestEventContext())
				{
					Delegate.OnRecalculate();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		public ISubscriber GetSubscriber()
		{
			return Delegate;
		}

		public override IDisposable RequestEventContext()
		{
			return ContextData<ComponentEventContext>.Request().Setup(this);
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public virtual bool IsOverrideOnActivateMethod
	{
		get
		{
			bool flag = false;
			Type type = GetType();
			while (type != null && type != typeof(EntityFactComponentDelegate) && !flag)
			{
				flag = type.GetMethod("OnActivate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) != null;
				type = type.BaseType;
			}
			return flag;
		}
	}

	protected int ExecutesCount { get; set; }

	public ComponentRuntime Runtime => ComponentEventContext.CurrentRuntime;

	[NotNull]
	protected TEntity Owner => ComponentEventContext.CurrentRuntime.Owner;

	protected EntityFact Fact => ComponentEventContext.CurrentRuntime.Fact;

	protected MechanicsContext Context => ComponentEventContext.CurrentRuntime.Fact.MaybeContext;

	protected bool IsReapplying => ComponentEventContext.CurrentRuntime.IsReapplying;

	[NotNull]
	protected TSavableData RequestSavableData<TSavableData>() where TSavableData : IEntityFactComponentSavableData, new()
	{
		return Fact.RequestSavableData<TSavableData>(Runtime);
	}

	[NotNull]
	protected TTransientData RequestTransientData<TTransientData>() where TTransientData : IEntityFactComponentTransientData, new()
	{
		return Fact.RequestTransientData<TTransientData>(Runtime);
	}

	public IEntity GetSubscribingEntity()
	{
		return null;
	}

	public virtual EntityFactComponent CreateRuntimeFactComponent()
	{
		return new ComponentRuntime();
	}

	protected virtual void OnFactAttached()
	{
	}

	protected virtual void OnFactDetached()
	{
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

	[Obsolete]
	protected virtual void OnRecalculate()
	{
		OnDeactivate();
		OnActivate();
		OnActivateOrPostLoad();
	}

	protected void RemoveAllFactsOriginatedFromThisComponent([CanBeNull] Entity factsOwner)
	{
		if (factsOwner == null)
		{
			return;
		}
		List<EntityFact> list = null;
		foreach (EntityFact item in factsOwner.Facts.List)
		{
			if (item.IsFrom(Fact, this))
			{
				list = list ?? TempList.Get<EntityFact>();
				list.Add(item);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityFact item2 in list)
		{
			factsOwner.Facts.Remove(item2);
		}
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
[TypeId("7fee73fef2544a278ec147707330d9a7")]
public abstract class EntityFactComponentDelegate : EntityFactComponentDelegate<Entity>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
