using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps;

[KnowledgeDatabaseID("0dfa673c30fe07e4f98b465b99e2b915")]
public abstract class TrapObjectView : MapObjectView, IReloadMechanicsHandler, ISubscriber
{
	public TrapObjectView LinkedTrap;

	public SetPathToTrapMechanics LinkLine;

	public TrapObjectViewSettings Settings;

	private Transform m_ScriptZoneOriginalParent;

	public Collider Collider { get; private set; }

	[CanBeNull]
	public TrapObjectView Device { get; set; }

	public InteractionPart TrappedObject { get; set; }

	public bool IsScriptZoneTrigger => !Settings.ScriptZoneTrigger;

	public new TrapObjectData Data => (TrapObjectData)base.Data;

	protected abstract TrapObjectData CreateData();

	protected static ScriptZone SetupViewAndFindScriptZone(TrapObjectView view, string scriptZoneId, UnitAnimationInteractionType animation)
	{
		GameObject gameObject = view.gameObject;
		gameObject.layer = 10;
		gameObject.EnsureComponent<AwarenessCheckComponent>();
		FillDisableTrapSettings(gameObject, animation);
		ScriptZone scriptZone = Game.Instance.State.MapObjects.FirstOrDefault((MapObjectEntity i) => i.UniqueId == scriptZoneId)?.View as ScriptZone;
		if (scriptZone != null)
		{
			FillMeshColliderSettings(gameObject, scriptZone);
		}
		return scriptZone;
	}

