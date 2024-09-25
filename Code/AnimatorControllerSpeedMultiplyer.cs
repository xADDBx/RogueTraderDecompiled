using Kingmaker.Utility.Random;
using UnityEngine;

public class AnimatorControllerSpeedMultiplyer : MonoBehaviour
{
	public float AnimatorSpeedMultiplyer = 1f;

	public float AnimatorDeelay;

	public float RandomStart;

	private float m_CurrentTime;

	private Animator m_Animator;

	public bool _realtime;

	private void OnEnable()
	{
		if (!m_Animator)
		{
			m_Animator = GetComponent<Animator>();
		}
		if ((bool)m_Animator)
		{
			SetSpeedMultiplyer();
		}
	}

	private void Start()
	{
		_realtime = false;
		if (RandomStart != 0f)
		{
			AnimatorDeelay = PFStatefulRandom.Visuals.Animation4.Range(0f, RandomStart);
		}
		if (AnimatorDeelay > 0f)
		{
			m_Animator.enabled = false;
		}
	}

	private void Update()
	{
		if (m_CurrentTime >= AnimatorDeelay)
		{
			m_Animator.enabled = true;
		}
		m_CurrentTime = Time.time;
		if (_realtime)
		{
			m_Animator = GetComponent<Animator>();
			if (AnimatorDeelay > 0f)
			{
				m_Animator.enabled = false;
			}
			if (!m_Animator)
			{
				SetSpeedMultiplyer();
			}
		}
	}

	private void SetSpeedMultiplyer()
	{
		m_Animator.SetFloat("SpeedMultiplyer", AnimatorSpeedMultiplyer);
	}
}
