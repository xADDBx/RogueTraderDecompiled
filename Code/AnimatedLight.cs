using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Particles;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Light))]
[DisallowMultipleComponent]
[DefaultExecutionOrder(1)]
public class AnimatedLight : MonoBehaviour, IDeactivatableComponent, ITimeOfDayChangedHandler, ISubscriber
{
	[Header("Timing")]
	[SerializeField]
	private float m_Delay;

	[SerializeField]
	private float m_Lifetime = 1f;

	[SerializeField]
	private float m_Frequency;

	[Header("Animations")]
	[SerializeField]
	private Gradient m_ColorOverLifetime;

	[SerializeField]
	private float m_ColorMin = 1f;

	[SerializeField]
	private float m_ColorMax = 1f;

	[SerializeField]
	[FormerlySerializedAs("m_SizeOverLifetime")]
	private AnimationCurve m_RangeOverLifetime = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	[SerializeField]
	private float m_RandomRangeMin;

	[SerializeField]
	private float m_RandomRangeMax;

	[SerializeField]
	private AnimationCurve m_MoveXOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	[SerializeField]
	private AnimationCurve m_MoveYOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	[SerializeField]
	private AnimationCurve m_MoveZOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	[SerializeField]
	private float m_MoveMultiplier = 1f;

	[SerializeField]
	private bool m_LoopAnimation;

	[SerializeField]
	private bool m_MoveInWorldspace;

	[SerializeField]
	private bool m_DestroyOnEnd;

	private Vector3 m_StartPos;

	[Header("Base values")]
	[SerializeField]
	private Color m_Color = Color.white;

	[SerializeField]
	private float m_Intensity = 1f;

	[SerializeField]
	[FormerlySerializedAs("m_Size")]
	private float m_Range = 10f;

	private float m_StartTime;

	private Light m_Light;

	private float m_TimeOfDayIntensityMultiplier = 1f;

	private float m_TimeOfDayRangeMultiplier = 1f;

	private float m_IntensityMultiplier = 1f;

	private float m_LastOperation;

	public float IntensityMultiplier
	{
		get
		{
			return m_IntensityMultiplier;
		}
		set
		{
			m_IntensityMultiplier = value;
			if (m_StartTime > Time.time)
			{
				ApplyValues((!(m_StartTime < float.MaxValue)) ? 1 : 0);
			}
		}
	}

	public float FadeOut { get; set; } = 1f;


	private void OnEnable()
	{
		m_Light = GetComponent<Light>();
		if (m_MoveInWorldspace)
		{
			m_StartPos = base.transform.position;
		}
		FadeOut = 1f;
		OnTimeOfDayChanged();
		m_StartTime = Time.time + m_Delay;
		Update();
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	private void Update()
	{
		if (m_Lifetime <= 0f)
		{
			base.enabled = false;
			return;
		}
		float num = (Time.time - m_StartTime) / m_Lifetime;
		if (num < 0f)
		{
			return;
		}
		if (m_LoopAnimation)
		{
			num -= Mathf.Floor(num);
		}
		if (m_DestroyOnEnd && num >= 1f)
		{
			Object.Destroy(base.gameObject);
		}
		if (!(num < 0f))
		{
			if (num > 1f)
			{
				num = 1f;
			}
			m_Light.enabled = true;
			if (m_Frequency == 0f)
			{
				ApplyValues(num);
			}
			else if (Time.time >= m_LastOperation + m_Frequency)
			{
				m_LastOperation = Time.time;
				ApplyValues(num);
			}
		}
	}

	private void ApplyValues(float normalizedTime)
	{
		Color color = ((m_ColorOverLifetime == null) ? Color.white : (m_ColorOverLifetime.Evaluate(normalizedTime) * Random.Range(m_ColorMin, m_ColorMax)));
		float num = m_RangeOverLifetime.Evaluate(normalizedTime) + Random.Range(m_RandomRangeMin, m_RandomRangeMax);
		Vector3 vector = (Vector3.right * m_MoveXOverLifetime.Evaluate(normalizedTime) + Vector3.up * m_MoveYOverLifetime.Evaluate(normalizedTime) + Vector3.forward * m_MoveZOverLifetime.Evaluate(normalizedTime)) * m_MoveMultiplier;
		m_Light.color = m_Color * color;
		m_Light.intensity = m_Intensity * color.a * IntensityMultiplier * m_TimeOfDayIntensityMultiplier * FadeOut;
		Vector3 lossyScale = base.transform.lossyScale;
		m_Light.range = m_Range * num * Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z)) * m_TimeOfDayRangeMultiplier;
		if (m_MoveInWorldspace)
		{
			base.transform.position = m_StartPos + vector;
		}
		else
		{
			base.transform.localPosition = m_StartPos + vector;
		}
	}

	public void OnTimeOfDayChanged()
	{
		FxRoot fxRoot = BlueprintRoot.Instance.FxRoot;
		Vector3 position = base.transform.position;
		m_TimeOfDayIntensityMultiplier = fxRoot.GetLightIntensityMultiplier(position);
		m_TimeOfDayRangeMultiplier = fxRoot.GetLightRangeMultiplier(position);
	}
}