	private static void FillMeshColliderSettings(GameObject go, [NotNull] ScriptZone scriptZone)
	{
		go.transform.position = scriptZone.ViewTransform.position;
		go.transform.rotation = scriptZone.ViewTransform.rotation;
		MeshCollider componentInChildren = scriptZone.GetComponentInChildren<MeshCollider>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			componentInChildren.gameObject.SetActive(value: true);
			componentInChildren.enabled = true;
			componentInChildren.GetComponent<Renderer>().enabled = true;
		}
	}

	private static void FillDisableTrapSettings(GameObject go, UnitAnimationInteractionType disarmAnimation)
	{
		go.EnsureComponent<DisableTrapInteractionComponent>().Settings = new InteractionDisableTrapSettings
		{
			ProximityRadius = 1,
			Type = InteractionType.Approach,
			UseAnimationState = disarmAnimation
		};
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		if (LinkedTrap != null)
		{
			LinkedTrap.Device = this;
		}
	}

	public override void OnAreaDidLoad()
	{
		base.OnAreaDidLoad();
		if ((bool)Settings.ScriptZoneTrigger)
		{
			Settings.ScriptZoneTrigger.OnUnitEntered.AddListener(delegate(BaseUnitEntity u, ScriptZone z)
			{
				Data.TryTriggerTrap(u);
			});
			AddHideRenderers(Settings.ScriptZoneTrigger.GetComponentsInChildren<Renderer>());
			MeshRenderer componentInChildren = Settings.ScriptZoneTrigger.GetComponentInChildren<MeshRenderer>();
			Collider = componentInChildren.Or(null)?.EnsureComponent<MeshCollider>();
			if (Collider != null)
			{
				Collider.transform.SetParent(base.ViewTransform, worldPositionStays: true);
				Collider.enabled = true;
			}
			m_ScriptZoneOriginalParent = Settings.ScriptZoneTrigger.ViewTransform.parent;
			Settings.ScriptZoneTrigger.ViewTransform.SetParent(base.ViewTransform, worldPositionStays: true);
		}
		UpdateLinkLine();
	}

	public override void OnAreaBeginUnloading()
	{
		base.OnAreaBeginUnloading();
		if ((bool)m_ScriptZoneOriginalParent)
		{
			Settings.ScriptZoneTrigger.Or(null)?.ViewTransform.SetParent(m_ScriptZoneOriginalParent, worldPositionStays: true);
			if ((bool)Collider)
			{
				Collider.transform.SetParent(Settings.ScriptZoneTrigger.ViewTransform, worldPositionStays: true);
				Collider.enabled = false;
			}
		}
	}

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		TrapObjectData trapObjectData = CreateData();
		if (load)
		{
			trapObjectData.TrapActive = false;
			trapObjectData.IsAwarenessCheckPassed = true;
			trapObjectData.IsInGame = false;
		}
		return trapObjectData;
	}

	protected override Color GetHighlightColor()
	{
		return MapObjectView.UIConfig.DefaultTrapHighlight;
	}

	protected override void OnHighlightUpdated()
	{
		TrapObjectView trapObjectView = LinkedTrap.Or(Device);
		if (trapObjectView != null)
		{
			bool highlight = base.ShouldBeHighlighted();
			trapObjectView.SetHighlight(highlight);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		(LinkLine.Or(null) ?? Device.Or(null)?.LinkLine).Or(null)?.gameObject.SetActive(value: false);
		base.OnDisable();
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		if ((bool)Settings.FxDecal)
		{
			Settings.FxDecal.enabled = base.IsVisible && (Data?.TrapActive ?? true);
		}
		UpdateLinkLine();
	}

	public void OnDeactivated()
	{
		if (Data.IsHiddenWhenInactive)
		{
			Settings.ScriptZoneTrigger.Or(null)?.ViewTransform.SetParent(m_ScriptZoneOriginalParent, worldPositionStays: true);
			Collider.Or(null)?.transform.SetParent(Settings.ScriptZoneTrigger.ViewTransform, worldPositionStays: true);
		}
		UpdateLinkLine();
		if ((bool)Settings.FxDecal)
		{
			Settings.FxDecal.enabled = false;
		}
	}

	public void PostSoundEvent(string @event)
	{
		SoundEventsManager.PostEvent(@event, base.gameObject);
	}

	public void OnBeforeMechanicsReload()
	{
	}

	public void OnMechanicsReloaded()
	{
		if ((bool)Settings.ScriptZoneTrigger && m_ScriptZoneOriginalParent == null)
		{
			Settings.ScriptZoneTrigger.OnUnitEntered.AddListener(delegate(BaseUnitEntity u, ScriptZone z)
			{
				Data.TryTriggerTrap(u);
			});
			AddHideRenderers(Settings.ScriptZoneTrigger.GetComponentsInChildren<Renderer>());
			MeshRenderer componentInChildren = Settings.ScriptZoneTrigger.GetComponentInChildren<MeshRenderer>();
			Collider = componentInChildren.Or(null)?.EnsureComponent<MeshCollider>();
			if (Collider != null)
			{
				Collider.transform.SetParent(base.ViewTransform, worldPositionStays: true);
				Collider.enabled = true;
			}
			m_ScriptZoneOriginalParent = Settings.ScriptZoneTrigger.ViewTransform.parent;
			Settings.ScriptZoneTrigger.ViewTransform.SetParent(base.ViewTransform, worldPositionStays: true);
		}
	}

	public override void OnEntityNoticed(BaseUnitEntity character)
	{
		base.OnEntityNoticed(character);
		UpdateLinkLine();
	}

	private void UpdateLinkLine()
	{
		if (!(LinkedTrap == null) || !(Device == null))
		{
			TrapObjectView trapObjectView = Device.Or(LinkedTrap);
			SetPathToTrapMechanics setPathToTrapMechanics = LinkLine.Or(trapObjectView.LinkLine);
			if (!(setPathToTrapMechanics == null) && trapObjectView.Data != null)
			{
				bool flag = Data.IsRevealed || trapObjectView.Data.IsRevealed;
				flag &= Data.IsAwarenessCheckPassed || trapObjectView.Data.IsAwarenessCheckPassed;
				flag &= Data.TrapActive || trapObjectView.Data.TrapActive;
				setPathToTrapMechanics.gameObject.SetActive(flag);
			}
		}
	}
}
