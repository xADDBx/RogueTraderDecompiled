using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GameConst;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.Mechanics;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[AddComponentMenu("Map Object View")]
[KnowledgeDatabaseID("037fe06a751be534fa04d8b0764331d1")]
public class MapObjectView : MechanicEntityView, IDetectHover, IEntitySubscriber, IAwarenessHandler<EntitySubscriber>, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IEventTag<IAwarenessHandler, EntitySubscriber>, IAreaHandler, IResource
{
	[SerializeField]
	private List<Renderer> m_HideRenderers;

	private Highlighter m_Highlighter;

	private FactHolder m_FactHolder;

	private CountingGuard m_Highlighted = new CountingGuard();

	private bool m_ForcedHighlightOnReveal;

	public bool AreaHighlighted;

	public override bool CreatesDataOnLoad => true;

	protected static UIConfig UIConfig => BlueprintRoot.Instance.UIConfig;

	public FactHolder FactHolder => m_FactHolder ?? (m_FactHolder = GetComponent<FactHolder>());

	public new MapObjectEntity Data => (MapObjectEntity)base.Data;

	protected virtual bool IsClickable
	{
		get
		{
			if (!SupportInteraction(InteractionType.Direct))
			{
				return SupportInteraction(InteractionType.Approach);
			}
			return true;
		}
	}

	protected virtual bool HasHighlight => IsClickable;

	public virtual bool CanBeAttackedDirectly => false;

	protected virtual bool GlobalHighlighting => Game.Instance.InteractionHighlightController?.IsHighlighting ?? false;

