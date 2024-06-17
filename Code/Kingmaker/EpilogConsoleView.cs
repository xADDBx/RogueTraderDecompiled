using Kingmaker.Code.UI.MVVM.View.Dialog.Epilog;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UnityEngine;

namespace Kingmaker;

public class EpilogConsoleView : EpilogBaseView
{
	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_Hint;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_InputLayer = InputLayer.FromView(this);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		AddDisposable(m_Hint.Bind(m_InputLayer.AddButton(delegate
		{
			Confirm();
		}, 8)));
		AddDisposable(m_ScrollHint.Bind(m_InputLayer.AddAxis(Scroll, 3)));
	}

	protected override void OnAnswersChanged()
	{
		LocalizedString localizedString = base.ViewModel.Answers.Value?.FirstOrDefault()?.Answer.Value?.Text;
		string label = ((localizedString != null) ? ((string)localizedString) : string.Empty).Replace(".", string.Empty);
		m_Hint.SetLabel(label);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_CueScrollRect.Scroll(value * 0.2f, smooth: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InputLayer = null;
	}
}
