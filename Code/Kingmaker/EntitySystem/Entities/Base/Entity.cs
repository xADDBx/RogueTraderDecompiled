using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.FogOfWar.Culling;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[JsonObject(MemberSerialization.OptIn)]
public abstract class Entity : IEntity, IDisposable, IHashable
{
	public class ForcePostLoad : ContextFlag<ForcePostLoad>
	{
	}

	public enum ViewHandlingOnDisposePolicyType
	{
		Destroy,
		Deactivate,
		FadeOutAndDestroy,
		Keep
	}

	public class FogOfWarTarget : ITarget
	{
		private string m_DebugString;

		private Entity m_Data;

		public int RegistryIndex { get; set; } = -1;


		public string SortOrder => m_Data.UniqueId;

		public TargetProperties Properties
		{
			get
			{
				TargetProperties result = default(TargetProperties);
				result.Center = new float2(m_Data.Position.x, m_Data.Position.z);
				result.Radius = (m_Data.View as MapObjectView)?.FogOfWarFudgeRadius ?? 0f;
				return result;
			}
		}

		public bool ForceReveal => m_Data.AlwaysRevealedInFogOfWar;

		public bool Revealed
		{
			get
			{
				return !m_Data.IsInFogOfWar;
			}
			set
			{
				m_Data.IsInFogOfWar = !value;
			}
		}

		public FogOfWarTarget(Entity data)
		{
			m_Data = data;
			m_DebugString = data.View?.ToString();
		}
	}

	private FogOfWarTarget m_FogOfWarTarget;

	[JsonProperty]
	private bool m_IsInGame = true;

	[JsonProperty]
	public readonly EntityFactsManager Facts;

	[JsonProperty]
	public readonly EntityPartsManager Parts;

	[JsonProperty]
	private bool m_IsRevealed;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private ViewHandlingOnDisposePolicyType? m_ViewHandlingOnDisposePolicyOverride;

	private bool m_IsInFogOfWar;

	private bool m_Suppressed;

	[CanBeNull]
	private SceneEntitiesState m_HoldingState;

	[JsonProperty]
	public string UniqueId { get; private set; }

	public ViewHandlingOnDisposePolicyType ViewHandlingOnDisposePolicy => m_ViewHandlingOnDisposePolicyOverride ?? DefaultViewHandlingOnDisposePolicy;

	[CanBeNull]
	public IEntityViewBase View { get; private set; }

	private EntityViewBase m_view => (EntityViewBase)View;

	public bool Destroyed { get; private set; }

	public bool IsPrePostLoadExecuted { get; private set; }

	public bool IsPostLoadExecuted { get; private set; }

	public bool IsInitialized { get; private set; }

	public bool IsDisposed { get; private set; }

	public bool IsDisposingNow { get; private set; }

	public bool IsSubscribedOnEventBus { get; private set; }

	public bool WillBeDestroyed { get; set; }

	[CanBeNull]
	public EntityServiceProxy Proxy { get; set; }

	public bool IsInCameraFrustum { get; set; } = true;


	public virtual bool NeedsView => true;

	public virtual bool ForbidFactsAndPartsModifications => false;

	public bool Suppressed
	{
		get
		{
			return m_Suppressed;
		}
		set
		{
			if (m_Suppressed != value)
			{
				m_Suppressed = value;
				if (View != null)
				{
					View.UpdateViewActive();
					UpdateFogOfWarState();
				}
				EventBus.RaiseEvent((IEntity)this, (Action<IEntitySuppressedHandler>)delegate(IEntitySuppressedHandler h)
				{
					h.HandleEntitySuppressionChanged(this, value);
				}, isCheckRuntime: true);
			}
		}
	}

	[CanBeNull]
	public virtual SceneEntitiesState HoldingState => m_HoldingState;

	public bool IsRegistered => EntityService.Instance.Contains(this);

	public bool IsInState => HoldingState != null;

	public virtual ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy
	{
		get
		{
			if (!(View.Or(null)?.GO.scene.name == "BaseMechanics"))
			{
				return ViewHandlingOnDisposePolicyType.Keep;
			}
			return ViewHandlingOnDisposePolicyType.Destroy;
		}
	}

	public bool ShouldBeEnumeratedByEntityPoolEnumerator
	{
		get
		{
			if (m_IsInGame)
			{
				return !m_Suppressed;
			}
			return false;
		}
	}

