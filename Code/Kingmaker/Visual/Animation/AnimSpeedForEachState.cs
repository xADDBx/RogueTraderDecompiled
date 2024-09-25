using System.Linq;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimSpeedForEachState : MonoBehaviour
{
	public Animator AnimatorForChangeSpeed;

	public float SpeedForNotFoundedClips = 1f;

	[HideInInspector]
	public AnimationManager AnimManager;

	public ClipOptions[] Clips;

	private int m_ClipCount;

	private AnimationBase m_ClipLoco;

	private AnimationBase m_ClipOther;

	private AnimationClip m_ClipLocoState;

	private AnimationClip m_ClipOtherState;

	private float m_TimeLoop;

	private float m_NextLoopTime;

	private ClipOptions FindClip(AnimationBase clip)
	{
		return Clips.FirstOrDefault((ClipOptions cl) => cl.Clip == clip.GetActiveClip());
	}

	private float CalculateSpeed(ClipOptions clip)
	{
		return clip?.Speed ?? SpeedForNotFoundedClips;
	}

	private float FindSpeedFromTwoClips(AnimationBase clipLoco, AnimationBase clipOther)
	{
		float num = CalculateSpeed(FindClip(clipLoco));
		float num2 = CalculateSpeed(FindClip(clipOther));
		return (num + num2) / 2f;
	}

	private float FindSpeedFromCurClips(AnimationBase clipLoco, AnimationBase clipOther)
	{
		if (clipOther != null)
		{
			return FindSpeedFromTwoClips(clipLoco, clipOther);
		}
		return CalculateSpeed(FindClip(clipLoco));
	}

	private void Start()
	{
		if (AnimManager == null && GetComponent<AnimationManager>() != null)
		{
			AnimManager = GetComponent<AnimationManager>();
		}
		if (AnimManager == null)
		{
			PFLog.Default.Error("No AnimationManagerForSearchStateIn in " + base.gameObject.name);
		}
	}

	private void Update()
	{
		if (!(AnimManager == null) && AnimManager.ActiveActions.Count != 0 && (m_ClipCount != AnimManager.ActiveActions.Count || m_ClipLoco != AnimManager.ActiveActions[0].ActiveAnimation || !object.Equals(m_ClipLocoState, AnimManager.ActiveActions[0].ActiveAnimation.GetActiveClip()) || (m_ClipCount != 1 && (m_ClipOther != AnimManager.ActiveActions[1].ActiveAnimation || AnimManager.ActiveActions[1].ActiveAnimation == null || !object.Equals(m_ClipOtherState, AnimManager.ActiveActions[1].ActiveAnimation.GetActiveClip())))))
		{
			m_ClipCount = AnimManager.ActiveActions.Count;
			m_ClipLoco = AnimManager.ActiveActions[0].ActiveAnimation;
			m_ClipOther = ((m_ClipCount == 1) ? null : AnimManager.ActiveActions[1].ActiveAnimation);
			m_ClipLocoState = AnimManager.ActiveActions[0].ActiveAnimation.GetActiveClip();
			m_ClipOtherState = ((m_ClipOther == null) ? null : AnimManager.ActiveActions[1].ActiveAnimation.GetActiveClip());
			AnimatorForChangeSpeed.speed = FindSpeedFromCurClips(m_ClipLoco, (m_ClipCount == 1) ? null : m_ClipOther);
		}
	}
}
