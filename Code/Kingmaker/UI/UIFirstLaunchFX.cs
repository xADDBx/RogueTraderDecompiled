using Kingmaker.UI.Common.Animations;
using UnityEngine;

namespace Kingmaker.UI;

public class UIFirstLaunchFX : MonoBehaviour
{
	[SerializeField]
	private MainMenuButtonFx[] m_FXObjects;

	[SerializeField]
	private MainMenuButtonFx.EffectSettings m_EffectSettings = new MainMenuButtonFx.EffectSettings
	{
		FirstDelay = 2f,
		FirstStay = 0.2f,
		SecondDelay = 0.5f,
		FadeInTime = 0.2f,
		FadeOutTime = 0.2f
	};

	public void PlayEffect()
	{
		Debug.Log("Play FX");
		MainMenuButtonFx[] fXObjects = m_FXObjects;
		for (int i = 0; i < fXObjects.Length; i++)
		{
			fXObjects[i].PlayFXSequence(m_EffectSettings);
		}
	}
}
