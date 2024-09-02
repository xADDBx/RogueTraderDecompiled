using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Trails;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[SelectionBase]
[KnowledgeDatabaseID("b94ee1445bc82104988a20f7ab018d36")]
public abstract class EntityViewBase : MonoBehaviour, IEntityViewBase, IFadeOutAndDestroyHandler<EntitySubscriber>, IFadeOutAndDestroyHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IFadeOutAndDestroyHandler, EntitySubscriber>
{
	[SerializeField]
	private bool m_IsInGame = true;

	[InspectorReadOnly]
	[NotNull]
	[HideInInspector]
	public string UniqueId = "";

	private bool m_IsListeningEvents;

	private bool m_VisibilitySet;

	private Action<bool> m_ChangeVisibleComplete;

	private readonly List<Renderer> m_Renderers = new List<Renderer>();

	private readonly List<Behaviour> m_VfxBehaviours = new List<Behaviour>();

	private readonly List<NavmeshCut> m_NavmeshCuts = new List<NavmeshCut>();

	protected readonly List<Collider> m_Colliders = new List<Collider>();

	private bool m_IsFadeHide;

	private bool m_GotTransform;

	private Transform m_CachedTransform;

	public string UniqueViewId
	{
		get
		{
			return UniqueId;
		}
		set
		{
			UniqueId = value;
		}
	}

	public List<NavmeshCut> NavmeshCuts => m_NavmeshCuts;

	public IEntity Data { get; private set; }

	private Entity m_Data => (Entity)Data;

	public bool IsVisible { get; private set; }

	public EntityFader Fader { get; protected set; }

	public AwarenessCheckComponent AwarenessCheckComponent { get; private set; }

	public new Transform transform
	{
		get
		{
			if (m_GotTransform)
			{
				return m_CachedTransform;
			}
			m_GotTransform = true;
			m_CachedTransform = base.transform;
			return m_CachedTransform;
		}
	}

	public bool IsInState
	{
		get
		{
			if (Data != null)
			{
				return Data.IsInState;
			}
			return false;
		}
	}

	public bool IsInGameBySettings => m_IsInGame;

	public bool IsInGame
	{
		get
		{
			if (Data != null)
			{
				return Data.IsInGame;
			}
			return false;
		}
	}

	public virtual List<Renderer> Renderers => m_Renderers;

	public List<Behaviour> VfxBehaviours => m_VfxBehaviours;

	public virtual bool IsSelectableInFogOfWar => false;

	public IEnumerable<Collider> Colliders => m_Colliders;

	public InteractionPart InteractionComponent => m_Data.GetAll<InteractionPart>().FirstOrDefault();

	public virtual bool CreatesDataOnLoad => false;

	public Transform ViewTransform => base.gameObject.transform;

	public string GameObjectName
	{
		get
		{
			return base.gameObject.name;
		}
		set
		{
			base.gameObject.name = value;
		}
	}

	public GameObject GO => base.gameObject;

	protected void UpdateCachedRenderersAndColliders(bool updateVisibility = true)
	{
		GetComponentsInChildren(includeInactive: true, m_Renderers);
		m_VfxBehaviours.Clear();
		m_VfxBehaviours.AddRange(GetComponentsInChildren<Light>(includeInactive: true));
		m_VfxBehaviours.AddRange(GetComponentsInChildren<CompositeTrailRenderer>(includeInactive: true));
		GetComponentsInChildren(includeInactive: true, m_NavmeshCuts);
		GetComponentsInChildren(includeInactive: true, m_Colliders);
		if (updateVisibility && !IsVisible && Data != null)
		{
			SetVisible(IsVisible, force: true);
		}
	}

	public void CleanupRenderersList()
	{
		m_Renderers.RemoveAll((Renderer r) => r == null);
	}

	protected virtual void Awake()
	{
		m_ChangeVisibleComplete = ChangeVisibleComplete;
		if (Application.isPlaying)
		{
			UpdateCachedRenderersAndColliders();
		}
		AwarenessCheckComponent = GetComponent<AwarenessCheckComponent>();
	}

