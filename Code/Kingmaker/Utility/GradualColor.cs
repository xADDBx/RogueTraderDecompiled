using UnityEngine;

namespace Kingmaker.Utility;

public class GradualColor
{
	private GradualValue m_R = new GradualValue(0f);

	private GradualValue m_G = new GradualValue(0f);

	private GradualValue m_B = new GradualValue(0f);

	public float Speed
	{
		get
		{
			return m_R.Speed;
		}
		set
		{
			GradualValue r = m_R;
			GradualValue g = m_G;
			float num2 = (m_B.Speed = value);
			float speed = (g.Speed = num2);
			r.Speed = speed;
		}
	}

	public Color Target
	{
		get
		{
			return new Color(m_R.Target, m_G.Target, m_B.Target);
		}
		set
		{
			m_R.Target = value.r;
			m_G.Target = value.g;
			m_B.Target = value.b;
		}
	}

	public Color Current => new Color(m_R, m_G, m_B);

	public GradualColor(Color start)
	{
		m_R = new GradualValue(start.r);
		m_G = new GradualValue(start.g);
		m_B = new GradualValue(start.b);
		Speed = 1f;
	}

	public static implicit operator Color(GradualColor value)
	{
		return value.Current;
	}
}
