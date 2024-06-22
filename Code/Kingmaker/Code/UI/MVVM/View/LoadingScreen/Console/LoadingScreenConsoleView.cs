using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.LoadingScreen.Console;

public class LoadingScreenConsoleView : LoadingScreenBaseView
{
	[SerializeField]
	private float m_DefaultConsoleFontTitleSize = 26f;

	[SerializeField]
	private float m_DefaultConsoleFontDescriptionSize = 23f;

	private InputLayer m_InputLayer;

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_BottomTitleText.fontSize = m_DefaultConsoleFontTitleSize * multiplier;
		m_BottomDescriptionText.fontSize = m_DefaultConsoleFontDescriptionSize * multiplier;
	}

	protected override void ShowUserInputWaiting(bool state)
	{
		if (state)
		{
			m_InputLayer = new InputLayer
			{
				ContextName = "LoadingScreen"
			};
			AddDisposable(m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 6));
			AddDisposable(m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 7));
			AddDisposable(m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 5));
			AddDisposable(m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 4));
			AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		}
		base.ShowUserInputWaiting(state);
	}

	protected override void DestroyViewImplementation()
	{
		m_InputLayer = null;
		base.DestroyViewImplementation();
	}
}
