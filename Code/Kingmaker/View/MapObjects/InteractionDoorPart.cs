using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.View.MapObjects;

public class InteractionDoorPart : InteractionPart<InteractionDoorSettings>, IHasInteractionVariantActors, IUpdatable, IInterpolatable, IHashable
{
	private const float OpenSpeed = 1f;

	private const float CloseSpeed = -1f;

	private const int PlayableIndex = 0;

	private Animator m_Animator;

	private PlayableDirector m_PlayableDirector;

	private float m_PreviousTime;

	private float m_CurrentTime;

	private bool m_IsPlayableDirectorAttached;

	[JsonProperty]
	private bool m_State;

	[JsonProperty]
	private bool m_Inited;

	private bool m_InteractThroughVariants;

	public bool IsOpen => m_State;

	public bool IsAnimationFinished
	{
		get
		{
			if (!m_IsPlayableDirectorAttached)
			{
				return true;
			}
			if (!(m_PlayableDirector.playableGraph.GetRootPlayable(0).GetSpeed() > 0.0))
			{
				return m_CurrentTime <= 0f;
			}
			return m_CurrentTime >= base.Settings.ObstacleAnimation.length;
		}
	}

	public override bool InteractThroughVariants
	{
		get
		{
			if (m_InteractThroughVariants && !AlreadyUnlocked)
			{
				return !m_State;
			}
			return false;
		}
	}

