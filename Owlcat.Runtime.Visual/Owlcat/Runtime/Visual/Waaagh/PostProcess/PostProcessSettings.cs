using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

[Serializable]
public class PostProcessSettings
{
	public const int kMinLutSize = 32;

	public const int kMaxLutSize = 65;

	[SerializeField]
	private int m_ColorGradingLutSize = 32;

	[SerializeField]
	private ColorGradingMode m_ColorGradingMode;

	[SerializeField]
	private TemporalAntialiasingSettings m_TemporalAntialiasingSettings;

	public int ColorGradingLutSize
	{
		get
		{
			return m_ColorGradingLutSize;
		}
		set
		{
			m_ColorGradingLutSize = Mathf.Clamp(value, 32, 65);
		}
	}

	public ColorGradingMode ColorGradingMode
	{
		get
		{
			return m_ColorGradingMode;
		}
		set
		{
			m_ColorGradingMode = value;
		}
	}

	public TemporalAntialiasingSettings TemporalAntialiasingSettings => m_TemporalAntialiasingSettings;
}
