using UnityEngine;

namespace Kingmaker.UI.Common.Animations;

[RequireComponent(typeof(Animation))]
public class AnimationWorkaround : MonoBehaviour
{
	private Animation m_Animation;

	private float m_AnimationTime;

	private float m_CurrentTime;

	private float m_PreviousTime;

	private bool m_IsPlaying;

	public void Awake()
	{
		if ((object)m_Animation == null)
		{
			m_Animation = GetComponent<Animation>();
		}
		m_PreviousTime = (m_CurrentTime = Time.realtimeSinceStartup);
	}

	private void OnEnable()
	{
		if (m_Animation.playAutomatically)
		{
			PlayAnimation();
		}
	}

	private void Update()
	{
		if ((bool)m_Animation.clip && m_IsPlaying)
		{
			m_CurrentTime = Time.realtimeSinceStartup;
			float num = m_CurrentTime - m_PreviousTime;
			m_PreviousTime = m_CurrentTime;
			m_Animation[m_Animation.clip.name].time = m_AnimationTime;
			m_Animation.Sample();
			m_AnimationTime += num;
			if (m_AnimationTime >= m_Animation.clip.length)
			{
				m_Animation[m_Animation.clip.name].time = m_Animation.clip.length;
				m_Animation.Sample();
				m_IsPlaying = false;
			}
		}
	}

	public void PlayAnimation()
	{
		if (m_Animation.clip != null)
		{
			m_AnimationTime = 0f;
			m_PreviousTime = (m_CurrentTime = Time.realtimeSinceStartup);
			m_IsPlaying = true;
			m_Animation.Play();
		}
	}
}
