using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;

[Serializable]
public class OvertipSystemObjectColorConfig
{
	[SerializeField]
	private OvertipSystemObjectStateColor[] m_NameStateColors;

	public Color GetColorByState(OvertipSystemObjectState state)
	{
		for (int i = 0; i < m_NameStateColors.Length; i++)
		{
			if (m_NameStateColors[i].State == state)
			{
				return m_NameStateColors[i].Color;
			}
		}
		PFLog.UI.Log("OvertipSystemObjectColorConfig.GetColorByState - can't find color for state " + state);
		return Color.white;
	}
}
