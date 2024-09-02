using Kingmaker.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.FX;

public class TextScrollSound : MonoBehaviour
{
	private Material m_Material;

	private float m_Step;

	private float m_StepScale;

	private float m_Time;

	private float m_ScrollX;

	private float m_PreviousOffset;

	[SerializeField]
	[AkEventReference]
	private string m_SoundEvent;

	public float OffsetError = 0.2f;

	private void OnEnable()
	{
		m_Material = base.gameObject.GetComponent<Renderer>().material;
		m_Step = m_Material.GetFloat("_Step");
		m_Time = m_Material.GetFloat("Time");
		m_StepScale = m_Material.GetFloat("_StepScale");
		m_ScrollX = m_Material.GetFloat("_ScrollX");
		float num = m_Time * m_ScrollX;
		float num2 = Mathf.Min(m_Step, num - Mathf.Floor(num));
		float num3 = Mathf.Floor(num) * m_Step;
		float previousOffset = (num2 + num3) * m_StepScale - Mathf.Floor((num2 + num3) * m_StepScale);
		m_PreviousOffset = previousOffset;
	}

	private void Update()
	{
		m_Time = Shader.GetGlobalVector("_Time").y;
		float num = m_Time * m_ScrollX;
		float num2 = Mathf.Min(m_Step, num - Mathf.Floor(num));
		float num3 = Mathf.Floor(num) * m_Step;
		float num4 = (num2 + num3) * m_StepScale - Mathf.Floor((num2 + num3) * m_StepScale);
		if (Mathf.Abs(m_PreviousOffset - num4) > OffsetError && !string.IsNullOrEmpty(m_SoundEvent))
		{
			SoundEventsManager.PostEvent(m_SoundEvent ?? "", base.gameObject);
			m_PreviousOffset = num4;
		}
	}
}
