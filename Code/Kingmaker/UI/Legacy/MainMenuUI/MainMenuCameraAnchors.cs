using System;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class MainMenuCameraAnchors : MonoBehaviour
{
	[Serializable]
	public class AspectRatioPreset
	{
		public int WidthProportion;

		public int HeightProportion;

		public Transform Anchor;

		public float AspectRatio => (float)WidthProportion / (float)HeightProportion;
	}

	private static MainMenuCameraAnchors s_Anchors;

	public Transform MainPosition;

	public AspectRatioPreset[] AspectRatioPresets;

	private AspectRatioPreset m_CurrentPreset;

	public static MainMenuCameraAnchors Instance => s_Anchors = (s_Anchors ? s_Anchors : UnityEngine.Object.FindObjectOfType<MainMenuCameraAnchors>());

	public void CalculateMainPosition(Camera camera)
	{
		m_CurrentPreset = AspectRatioPresets[0];
		float num = 10000f;
		AspectRatioPreset[] aspectRatioPresets = AspectRatioPresets;
		foreach (AspectRatioPreset aspectRatioPreset in aspectRatioPresets)
		{
			float num2 = Mathf.Abs(camera.aspect - aspectRatioPreset.AspectRatio);
			if (num2 < num)
			{
				num = num2;
				m_CurrentPreset = aspectRatioPreset;
			}
		}
		MainPosition.SetPositionAndRotation(m_CurrentPreset.Anchor.position, m_CurrentPreset.Anchor.rotation);
	}
}