	protected virtual bool HighlightOnHover
	{
		get
		{
			if (Data.IsRevealed)
			{
				if (!CanBeAttackedDirectly)
				{
					return Data.Parts.GetAll<InteractionPart>().Any(ShouldHighlightInteraction);
				}
				return true;
			}
			return false;
			static bool ShouldHighlightInteraction(InteractionPart i)
			{
				InteractionType type = i.Type;
				if ((type == InteractionType.Approach || type == InteractionType.Direct) && i.Enabled)
				{
					if (i.Settings.ShowOvertip)
					{
						if (i.Settings.ShowOvertip)
						{
							return i.Settings.ShowHighlight;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}
	}

	public override List<Renderer> Renderers
	{
		get
		{
			List<Renderer> obj = m_HideRenderers ?? new List<Renderer>();
			List<Renderer> result = obj;
			m_HideRenderers = obj;
			return result;
		}
	}

	public override bool IsSelectableInFogOfWar => true;

	public bool Highlighted
	{
		get
		{
			return m_Highlighted;
		}
		set
		{
			if ((bool)Data?.GetOptional<AreaTransitionPart>() && m_Highlighted.SetValue(value))
			{
				AreaHighlighted = value;
			}
			if ((bool)m_Highlighter && m_Highlighted.SetValue(value))
			{
				UpdateHighlight();
			}
			EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectHighlightChange();
			}, isCheckRuntime: true);
		}
	}

	protected Highlighter Highlighter => m_Highlighter;

	public virtual float FogOfWarFudgeRadius { get; set; }

	public IEntity GetSubscribingEntity()
	{
		return Data;
	}

	public bool IsOwnerOf(EntityFactComponent component)
	{
		if ((bool)FactHolder && component.Fact != null)
		{
			return component.Fact == FactHolder.GetFact();
		}
		return false;
	}

	private bool SupportInteraction(InteractionType type)
	{
		return Data?.Parts.GetAll<InteractionPart>().Any((InteractionPart interaction) => interaction.Type == type) ?? GetComponents<AbstractEntityPartComponent>().OfType<IInteractionComponent>().Any((IInteractionComponent i) => i.Settings.Type == type);
	}

	public virtual bool SupportBlueprint(BlueprintMapObject blueprint)
	{
		return blueprint != null;
	}

	public virtual void ApplyBlueprint(BlueprintMapObject blueprint)
	{
		if (!SupportBlueprint(blueprint))
		{
			throw new Exception("Blueprint is not supported");
		}
		base.gameObject.EnsureComponent<FactHolder>().Blueprint = blueprint;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!Application.isPlaying)
		{
			return;
		}
		if (HasHighlight)
		{
			m_Highlighter = this.EnsureComponent<Highlighter>();
		}
		List<Transform> list = ListPool<Transform>.Claim();
		GetComponentsInChildren(list);
		foreach (Transform item in list)
		{
			if (item.gameObject.layer == 9)
			{
				item.gameObject.layer = 10;
			}
		}
		ListPool<Transform>.Release(list);
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		UpdateHighlight();
	}

	public override void OnInFogOfWarChanged()
	{
		base.OnInFogOfWarChanged();
		UpdateHighlight();
	}

	public void UpdateHighlight()
	{
		if ((bool)m_Highlighter)
		{
			SetHighlight(ShouldBeHighlighted());
			OnHighlightUpdated();
		}
	}

	protected virtual bool ShouldBeHighlighted()
	{
		bool flag = Highlighted || m_ForcedHighlightOnReveal || (GlobalHighlighting && !Data.IsInFogOfWar);
		if (Game.Instance.TurnController.TurnBasedModeActive && Data.Parts.GetAll<InteractionPart>().Any((InteractionPart i) => i is InteractionLootPart))
		{
			flag = false;
		}
		if (!flag || !HighlightOnHover || !Data.IsRevealed || !Data.IsAwarenessCheckPassed)
		{
			return Data.Parts.GetAll<InteractionPart>().Any((InteractionPart i) => i.HasVisibleTrap());
		}
		return true;
	}

	protected virtual Color GetHighlightColor()
	{
		bool flag = Data.View.AwarenessCheckComponent != null;
		bool flag2 = Data.View.InteractionComponent != null;
		bool num = Data.Parts.GetAll<InteractionPart>().Any((InteractionPart i) => i.HasVisibleTrap());
		Color result = UIConfig.DefaultHighlight;
		if (num)
		{
			result = (Highlighted ? UIConfig.HighlightedTrapedLoot : UIConfig.TrapedLoot);
		}
		else if (m_ForcedHighlightOnReveal)
		{
			result = ((flag2 && !flag) ? UIConfig.StandartLootColorPercepted : UIConfig.PerceptedLootColor);
		}
		else if (flag2)
		{
			if (Data.GetAll<InteractionPart>() is InteractionLootPart interactionLootPart)
			{
				result = (interactionLootPart.LootViewed ? UIConfig.VisitedLootColor : UIConfig.StandartLootColor);
			}
		}
		else
		{
			result = UIConfig.InteractionHighlight;
		}
		return result;
	}

	protected void SetHighlight(bool on)
	{
		if (on)
		{
			Color highlightColor = GetHighlightColor();
			m_Highlighter.ConstantOn(highlightColor, 0f);
		}
		else
		{
			m_Highlighter.ConstantOff(0f);
		}
	}

	protected virtual void OnHighlightUpdated()
	{
	}

	public override Entity CreateEntityData(bool load)
	{
		return CreateMapObjectEntityData(load);
	}

	protected virtual MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new MapObjectEntity(this));
	}

	public virtual void HandleHoverChange(bool isHover)
	{
		AreaHighlighted = Data?.GetOptional<AreaTransitionPart>();
		if (AreaHighlighted)
		{
			EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectHighlightChange();
			}, isCheckRuntime: true);
		}
		else if (!(m_Highlighter == null))
		{
			m_ForcedHighlightOnReveal = false;
			Highlighted = isHover;
			Game.Instance.CursorController.SetMapObjectCursor(this, isHover);
			EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectHighlightChange();
			}, isCheckRuntime: true);
		}
	}

	public virtual void OnEntityNoticed(BaseUnitEntity character)
	{
		if ((bool)m_Highlighter)
		{
			if (Data.Parts.GetAll<InteractionPart>().Any((InteractionPart ic) => ic.Enabled && (ic.Type == InteractionType.Approach || ic.Type == InteractionType.Direct)))
			{
				bool highlightPermanent = Data.View.AwarenessCheckComponent != null;
				ForceHighlightOnReveal(highlightPermanent);
			}
			if (Data.Parts.GetAll<InteractionPart>().Any((InteractionPart ic) => ic.Enabled && (bool)ic.Settings.Trap && ic.Settings.Trap == this))
			{
				UpdateHighlight();
			}
		}
	}

	public void ForceHighlightOnReveal(bool highlightPermanent)
	{
		Data.WasHighlightedOnReveal = true;
		m_ForcedHighlightOnReveal = true;
		UpdateHighlight();
		if (!highlightPermanent)
		{
			StartCoroutine(SwitchOffHighlight());
		}
	}

	public IEnumerator SwitchOffHighlight()
	{
		yield return new WaitForSeconds(UIConsts.RevealObjectHighlightTime);
		m_ForcedHighlightOnReveal = false;
		UpdateHighlight();
	}

	public void ReinitHighlighterMaterials()
	{
		m_Highlighter.Or(null)?.ReinitMaterials();
	}

	public virtual void OnAreaBeginUnloading()
	{
	}

	public virtual void OnAreaDidLoad()
	{
		UpdateHighlight();
	}

	public void AddHideRenderer(Renderer newRenderer)
	{
		if ((bool)newRenderer)
		{
			m_HideRenderers = m_HideRenderers ?? new List<Renderer>();
			m_HideRenderers.Add(newRenderer);
			SetVisible(base.IsVisible, force: true);
		}
	}

	protected void AddHideRenderers(IEnumerable<Renderer> renderers)
	{
		if (!renderers.Empty())
		{
			m_HideRenderers = m_HideRenderers ?? new List<Renderer>();
			m_HideRenderers.AddRange(renderers);
			SetVisible(base.IsVisible, force: true);
		}
	}
}