	public bool IsInGame
	{
		get
		{
			return m_IsInGame;
		}
		set
		{
			if (m_IsInGame != value)
			{
				m_IsInGame = value;
				if (View != null)
				{
					View.UpdateViewActive();
				}
				UpdateFogOfWarState();
				OnIsInGameChanged();
				EventBus.RaiseEvent((IEntity)this, (Action<IInGameHandler>)delegate(IInGameHandler h)
				{
					h.HandleObjectInGameChanged();
				}, isCheckRuntime: true);
			}
		}
	}

	public virtual Vector3 Position
	{
		get
		{
			return View?.Or(null)?.ViewTransform.position ?? Vector3.zero;
		}
		set
		{
			Transform transform = View?.Or(null)?.ViewTransform;
			if (transform != null && transform.position != value)
			{
				transform.position = value;
				OnPositionChanged();
			}
		}
	}

	public virtual bool IsViewActive => m_IsInGame;

	public bool IsVisibleForPlayer
	{
		get
		{
			if (!IsInFogOfWar && View != null && View.IsVisible)
			{
				return IsInGame;
			}
			return false;
		}
	}

	public bool IsInFogOfWar
	{
		get
		{
			return m_IsInFogOfWar;
		}
		set
		{
			if (m_IsInFogOfWar != value)
			{
				m_IsInFogOfWar = value;
				View.Or(null)?.OnInFogOfWarChanged();
			}
		}
	}

	public bool IsRevealed
	{
		get
		{
			return m_IsRevealed;
		}
		set
		{
			if (m_IsRevealed == value)
			{
				return;
			}
			m_IsRevealed = value;
			if (m_IsRevealed)
			{
				EventBus.RaiseEvent((IEntity)this, (Action<IEntityRevealedHandler>)delegate(IEntityRevealedHandler h)
				{
					h.HandleEntityRevealed();
				}, isCheckRuntime: true);
			}
		}
	}

	public virtual bool IsSuppressible => false;

	public virtual bool IsAffectedByFogOfWar => false;

	public virtual bool AlwaysRevealedInFogOfWar => false;

	public virtual bool AddToGrid => false;

	public EntityRef Ref => new EntityRef(this);

	protected void UpdateFogOfWarState()
	{
		if (m_FogOfWarTarget != null)
		{
			if (Game.Instance.FogOfWar.FowStateIsSet && !FogOfWarScheduleController.FowIsActive)
			{
				IsInFogOfWar = false;
			}
			FogOfWarCulling.UpdateTarget(m_FogOfWarTarget);
		}
	}

	protected Entity(string uniqueId, bool isInGame)
	{
		Parts = new EntityPartsManager(this);
		Facts = new EntityFactsManager(this);
		UniqueId = uniqueId;
		m_IsInGame = isInGame;
	}

	protected Entity(JsonConstructorMark _)
	{
	}

	private void Initialize()
	{
		IsPrePostLoadExecuted = true;
		IsPostLoadExecuted = true;
		Subscribe();
		EntityService.Instance?.Register(this);
		OnPrepareOrPrePostLoad();
		OnCreateParts();
		OnInitialize();
		if (!ContextData<UnitHelper.ChargenUnit>.Current)
		{
			PartUnitBody optional = GetOptional<PartUnitBody>();
			if (optional != null)
			{
				optional.Initialize();
				optional.InitializeWeapons(optional.Owner.OriginalBlueprint.Body);
			}
		}
		IsInitialized = true;
	}

	protected virtual void OnPrepareOrPrePostLoad()
	{
	}

