using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;

namespace Kingmaker.Visual;

public class UnitMultiHighlight : MultiHighlighter
{
	private readonly HighlightSource m_BaseAnim = new HighlightSource();

	public Color BaseColor
	{
		get
		{
			return m_BaseAnim.Color;
		}
		set
		{
			if (value.a > 0f)
			{
				m_BaseAnim.Color = value;
				Play(m_BaseAnim);
				ReapplyColorInCurrentHighlight(m_BaseAnim);
			}
			else
			{
				Stop(m_BaseAnim);
			}
		}
	}
}
