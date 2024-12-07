using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour, IInterpolatable
{
	[NonSerialized]
	public bool IsInDollRoom;

	private string m_Name;

	private bool m_IsInitialized;

	private Animator m_Animator;

	private PlayableGraph m_Graph;

	private AnimationActionHandle m_CurrentAction;

	private readonly List<AnimationActionHandle> m_ActiveActions = new List<AnimationActionHandle>();

	private readonly Queue<AnimationActionHandle> m_SequencedActions = new Queue<AnimationActionHandle>();

	private readonly List<MixerInfo> m_Mixers = new List<MixerInfo>();

	private readonly List<AnimationBase> m_ActiveAnimations = new List<AnimationBase>();

	private readonly Dictionary<AnimationActionBase, List<Transition>> m_FromTransitions = new Dictionary<AnimationActionBase, List<Transition>>();

	private readonly Dictionary<AnimationActionBase, List<Transition>> m_ToTransitions = new Dictionary<AnimationActionBase, List<Transition>>();

	private readonly Dictionary<AnimatorControllerPlayable, PlayableInfo> m_AnimatorControllerPlayableToPlayableInfo = new Dictionary<AnimatorControllerPlayable, PlayableInfo>();

	private AnimationPlayableOutput m_Output;

	private MixerInfo m_DefaultMixer;

	private float m_DefaultMixerSpeed = 1f;

	private CountableFlag m_RotationForbidden = new CountableFlag();

	private readonly CountingGuard m_Disabled = new CountingGuard();

	private int m_LastUpdateTick;

	[SerializeField]
	private AnimationSet m_AnimationSet;

	[SerializeField]
	private bool m_FireEvents = true;

	public StatefulRandom StatefulRandom
	{
		get
		{
			if (!IsInDollRoom)
			{
				return PFStatefulRandom.Visuals.Animation3;
			}
			return PFStatefulRandom.Visuals.DollRoom;
		}
	}

	private static int CurrentUpdateTick => Game.Instance.RealTimeController.CurrentSystemStepIndex;

	public bool IsRotationForbidden => m_RotationForbidden;

	public float DefaultMixerSpeed
	{
		get
		{
			if (!Disabled)
			{
				return m_DefaultMixerSpeed;
			}
			return 0f;
		}
		set
		{
			m_DefaultMixerSpeed = value;
			if (!Disabled)
			{
				m_DefaultMixer.Mixer.SetSpeed(value);
			}
		}
	}

	public float LastDeltaTime { get; private set; }

	public bool Disabled
	{
		get
		{
			return m_Disabled;
		}
		set
		{
			m_Disabled.Value = value;
			m_DefaultMixer.Mixer.SetSpeed(DefaultMixerSpeed);
		}
	}

	public Animator Animator => m_Animator;

	public IReadOnlyList<AnimationActionHandle> ActiveActions => m_ActiveActions;

	public List<AnimationBase> ActiveAnimations => m_ActiveAnimations;

	public Queue<AnimationActionHandle> SequencedActions => m_SequencedActions;

	public AnimationActionHandle CurrentAction => m_CurrentAction;

	public PlayableGraph PlayableGraph => m_Graph;

	public IReadOnlyList<MixerInfo> Mixers => m_Mixers;

	public AnimationSet AnimationSet
	{
		get
		{
			if (m_AnimationSet == null)
			{
				return BlueprintRoot.Instance.HumanAnimationSet;
			}
			return m_AnimationSet;
		}
		set
		{
			m_AnimationSet = value;
			if (Application.isPlaying)
			{
				RuntimeInitializeAnimationSet();
				OnAnimationSetChanged();
			}
		}
	}

	public IEnumerable<PlayableInfo> Playables => m_DefaultMixer.Playables;

	protected AnimationPlayableOutput Output => m_Output;

	public bool FireEvents
	{
		get
		{
			return m_FireEvents;
		}
		set
		{
			m_FireEvents = value;
		}
	}

	private void Awake()
	{
		m_Name = ConstructName(base.gameObject);
	}

	private static string ConstructName(GameObject go)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (go != null)
		{
			stringBuilder.Insert(0, "/" + go.name);
			go = ObjectExtensions.Or(go.transform.parent, null)?.gameObject;
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return m_Name;
	}

	protected virtual void OnEnable()
	{
		Game.Instance.AnimationManagerController.Subscribe(this);
		Game.Instance.InterpolationController.Add(this);
		if (!m_IsInitialized)
		{
			m_Animator = GetComponent<Animator>();
			m_Graph = PlayableGraph.Create();
			m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
			PlayableGraph.Stop();
			AnimationLayerMixerPlayable animationLayerMixerPlayable = AnimationLayerMixerPlayable.Create(PlayableGraph, 10);
			m_DefaultMixer = new MixerInfo(animationLayerMixerPlayable, null, GetActiveTransformCount(null));
			m_Mixers.Add(m_DefaultMixer);
			m_Output = AnimationPlayableOutput.Create(PlayableGraph, "MainOutput", m_Animator);
			m_Output.SetSourcePlayable(animationLayerMixerPlayable, 0);
			PlayableGraph.Play();
			RuntimeInitializeAnimationSet();
			m_IsInitialized = true;
		}
	}

	protected virtual void OnDisable()
	{
		Game.Instance.AnimationManagerController.Unsubscribe(this);
		Game.Instance.InterpolationController.Remove(this);
	}

	protected virtual void OnDestroy()
	{
		StopEvents();
		if (m_IsInitialized)
		{
			m_Graph.Destroy();
		}
	}

	public void CustomUpdate(float dt)
	{
		using (Counters.AnimationManager?.Measure())
		{
			if (!Disabled)
			{
				LastDeltaTime = dt;
				using (ProfileScope.New("AnimationManager.UpdateAnimations"))
				{
					UpdateAnimations(dt);
				}
				using (ProfileScope.New("AnimationManager.UpdateActions"))
				{
					UpdateActions(dt);
				}
				using (ProfileScope.New("AnimationManager.UpdateAnimations(NewOnly)"))
				{
					UpdateAnimations(dt, newOnly: true);
				}
				InterpolateAnimations(0f, force: true);
				m_Animator.fireEvents = m_FireEvents && m_CurrentAction?.ActiveAnimation?.AnimatorController != null;
				m_LastUpdateTick = CurrentUpdateTick;
			}
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		if ((m_LastUpdateTick != CurrentUpdateTick && (m_LastUpdateTick + 1 != CurrentUpdateTick || !Mathf.Approximately(progress, 1f))) || Disabled)
		{
			return;
		}
		Game instance = Game.Instance;
		if (instance != null && instance.IsPaused)
		{
			return;
		}
		using (ProfileScope.New("AnimationManager.InterpolateAnimations"))
		{
			InterpolateAnimations(progress);
		}
	}

	protected virtual void UpdateAnimations(float dt, bool newOnly = false)
	{
		if (!newOnly && m_Graph.GetTimeUpdateMode() == DirectorUpdateMode.Manual)
		{
			m_Graph.Evaluate(dt);
		}
		AnimationBase animationBase = m_CurrentAction?.ActiveAnimation;
		if (animationBase != null && (!newOnly || !animationBase.UpdatedOnce))
		{
			float deltaTime = dt * animationBase.GetSpeed();
			animationBase.Update(deltaTime);
		}
		for (int i = 0; i < m_ActiveAnimations.Count; i++)
		{
			AnimationBase animationBase2 = m_ActiveAnimations[i];
			if (animationBase2 != animationBase && (!newOnly || !animationBase2.UpdatedOnce))
			{
				float deltaTime2;
				using (ProfileScope.New("GetSpeed"))
				{
					deltaTime2 = dt * animationBase2.GetSpeed();
				}
				animationBase2.Update(deltaTime2);
			}
			if (animationBase2 == null || animationBase2.State != AnimationState.Finished)
			{
				continue;
			}
			using (ProfileScope.New("Finishing"))
			{
				m_ActiveAnimations.RemoveAt(i);
				if (animationBase2.Handle.ActiveAnimation == animationBase2)
				{
					animationBase2.Handle.ActiveAnimation = null;
				}
				animationBase2.RemoveFromManager();
				i--;
			}
		}
	}

	private void InterpolateAnimations(float progress, bool force = false)
	{
		float num = 1f;
		AnimationBase animationBase = m_CurrentAction?.ActiveAnimation;
		if (animationBase != null)
		{
			animationBase.Interpolate(progress, 1f, force);
			if (animationBase.State != AnimationState.Finished && !animationBase.DoNotZeroOtherAnimations)
			{
				using (ProfileScope.New("GetWeight"))
				{
					num = 1f - m_CurrentAction.ActiveAnimation.GetWeight();
				}
			}
		}
		int num2 = 0;
		foreach (AnimationBase activeAnimation in m_ActiveAnimations)
		{
			if (activeAnimation.Handle.HasCrossfadePriority)
			{
				num2++;
			}
		}
		bool flag = num2 >= 2;
		foreach (AnimationBase activeAnimation2 in m_ActiveAnimations)
		{
			if (activeAnimation2 != animationBase)
			{
				float weightFromManager = ((activeAnimation2.Handle == m_CurrentAction || activeAnimation2.Handle.IsAdditive || (flag && activeAnimation2.Handle.HasCrossfadePriority)) ? 1f : ((flag && !activeAnimation2.Handle.HasCrossfadePriority) ? 0f : num));
				activeAnimation2.Interpolate(progress, weightFromManager, force);
			}
		}
		foreach (MixerInfo mixer in m_Mixers)
		{
			using (ProfileScope.New("Mixer.NormalizeWeights"))
			{
				mixer.NormalizeWeights();
			}
		}
	}

	protected virtual void OnAnimationSetChanged()
	{
	}

	private void UpdateActions(float dt)
	{
		for (int i = 0; i < m_ActiveActions.Count; i++)
		{
			AnimationActionHandle animationActionHandle = m_ActiveActions[i];
			if (animationActionHandle.IsStarted)
			{
				animationActionHandle.UpdateInternal(dt);
				if (animationActionHandle.IsReleased && animationActionHandle.ActiveAnimation == null)
				{
					animationActionHandle.FinishInternal();
				}
				if (animationActionHandle.IsFinished)
				{
					if (animationActionHandle == m_CurrentAction)
					{
						m_CurrentAction = null;
					}
					RemoveActionHandleAt(i);
					i--;
				}
			}
			else
			{
				animationActionHandle.StartInternal();
			}
		}
		if (m_CurrentAction == null)
		{
			double num = double.MinValue;
			for (int j = 0; j < m_ActiveActions.Count; j++)
			{
				AnimationActionHandle animationActionHandle2 = m_ActiveActions[j];
				if (animationActionHandle2.DontReleaseOnInterrupt && animationActionHandle2.ActiveAnimation != null && animationActionHandle2.ActiveAnimation.CreationTime.TotalSeconds > num)
				{
					num = animationActionHandle2.ActiveAnimation.CreationTime.TotalSeconds;
					m_CurrentAction = animationActionHandle2;
				}
			}
		}
		if ((m_CurrentAction == null || m_CurrentAction.IsReleased || m_CurrentAction.DontReleaseOnInterrupt) && m_SequencedActions.Count > 0)
		{
			m_CurrentAction = m_SequencedActions.Dequeue();
			AddActionHandle(m_CurrentAction);
			m_CurrentAction.StartInternal();
		}
	}

	protected void AddActionHandle(AnimationActionHandle handle)
	{
		m_ActiveActions.Add(handle);
		if (handle.PreventsRotation)
		{
			m_RotationForbidden.Retain();
		}
	}

	protected void RemoveActionHandle(AnimationActionHandle handle)
	{
		m_ActiveActions.Remove(handle);
		if (handle.PreventsRotation)
		{
			m_RotationForbidden.Release();
		}
	}

	protected void RemoveActionHandleAt(int index)
	{
		AnimationActionHandle animationActionHandle = m_ActiveActions[index];
		m_ActiveActions.RemoveAt(index);
		if (animationActionHandle.PreventsRotation)
		{
			m_RotationForbidden.Release();
		}
	}

	public void StartEvents(AnimatorControllerPlayable controller, IEnumerable<AnimationClipEvent> events, float time = 0f)
	{
		if (m_AnimatorControllerPlayableToPlayableInfo.TryGetValue(controller, out var value))
		{
			value.StartEvents(events, time);
		}
	}

	public void StartEvents()
	{
		float time = (float)m_DefaultMixer.Mixer.GetTime();
		foreach (PlayableInfo playable in m_DefaultMixer.Playables)
		{
			playable.StartEvents(time);
		}
	}

	public void UpdateEvents(AnimatorControllerPlayable controller, float time, float length)
	{
		if (m_AnimatorControllerPlayableToPlayableInfo.TryGetValue(controller, out var value))
		{
			value.UpdateEvents(time, length);
		}
	}

	public void SuspendEvents()
	{
		foreach (PlayableInfo playable in m_DefaultMixer.Playables)
		{
			playable.SuspendEvents();
		}
		foreach (KeyValuePair<AnimatorControllerPlayable, PlayableInfo> item in m_AnimatorControllerPlayableToPlayableInfo)
		{
			item.Value.SuspendEvents();
		}
	}

	public void ResumeEvents()
	{
		foreach (PlayableInfo playable in m_DefaultMixer.Playables)
		{
			playable.ResumeEvents();
		}
		foreach (KeyValuePair<AnimatorControllerPlayable, PlayableInfo> item in m_AnimatorControllerPlayableToPlayableInfo)
		{
			item.Value.ResumeEvents();
		}
	}

	public void StopEvents()
	{
		if (m_DefaultMixer == null || m_DefaultMixer.Playables == null)
		{
			return;
		}
		foreach (PlayableInfo playable in m_DefaultMixer.Playables)
		{
			playable.StopEvents();
		}
		foreach (KeyValuePair<AnimatorControllerPlayable, PlayableInfo> item in m_AnimatorControllerPlayableToPlayableInfo)
		{
			item.Value.StopEvents();
		}
	}

	public void StopEvents(AnimatorControllerPlayable controller)
	{
		if (m_AnimatorControllerPlayableToPlayableInfo.TryGetValue(controller, out var value))
		{
			value.StopEvents();
		}
	}

	private void RuntimeInitializeAnimationSet()
	{
		m_FromTransitions.Clear();
		m_ToTransitions.Clear();
		m_SequencedActions.Clear();
		foreach (AnimationActionHandle activeAction in m_ActiveActions)
		{
			activeAction.FinishInternal();
		}
		foreach (AnimationActionHandle activeAction2 in m_ActiveActions)
		{
			if (activeAction2.PreventsRotation)
			{
				m_RotationForbidden.Release();
			}
		}
		m_ActiveActions.Clear();
		if (m_AnimationSet == null)
		{
			return;
		}
		foreach (AnimationActionBase action in m_AnimationSet.Actions.Where((AnimationActionBase a) => a))
		{
			List<Transition> value = m_AnimationSet.Transitions.Where((Transition t) => t.FromAction == action).ToList();
			List<Transition> value2 = m_AnimationSet.Transitions.Where((Transition t) => t.ToAction == action).ToList();
			m_FromTransitions[action] = value;
			m_ToTransitions[action] = value2;
		}
		if (m_AnimationSet.StartupAction != null)
		{
			Execute(m_AnimationSet.StartupAction);
		}
	}

	public void PrepareForCombat()
	{
		foreach (AnimationActionHandle item in m_ActiveActions.Where((AnimationActionHandle h) => h.Action.ForceFinishOnJoinCombat))
		{
			item.FinishInternal();
		}
	}

	public virtual AnimationActionHandle CreateHandle([NotNull] AnimationActionBase animationAction)
	{
		return new AnimationActionHandle(animationAction, this);
	}

	public virtual void Execute(AnimationActionHandle handle)
	{
		if (handle == null)
		{
			PFLog.Animations.Error("AnimationActionHandle is null");
			return;
		}
		using (ProfileScope.New("Animator.Execute " + handle.Action.name))
		{
			if (handle.Manager != this)
			{
				PFLog.Animations.Error("Can't execute handle which created by another manager.");
			}
			else if (handle.IsStarted)
			{
				PFLog.Animations.Error("Started animation action handle can't be executed multiple times: " + handle.Action.NameSafe());
			}
			else if (m_ActiveActions.Contains(handle))
			{
				PFLog.Animations.Error("Action handle already added to manager: " + handle.Action.NameSafe());
			}
			else if (handle.IsAdditive)
			{
				if (!handle.Action.IsAdditiveToItself)
				{
					foreach (AnimationActionHandle activeAction in ActiveActions)
					{
						if (handle.Action.GetType() == activeAction.Action.GetType() && !activeAction.IsReleased)
						{
							return;
						}
					}
				}
				if (handle.Action.IsAdditiveInterruptsSameType)
				{
					if (m_CurrentAction != null && !m_CurrentAction.DontReleaseOnInterrupt && handle.Action.GetType() == m_CurrentAction.Action.GetType())
					{
						m_CurrentAction.Release();
					}
					foreach (AnimationActionHandle activeAction2 in m_ActiveActions)
					{
						if (!activeAction2.DontReleaseOnInterrupt && handle.Action.GetType() == activeAction2.Action.GetType())
						{
							activeAction2.Release();
						}
					}
				}
				AddActionHandle(handle);
				handle.StartInternal();
			}
			else if (handle.Action.ExecutionMode == ExecutionMode.Interrupted)
			{
				AddActionHandle(handle);
				if (m_CurrentAction != null && !m_CurrentAction.DontReleaseOnInterrupt && (!(m_CurrentAction.Action is WarhammerUnitAnimationActionHandAttack) || !(handle.Action is UnitAnimationActionCover)))
				{
					m_CurrentAction.MarkInterrupted();
					m_CurrentAction.Release();
				}
				m_CurrentAction = handle;
				m_CurrentAction.StartInternal();
				foreach (AnimationActionHandle sequencedAction in m_SequencedActions)
				{
					if (sequencedAction.Action != null)
					{
						PFLog.Animations.Log("Cleared sequenced action: {0}", sequencedAction.Action.NameSafe());
					}
					else
					{
						PFLog.Animations.Log("Cleared sequenced action: (destroyed)");
					}
					sequencedAction.MarkInterrupted();
				}
				m_SequencedActions.Clear();
			}
			else if (m_CurrentAction == null || m_CurrentAction.DontReleaseOnInterrupt)
			{
				AddActionHandle(handle);
				m_CurrentAction = handle;
				m_CurrentAction.StartInternal();
			}
			else if (m_SequencedActions.Count > 10)
			{
				PFLog.Animations.Warning($"Animation manager {this} has too many SequencedActions! This might be a leak.");
			}
			else
			{
				m_SequencedActions.Enqueue(handle);
			}
		}
	}

	public AnimationActionHandle Execute(AnimationActionBase action)
	{
		AnimationActionHandle animationActionHandle = CreateHandle(action);
		Execute(animationActionHandle);
		return animationActionHandle;
	}

	public void AddAnimationClip(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, bool isAdditive = false)
	{
		AddAnimationClip(handle, clipWrapper, null, useEmptyAvatarMask: true, isAdditive);
	}

	public void AddAnimationClip(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, ClipDurationType durType)
	{
		AddAnimationClip(handle, clipWrapper, null, useEmptyAvatarMask: true, isAdditive: false, durType);
	}

	public void AddAnimationClip(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, List<AvatarMask> avatarMasks, bool useEmptyAvatarMask = true, bool isAdditive = false, ClipDurationType durType = ClipDurationType.Default, IAddableAnimation animation = null)
	{
		bool isAdditive2 = handle.IsAdditive;
		if (handle != m_CurrentAction && !handle.DontReleaseOnInterrupt && !isAdditive2)
		{
			PFLog.Animations.Error("Can't start animation on interrupted action: " + handle.Action.NameSafe());
			return;
		}
		int num = avatarMasks?.Count((AvatarMask am) => am) ?? 0;
		num += (useEmptyAvatarMask ? 1 : 0);
		if (num <= 0)
		{
			return;
		}
		AnimationBase activeAnimation;
		if (num == 1 && animation == null)
		{
			AvatarMask avatarMask = ((!useEmptyAvatarMask) ? avatarMasks[0] : null);
			activeAnimation = AddAnimationClip(handle, clipWrapper, avatarMask, isAdditive);
		}
		else
		{
			IAddableAnimation addableAnimation = animation ?? new AnimationComposition(handle);
			if (useEmptyAvatarMask)
			{
				addableAnimation.AddPlayableInfo(AddAnimationClip(handle, clipWrapper, (AvatarMask)null, isAdditive));
			}
			for (int i = 0; i < (avatarMasks?.Count ?? 0); i++)
			{
				if (avatarMasks[i] != null)
				{
					addableAnimation.AddPlayableInfo(AddAnimationClip(handle, clipWrapper, avatarMasks[i], isAdditive));
				}
			}
			activeAnimation = (AnimationBase)addableAnimation;
		}
		handle.ActiveAnimation?.StartTransitionOut();
		handle.ActiveAnimation?.StopEvents(clipWrapper.Events);
		handle.ActiveAnimation = activeAnimation;
		handle.ActiveAnimation.TransitionIn = GetTransitionInDuration(handle.ActiveAnimation);
		handle.ActiveAnimation.TransitionOut = GetTransitionOutDuration(handle);
		float num2 = 0f;
		switch (durType)
		{
		case ClipDurationType.Endless:
			num2 = 0f;
			break;
		case ClipDurationType.Oneshot:
			num2 = clipWrapper.Length;
			break;
		case ClipDurationType.Default:
			num2 = handle.ActiveAnimation.GetDuration();
			break;
		}
		handle.ActiveAnimation.TransitionOutStartTime = ((num2 > 0f) ? Math.Max(num2 - handle.ActiveAnimation.TransitionOut, 0.01f) : 0f);
		m_ActiveAnimations.Add(handle.ActiveAnimation);
	}

	public void AddClipToComposition(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, AvatarMask avatarMask, bool isAdditive)
	{
		if (handle.ActiveAnimation is IAddableAnimation addableAnimation)
		{
			addableAnimation.AddPlayableInfo(AddAnimationClip(handle, clipWrapper, avatarMask, isAdditive));
		}
		else
		{
			PFLog.Animations.Error("Cannot add clip to handle: not a composition. Action: " + handle.Action.NameSafe());
		}
	}

	private PlayableInfo AddAnimationClip(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, AvatarMask avatarMask = null, bool isAdditive = false)
	{
		if (clipWrapper == null)
		{
			throw new Exception("Animation clip wrapper is null. Handle: " + handle.Action.NameSafe());
		}
		if (clipWrapper.AnimationClip == null)
		{
			throw new Exception("Animation clip wrapper has a null animation clip. Handle: " + handle.Action.NameSafe() + ", ClipWrapper: " + clipWrapper.NameSafe());
		}
		MixerInfo mixer = GetMixer(avatarMask, isAdditive);
		PlayableInfo playableInfo = mixer.AddPlayableFromCache(handle, (PlayableInfo p) => p.CanBeUsedFromCache(clipWrapper.AnimationClip, clipWrapper.EventsSorted));
		if (playableInfo == null)
		{
			AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(PlayableGraph, clipWrapper.AnimationClip);
			IEnumerable<AnimationClipEvent> eventsSorted = clipWrapper.EventsSorted;
			playableInfo = mixer.AddPlayable(handle, animationClipPlayable, eventsSorted);
		}
		playableInfo.SetSpeed(handle.SpeedScale);
		return playableInfo;
	}

	public void AddAnimatorController(AnimationActionHandle handle, RuntimeAnimatorController controller, bool isAdditive)
	{
		AddAnimatorController(handle, controller, null, useEmptyAvatarMask: true, isAdditive);
	}

	public void AddAnimatorController(AnimationActionHandle handle, RuntimeAnimatorController controller, List<AvatarMask> avatarMasks, bool useEmptyAvatarMask = true, bool isAdditive = false)
	{
		if (handle != m_CurrentAction && !handle.DontReleaseOnInterrupt)
		{
			PFLog.Animations.Error("Can't start animation on interrupted action: " + handle.Action.NameSafe());
			return;
		}
		int num = avatarMasks?.Count((AvatarMask am) => am) ?? 0;
		num += (useEmptyAvatarMask ? 1 : 0);
		if (num <= 0)
		{
			return;
		}
		AnimationBase activeAnimation;
		if (num == 1)
		{
			AvatarMask avatarMask = ((!useEmptyAvatarMask) ? avatarMasks[0] : null);
			activeAnimation = AddAnimatorController(handle, controller, avatarMask, isAdditive);
		}
		else
		{
			AnimationComposition animationComposition = new AnimationComposition(handle);
			if (useEmptyAvatarMask)
			{
				animationComposition.AddPlayableInfo(AddAnimatorController(handle, controller, (AvatarMask)null, isAdditive));
			}
			for (int i = 0; i < avatarMasks.Count; i++)
			{
				if (avatarMasks[i] != null)
				{
					animationComposition.AddPlayableInfo(AddAnimatorController(handle, controller, avatarMasks[i], isAdditive));
				}
			}
			activeAnimation = animationComposition;
		}
		handle.ActiveAnimation?.StartTransitionOut();
		handle.ActiveAnimation?.StopEvents();
		handle.ActiveAnimation = activeAnimation;
		handle.ActiveAnimation.TransitionIn = GetTransitionInDuration(handle.ActiveAnimation);
		handle.ActiveAnimation.TransitionOut = GetTransitionOutDuration(handle);
		float duration = handle.ActiveAnimation.GetDuration();
		if (duration > 0f)
		{
			handle.ActiveAnimation.TransitionOutStartTime = duration - handle.ActiveAnimation.TransitionOut;
		}
		m_ActiveAnimations.Add(handle.ActiveAnimation);
	}

	private PlayableInfo AddAnimatorController(AnimationActionHandle handle, RuntimeAnimatorController runtimeAnimatorController, AvatarMask avatarMask = null, bool isAdditive = false)
	{
		MixerInfo mixer = GetMixer(avatarMask, isAdditive);
		PlayableInfo playableInfo = mixer.AddPlayableFromCache(handle, (PlayableInfo p) => p.GetPlayableController() == runtimeAnimatorController);
		if (playableInfo == null)
		{
			AnimatorControllerPlayable animatorControllerPlayable = AnimatorControllerPlayable.Create(PlayableGraph, runtimeAnimatorController);
			playableInfo = mixer.AddPlayable(handle, animatorControllerPlayable);
			playableInfo.AnimatorController = runtimeAnimatorController;
			m_AnimatorControllerPlayableToPlayableInfo[animatorControllerPlayable] = playableInfo;
		}
		if (handle.Action is WarhammerUnitAnimationActionLocoMotion)
		{
			if (Animator.runtimeAnimatorController != null)
			{
				PFLog.Animations.Error("Animator.runtimeAnimatorController is already set for " + base.name + ".");
			}
			else
			{
				Animator.runtimeAnimatorController = runtimeAnimatorController;
			}
		}
		playableInfo.SetSpeed(handle.SpeedScale);
		return playableInfo;
	}

	private MixerInfo GetMixer(AvatarMask avatarMask, bool isAdditive)
	{
		int activeTransformCount = GetActiveTransformCount(avatarMask);
		if (m_Mixers.Count == 0)
		{
			AnimationLayerMixerPlayable animationLayerMixerPlayable = AnimationLayerMixerPlayable.Create(PlayableGraph, 10);
			m_Output.SetSourcePlayable(animationLayerMixerPlayable);
			MixerInfo mixerInfo = new MixerInfo(animationLayerMixerPlayable, avatarMask, activeTransformCount, isAdditive);
			m_Mixers.Add(mixerInfo);
			return mixerInfo;
		}
		Playable playable = Playable.Null;
		Playable current = m_Output.GetSourcePlayable();
		while (current.IsPlayableOfType<AnimationLayerMixerPlayable>())
		{
			MixerInfo mixerInfo2 = m_Mixers.FirstOrDefault((MixerInfo mi) => current.Equals(mi.Mixer));
			if (mixerInfo2 == null)
			{
				break;
			}
			if (mixerInfo2.ActiveTransformCount == activeTransformCount && mixerInfo2.AvatarMask == avatarMask && mixerInfo2.IsAdditive == isAdditive)
			{
				return mixerInfo2;
			}
			if ((isAdditive && !mixerInfo2.IsAdditive) || (avatarMask != mixerInfo2.AvatarMask && activeTransformCount < mixerInfo2.ActiveTransformCount && mixerInfo2.IsAdditive == isAdditive) || current.Equals(m_DefaultMixer.Mixer))
			{
				break;
			}
			playable = current;
			current = current.GetInput(0);
		}
		AnimationLayerMixerPlayable animationLayerMixerPlayable2 = AnimationLayerMixerPlayable.Create(PlayableGraph, 10);
		if (playable.Equals(Playable.Null))
		{
			m_Output.SetSourcePlayable(animationLayerMixerPlayable2);
		}
		else
		{
			PlayableGraph.Disconnect(playable, 0);
			PlayableGraph.Connect(animationLayerMixerPlayable2, 0, playable, 0);
		}
		PlayableGraph.Connect(current, 0, animationLayerMixerPlayable2, 0);
		MixerInfo mixerInfo3 = new MixerInfo(animationLayerMixerPlayable2, avatarMask, activeTransformCount, isAdditive);
		m_Mixers.Add(mixerInfo3);
		animationLayerMixerPlayable2.SetInputWeight(0, 1f);
		return mixerInfo3;
	}

	private int GetActiveTransformCount(AvatarMask avatarMask)
	{
		if (avatarMask == null)
		{
			return int.MaxValue;
		}
		int transformCount = avatarMask.transformCount;
		int num = 0;
		for (int i = 0; i < transformCount; i++)
		{
			if (avatarMask.GetTransformActive(i))
			{
				num++;
			}
		}
		return num;
	}

	internal Transition FindTransition(PlayableInfo fromPlayableInfo, PlayableInfo toPlayableInfo, List<Transition> transitions)
	{
		AnimationClip activeClip = fromPlayableInfo.GetActiveClip();
		AnimationClip activeClip2 = toPlayableInfo.GetActiveClip();
		Transition transition = null;
		Transition result = null;
		foreach (Transition transition2 in transitions)
		{
			if (transition2.FromAction == fromPlayableInfo.Handle.Action && transition2.ToAction == toPlayableInfo.Handle.Action && (transition2.FromClip == activeClip || transition2.FromClip == null) && (transition2.ToClip == activeClip2 || transition2.ToClip == null))
			{
				result = transition2;
				if (transition2.FromClip == activeClip && transition2.ToClip == activeClip2)
				{
					transition = transition2;
				}
			}
		}
		if (transition != null)
		{
			return transition;
		}
		return result;
	}

	internal float GetTransitionInDuration(AnimationBase animation)
	{
		float result = animation.Handle.Action.TransitionIn;
		if (m_CurrentAction?.ActiveAnimation != null && m_CurrentAction != animation.Handle && m_ToTransitions.TryGetValue(animation.Handle.Action, out var value))
		{
			AnimationClip activeClip = m_CurrentAction.ActiveAnimation.GetActiveClip();
			AnimationClip activeClip2 = animation.GetActiveClip();
			foreach (Transition item in value)
			{
				if (item.FromAction == m_CurrentAction.Action)
				{
					if (activeClip != null && item.FromClip == activeClip)
					{
						_ = item.ToClip == activeClip2;
						result = item.Duration;
					}
					else
					{
						result = item.Duration;
					}
					break;
				}
			}
		}
		return result;
	}

	internal float GetTransitionOutDuration(AnimationActionHandle actionHandle)
	{
		float num = actionHandle.Action.TransitionOut;
		if (m_FromTransitions.TryGetValue(actionHandle.Action, out var value))
		{
			foreach (Transition item in value)
			{
				if (item.Duration > num)
				{
					num = item.Duration;
				}
			}
		}
		return num;
	}
}