	protected virtual void OnCreateParts()
	{
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnIsInGameChanged()
	{
	}

	public void SetHoldingState([CanBeNull] SceneEntitiesState newHoldingState)
	{
		if (m_HoldingState == newHoldingState || IsDisposed || IsDisposingNow)
		{
			return;
		}
		m_HoldingState = newHoldingState;
		Facts.OnHoldingStateChanged();
		Parts.OnHoldingStateChanged();
		try
		{
			OnHoldingStateChanged();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	public void AttachToViewOnLoad(IEntityViewBase view)
	{
		if (view != null && view.GO != null)
		{
			UniqueId = view.UniqueViewId;
		}
		else
		{
			view = CreateViewForData();
			if (view == null)
			{
				if (IsInGame && NeedsView)
				{
					IsInGame = false;
					PFLog.Default.Error("Entity data '{0}' (id={1}) failed to create a view on load", GetType().Name, UniqueId);
				}
				return;
			}
		}
		AttachView(view);
	}

	public void PrePostLoad()
	{
		if (!IsPrePostLoadExecuted || (bool)ContextData<ForcePostLoad>.Current)
		{
			IsInitialized = true;
			EntityService.Instance?.Register(this);
			Facts.PrePostLoad(this);
			Parts.PrePostLoad(this);
			try
			{
				OnPrepareOrPrePostLoad();
				OnPrePostLoad();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IsPrePostLoadExecuted = true;
		}
	}

	public void PostLoad()
	{
		if (IsPostLoadExecuted && !ContextData<ForcePostLoad>.Current)
		{
			PFLog.Entity.Error($"EntityDataBase.PostLoad: already executed ({this})");
			return;
		}
		PrePostLoad();
		try
		{
			OnPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
		Facts.PostLoad();
		Parts.PostLoad();
		try
		{
			OnDidPostLoad();
		}
		catch (Exception ex2)
		{
			PFLog.Entity.Exception(ex2);
		}
		Facts.DidPostLoad();
		Parts.DidPostLoad();
		IsPostLoadExecuted = true;
	}

	public void ApplyPostLoadFixes()
	{
		Facts.ApplyPostLoadFixes();
		Parts.ApplyPostLoadFixes();
		try
		{
			OnApplyPostLoadFixes();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
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
			PFLog.Entity.Exception(ex);
		}
		Facts.PreSave();
		Parts.PreSave();
	}

	public void Subscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			Facts.Subscribe();
			Parts.Subscribe();
			EventBus.Subscribe(this);
			try
			{
				OnSubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (IsSubscribedOnEventBus)
		{
			Facts.Unsubscribe();
			Parts.Unsubscribe();
			EventBus.Unsubscribe(this);
			try
			{
				OnUnsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IsSubscribedOnEventBus = false;
		}
	}

	public void AttachView([NotNull] IEntityViewBase view)
	{
		if (view == null)
		{
			PFLog.Default.ErrorWithReport("Try attach null view to " + ToString());
		}
		else
		{
			if (View == view)
			{
				return;
			}
			if (View != null)
			{
				DetachView();
			}
			View = view;
			View.AttachToData(this);
			m_view.EntityPartComponentEnsureEntityPart(Parts);
			AbstractEntityPartComponent[] components = m_view.GetComponents<AbstractEntityPartComponent>();
			Parts.RemoveAll((ViewBasedPart i) => i.ShouldCheckSourceComponent && !components.HasItem((AbstractEntityPartComponent ii) => i.SourceType == ii.GetType().Name));
			try
			{
				OnViewDidAttach();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			Parts.ViewDidAttach();
			Facts.ViewDidAttach();
			EventBus.RaiseEvent((IEntity)this, (Action<IViewAttachedHandler>)delegate(IViewAttachedHandler h)
			{
				h.OnViewAttached(view);
			}, isCheckRuntime: true);
		}
	}

	public void DetachView()
	{
		if (View != null)
		{
			EventBus.RaiseEvent((IEntity)this, (Action<IViewDetachedHandler>)delegate(IViewDetachedHandler h)
			{
				h.OnViewDetached(View);
			}, isCheckRuntime: true);
			Parts.ViewWillDetach();
			Facts.ViewWillDetach();
			try
			{
				OnViewWillDetach();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			View.Or(null)?.DetachFromData();
			View = null;
		}
	}

	public void HandleDestroy()
	{
		if (Destroyed)
		{
			return;
		}
		if (HoldingState != null)
		{
			PFLog.Default.ErrorWithReport("It is unsafe to destroy entities which still in game state");
		}
		try
		{
			OnDestroy();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
		finally
		{
			Destroyed = true;
		}
		EventBus.ClearEntitySubscriptions(this);
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			DisposeImplementation();
		}
	}

	protected virtual void DisposeImplementation()
	{
		if (HoldingState != null)
		{
			if (Application.isPlaying)
			{
				PFLog.Entity.ErrorWithReport("Trying to Dispose entity which still in the game state");
			}
			HoldingState.RemoveEntityData(this);
		}
		Unsubscribe();
		try
		{
			IsDisposingNow = true;
			try
			{
				OnDispose();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IEntityViewBase view = View;
			ViewHandlingOnDisposePolicyType viewHandlingOnDisposePolicyType = m_ViewHandlingOnDisposePolicyOverride ?? DefaultViewHandlingOnDisposePolicy;
			DetachView();
			if (view != null)
			{
				switch (viewHandlingOnDisposePolicyType)
				{
				case ViewHandlingOnDisposePolicyType.Destroy:
				case ViewHandlingOnDisposePolicyType.FadeOutAndDestroy:
					view.DestroyViewObject();
					break;
				case ViewHandlingOnDisposePolicyType.Deactivate:
					view.GO.SetActive(value: false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case ViewHandlingOnDisposePolicyType.Keep:
					break;
				}
			}
			Parts.Dispose();
			Facts.Dispose();
		}
		finally
		{
			EntityService.Instance?.Unregister(this);
			IsDisposed = true;
			IsDisposingNow = false;
		}
	}

	protected virtual void OnHoldingStateChanged()
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

	protected virtual void OnPreSave()
	{
	}

	protected virtual void OnSubscribe()
	{
	}

	protected virtual void OnUnsubscribe()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected virtual void OnViewDidAttach()
	{
		if (Application.isPlaying && IsAffectedByFogOfWar)
		{
			if (m_FogOfWarTarget == null)
			{
				m_FogOfWarTarget = new FogOfWarTarget(this);
			}
			IsInFogOfWar = !Game.Instance.FogOfWar.FowStateIsSet || FogOfWarScheduleController.FowIsActive;
			FogOfWarCulling.RegisterTarget(m_FogOfWarTarget);
		}
	}

	protected virtual void OnViewWillDetach()
	{
		if (m_FogOfWarTarget != null && Application.isPlaying)
		{
			FogOfWarCulling.UnregisterTarget(m_FogOfWarTarget);
			m_FogOfWarTarget = null;
		}
	}

	protected abstract IEntityViewBase CreateViewForData();

	public IEntity GetSubscribingEntity()
	{
		return this;
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		string value = ((View != null) ? View.GO.name : "-");
		builder.Append(GetType().Name);
		builder.Append('[');
		builder.Append(value);
		builder.Append("]#");
		builder.Append(UniqueId);
		return builder.ToString();
	}

	protected void OnPositionChanged()
	{
		using (Counters.EntityPositionChanged?.Measure())
		{
			if (m_FogOfWarTarget != null && !AlwaysRevealedInFogOfWar)
			{
				FogOfWarCulling.UpdateTarget(m_FogOfWarTarget);
			}
			EventBus.RaiseEvent((IEntity)this, (Action<IEntityPositionChangedHandler>)delegate(IEntityPositionChangedHandler h)
			{
				h.HandleEntityPositionChanged();
			}, isCheckRuntime: true);
		}
	}

	[CanBeNull]
	public TPart GetOptional<TPart>() where TPart : EntityPart, new()
	{
		return Parts.GetOptional<TPart>();
	}

	[CanBeNull]
	public TPart GetOptional<TPart>(Type type) where TPart : EntityPart
	{
		return Parts.GetOptional<TPart>(type);
	}

	[NotNull]
	public TPart GetRequired<TPart>() where TPart : EntityPart, new()
	{
		return Parts.GetRequired<TPart>();
	}

	public TPart GetOrCreate<TPart>() where TPart : EntityPart, new()
	{
		return Parts.GetOrCreate<TPart>();
	}

	public void Remove<TPart>() where TPart : EntityPart, new()
	{
		Parts.Remove<TPart>();
	}

	public IEnumerable<TPart> GetAll<TPart>() where TPart : class
	{
		return Parts.GetAll<TPart>();
	}

	public void AssertRequiredPart<TPart>() where TPart : EntityPart, new()
	{
		GetRequired<TPart>();
	}

	public static TEntity Initialize<TEntity>(TEntity entity) where TEntity : Entity
	{
		entity.Initialize();
		return entity;
	}

	public void SetViewHandlingOnDisposePolicy(ViewHandlingOnDisposePolicyType policy)
	{
		m_ViewHandlingOnDisposePolicyOverride = policy;
	}

	public static implicit operator Entity(EntityRef @ref)
	{
		return (Entity)@ref.Entity;
	}

	public static implicit operator EntityReference([CanBeNull] Entity entity)
	{
		return new EntityReference
		{
			UniqueId = entity?.UniqueId
		};
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(UniqueId);
		result.Append(ref m_IsInGame);
		Hash128 val = ClassHasher<EntityFactsManager>.GetHash128(Facts);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<EntityPartsManager>.GetHash128(Parts);
		result.Append(ref val2);
		result.Append(ref m_IsRevealed);
		if (m_ViewHandlingOnDisposePolicyOverride.HasValue)
		{
			ViewHandlingOnDisposePolicyType val3 = m_ViewHandlingOnDisposePolicyOverride.Value;
			result.Append(ref val3);
		}
		return result;
	}
}