	protected virtual void OnEnable()
	{
		if ((bool)Fader)
		{
			EntityFader fader = Fader;
			fader.OnVisibleChangedEvent = (Action<bool>)Delegate.Combine(fader.OnVisibleChangedEvent, m_ChangeVisibleComplete);
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)Fader)
		{
			EntityFader fader = Fader;
			fader.OnVisibleChangedEvent = (Action<bool>)Delegate.Remove(fader.OnVisibleChangedEvent, m_ChangeVisibleComplete);
		}
	}

	protected void StartListenEvents()
	{
		if (!m_IsListeningEvents)
		{
			EventBus.Subscribe(this);
			m_IsListeningEvents = true;
		}
	}

	protected void StopListenEvents()
	{
		if (m_IsListeningEvents)
		{
			EventBus.Unsubscribe(this);
			m_IsListeningEvents = false;
		}
	}

	public void AttachToData(IEntity data)
	{
		if (Data != data)
		{
			if (Data != null)
			{
				DetachFromData();
			}
			Data = (Entity)data;
			UniqueId = Data.UniqueId;
			StartListenEvents();
			try
			{
				SetupFogOfWarBlockerUpdaters();
				OnDidAttachToData();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			UpdateViewActive();
		}
	}

	public void DetachFromData()
	{
		if (Data != null)
		{
			StopListenEvents();
			try
			{
				OnWillDetachFromData();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			Data = null;
		}
	}

	protected virtual void OnDidAttachToData()
	{
		UpdateCachedRenderersAndColliders();
	}

	protected virtual void OnWillDetachFromData()
	{
	}

	public virtual void UpdateViewActive()
	{
		if (Data == null || m_IsFadeHide)
		{
			return;
		}
		bool flag = Data != null && m_Data.IsViewActive;
		if (flag != base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(flag);
			for (int i = 0; i < m_NavmeshCuts.Count; i++)
			{
				m_NavmeshCuts[i].enabled = flag;
			}
		}
	}

	public void SetVisible(bool visible, bool force = false, bool revealVisible = true)
	{
		if (!force && !m_VisibilitySet)
		{
			m_VisibilitySet = true;
			force = true;
		}
		bool flag = false;
		if (visible && revealVisible && !m_Data.IsRevealed)
		{
			m_Data.IsRevealed = true;
			flag = true;
		}
		bool flag2 = IsVisible != visible;
		bool num = force || flag2;
		bool flag3 = force || flag2 || flag;
		bool flag4 = force || flag2;
		if (num)
		{
			if ((bool)Fader)
			{
				Fader.Visible = visible;
				if (force)
				{
					Fader.Force();
				}
			}
			else
			{
				foreach (Renderer renderer in Renderers)
				{
					if (!(renderer == null))
					{
						renderer.enabled = visible;
					}
				}
				foreach (Behaviour vfxBehaviour in VfxBehaviours)
				{
					if (!(vfxBehaviour == null))
					{
						vfxBehaviour.enabled = visible;
					}
				}
			}
		}
		if (flag3)
		{
			bool flag5 = (IsSelectableInFogOfWar ? m_Data.IsRevealed : visible);
			foreach (Collider collider in m_Colliders)
			{
				if (collider != null && collider.gameObject.layer == 10)
				{
					collider.enabled = flag5;
				}
			}
		}
		if (flag4)
		{
			IsVisible = visible;
			OnVisibilityChanged();
		}
	}

	public void UnFade()
	{
		if (m_IsFadeHide)
		{
			m_IsFadeHide = false;
			SetVisible(visible: true, force: true);
		}
	}

	public void FadeHide()
	{
		m_IsFadeHide = true;
		SetVisible(visible: false);
	}

	private void ChangeVisibleComplete(bool value)
	{
		if (m_IsFadeHide)
		{
			m_IsFadeHide = false;
			UpdateViewActive();
		}
	}

	protected virtual void OnVisibilityChanged()
	{
		if (IsVisible)
		{
			EventBus.RaiseEvent(Data, delegate(IUnitBecameVisibleHandler h)
			{
				h.OnEntityBecameVisible();
			});
		}
		else
		{
			EventBus.RaiseEvent(Data, delegate(IUnitBecameInvisibleHandler h)
			{
				h.OnEntityBecameInvisible();
			});
		}
	}

	public abstract Entity CreateEntityData(bool load);

	protected virtual void OnDrawGizmos()
	{
		if (!Application.isPlaying && !m_IsInGame)
		{
			Gizmos.color = Color.gray;
			Gizmos.DrawLine(ViewTransform.position, ViewTransform.position + Vector3.up * 8f);
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
	}

	public void DestroyViewObject()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual void OnFadeOut()
	{
		FxHelper.SpawnFxOnGameObject(BlueprintRoot.Instance.SystemMechanics.FadeOutFx, base.gameObject);
	}

	public virtual void OnInFogOfWarChanged()
	{
	}

	protected virtual void OnDestroy()
	{
		if (Data != null && Data.View == this)
		{
			Data.DetachView();
		}
	}

	private void SetupFogOfWarBlockerUpdaters()
	{
		List<FogOfWarBlocker> list = TempList.Get<FogOfWarBlocker>();
		GetComponentsInChildren(list);
		foreach (FogOfWarBlocker item in list)
		{
			if (!TryGetComponent<FogOfWarBlockerUpdater>(out var _))
			{
				item.gameObject.AddComponent<FogOfWarBlockerUpdater>();
			}
		}
	}

	public void EntityPartComponentEnsureEntityPart(EntityPartsManager parts)
	{
		AbstractEntityPartComponent[] components = GetComponents<AbstractEntityPartComponent>();
		AbstractEntityPartComponent[] array = components;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].EnsureEntityPart();
		}
		parts.RemoveAll((ViewBasedPart i) => i.ShouldCheckSourceComponent && !components.HasItem((AbstractEntityPartComponent ii) => i.SourceType == ii.GetType().Name));
	}

	public void HandleFadeOutAndDestroy()
	{
		OnFadeOut();
	}
}
