using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.LoadingScreen.PC;

public class LoadingScreenPCView : LoadingScreenBaseView
{
	[SerializeField]
	private float m_DefaultFontTitleSize = 26f;

	[SerializeField]
	private float m_DefaultFontDescriptionSize = 23f;

	private InputLayer m_InputLayer;

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_BottomTitleText.fontSize = m_DefaultFontTitleSize * multiplier;
		m_BottomDescriptionText.fontSize = m_DefaultFontDescriptionSize * multiplier;
	}

	protected override void DestroyViewImplementation()
	{
		m_InputLayer = null;
		base.DestroyViewImplementation();
	}
}