	public float OvertipCorrection => base.Settings.OvertipVerticalCorrection;

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		base.View.FogOfWarFudgeRadius = 0.5f;
		m_Animator = base.View.Or(null)?.GetComponentInChildren<Animator>();
		if (m_Animator != null)
		{
			m_Animator.fireEvents = true;
		}
		StaticRendererLink[] array = base.View.Or(null)?.GetComponents<StaticRendererLink>();
		if (array != null)
		{
			StaticRendererLink[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].DoLink();
			}
		}
		m_Animator.Or(null)?.Update(0f);
		AttachPlayableDirector();
		if (!m_Inited)
		{
			m_Inited = true;
			if (base.Settings.OpenByDefault)
			{
				Open();
			}
		}
		if ((bool)base.Settings.HideWhenOpen)
		{
			Renderer renderer = base.Settings.HideWhenOpen.FindLinkedTransform()?.GetComponent<Renderer>();
			if ((bool)renderer)
			{
				renderer.enabled = !m_State;
			}
		}
		SetNavmeshCutState();
		m_PlayableDirector.Or(null)?.Evaluate();
	}

	protected override void OnDetach()
	{
		DetachPlayableDirector();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		Open();
	}

	public override void HandleDestructionSuccess(MapObjectView mapObjectView)
	{
		base.HandleDestructionSuccess(mapObjectView);
		Open();
	}

	public void Open()
	{
		PlayOpen();
		m_State = !m_State;
		if ((bool)base.Settings.HideWhenOpen)
		{
			Renderer renderer = base.Settings.HideWhenOpen.FindLinkedTransform()?.GetComponent<Renderer>();
			if ((bool)renderer)
			{
				renderer.enabled = !m_State;
			}
		}
		string text = (m_State ? base.Settings.OpenSound : base.Settings.CloseSound);
		if (!string.IsNullOrEmpty(text))
		{
			SoundEventsManager.PostEvent(text, base.View.Or(null)?.gameObject);
		}
		if (base.Settings.DisableOnOpen)
		{
			base.Enabled = false;
		}
		EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
		{
			h.HandleObjectInteractChanged();
		}, isCheckRuntime: true);
		SetNavmeshCutState();
	}

	private void SetNavmeshCutState()
	{
		List<NavmeshCut> list = ((base.Settings.NavmeshCutAction == InteractionDoorSettings.NavMeshCutActionSettings.DoNotTouchNavmeshCut) ? null : base.View.Or(null)?.NavmeshCuts);
		if (list == null)
		{
			return;
		}
		foreach (NavmeshCut item in list)
		{
			item.enabled = base.Settings.NavmeshCutAction == InteractionDoorSettings.NavMeshCutActionSettings.EnableNavmeshCutWhenOpen == m_State;
		}
	}

	public bool GetState()
	{
		return m_State;
	}

	IEnumerable<IInteractionVariantActor> IHasInteractionVariantActors.GetInteractionVariantActors()
	{
		if (base.Type == InteractionType.Direct || !InteractThroughVariants)
		{
			return null;
		}
		IEnumerable<IInteractionVariantActor> all = base.View.Data.Parts.GetAll<IInteractionVariantActor>();
		if (all.Any((IInteractionVariantActor x) => x is KeyRestrictionPart && x.CanInteract(GameHelper.GetPlayerCharacter())))
		{
			return null;
		}
		all = all.Where((IInteractionVariantActor x) => !(x is KeyRestrictionPart));
		if (!all.Any())
		{
			return null;
		}
		return all;
	}

	protected override void ConfigureRestrictions()
	{
		TechUseRestriction component = base.View.GetComponent<TechUseRestriction>();
		if (component != null)
		{
			base.Settings.ShowOvertip = false;
			component.Settings.InteractOnlyWithToolIfFailed = true;
			base.View.Data.Parts.GetOrCreate<TechUseMultikeyItemRestrictionPart>().Settings.CopyDCData(component.Settings);
			base.View.Data.Parts.GetOrCreate<DemolitionMeltaChargeRestrictionPart>().Settings.CopyDCData(component.Settings);
			m_InteractThroughVariants = true;
		}
		LoreXenosRestriction component2 = base.View.GetComponent<LoreXenosRestriction>();
		if (component2 != null)
		{
			base.Settings.ShowOvertip = false;
			component2.Settings.InteractOnlyWithToolIfFailed = true;
			base.View.Data.Parts.GetOrCreate<LoreXenosMultikeyItemRestrictionPart>().Settings.CopyDCData(component2.Settings);
			m_InteractThroughVariants = true;
		}
	}

	void IUpdatable.Tick(float delta)
	{
		if (m_PlayableDirector.state != 0)
		{
			m_PreviousTime = m_CurrentTime;
			m_CurrentTime += ((m_PlayableDirector.playableGraph.GetRootPlayable(0).GetSpeed() > 0.0) ? delta : (0f - delta));
			m_PlayableDirector.time = m_PreviousTime;
			m_PlayableDirector.Evaluate();
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		if (m_PlayableDirector.state != 0)
		{
			float num = Mathf.LerpUnclamped(m_PreviousTime, m_CurrentTime, progress);
			if (!(Math.Abs((double)num - m_PlayableDirector.time) < 0.0001))
			{
				m_PlayableDirector.time = num;
				m_PlayableDirector.Evaluate();
			}
		}
	}

	private bool AttachPlayableDirector()
	{
		if (base.Owner.View == null || m_Animator == null || !base.View.gameObject.activeInHierarchy)
		{
			return false;
		}
		m_PlayableDirector = base.Owner.View.gameObject.EnsureComponent<PlayableDirector>();
		m_PlayableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
		TimelineAsset timelineAsset = m_PlayableDirector.playableAsset as TimelineAsset;
		if (timelineAsset == null)
		{
			timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
			m_PlayableDirector.playableAsset = timelineAsset;
		}
		AnimationTrack animationTrack = timelineAsset.CreateTrack<AnimationTrack>(null, "AnimationTrack");
		TimelineClip timelineClip = animationTrack.CreateClip(base.Settings.ObstacleAnimation);
		if (timelineClip.animationClip.hasGenericRootTransform)
		{
			animationTrack.position = m_Animator.transform.position;
			animationTrack.rotation = m_Animator.transform.rotation;
		}
		m_PlayableDirector.SetGenericBinding(animationTrack, m_Animator);
		m_Animator.runtimeAnimatorController = null;
		m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		m_PlayableDirector.time = (m_State ? timelineClip.duration : 0.0);
		m_CurrentTime = (float)m_PlayableDirector.time;
		m_PreviousTime = m_CurrentTime;
		m_PlayableDirector.Play();
		m_PlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(m_State ? 1f : (-1f));
		Game.Instance.DoorUpdateController.Add(this);
		Game.Instance.InterpolationController.Add(this);
		m_IsPlayableDirectorAttached = true;
		return true;
	}

	private void DetachPlayableDirector()
	{
		if (m_IsPlayableDirectorAttached)
		{
			m_IsPlayableDirectorAttached = false;
			Game.Instance.DoorUpdateController.Remove(this);
			Game.Instance.InterpolationController.Remove(this);
			if (!(m_PlayableDirector == null) && m_PlayableDirector.playableGraph.IsValid())
			{
				m_PlayableDirector.playableGraph.Destroy();
			}
		}
	}

	private void PlayOpen()
	{
		if (m_IsPlayableDirectorAttached || AttachPlayableDirector())
		{
			float num = (m_State ? (-1f) : 1f);
			m_PlayableDirector.time = Mathf.Clamp((float)m_PlayableDirector.time, 0f, base.Settings.ObstacleAnimation.length);
			m_CurrentTime = (float)m_PlayableDirector.time;
			m_PreviousTime = m_CurrentTime;
			m_PlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(num);
		}
	}

	[Cheat(Name = "toggle_door", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ToggleDoor(string name, bool newState)
	{
		foreach (InteractionDoorPart item in from mapObj in Game.Instance.State.MapObjects
			where mapObj.IsInGame
			where (bool)mapObj.View && mapObj.View.name.Contains(name, StringComparison.InvariantCultureIgnoreCase)
			let doorPart = mapObj.GetOptional<InteractionDoorPart>()
			where doorPart != null
			where doorPart.IsOpen != newState
			select doorPart)
		{
			item.Open();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_State);
		result.Append(ref m_Inited);
		return result;
	}
}
