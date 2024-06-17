using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class CharInfoNameAndPortraitConsoleView : CharInfoNameAndPortraitPCView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectPrevCharacter();
		}, 14, enabledHints);
		AddDisposable(m_PreviousFilterHint.Bind(inputBindStruct));
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectNextCharacter();
		}, 15, enabledHints);
		AddDisposable(m_NextFilterHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		AddDisposable(enabledHints.Subscribe(delegate(bool value)
		{
			m_NextButton.gameObject.SetActive(!value);
			m_PrevButton.gameObject.SetActive(!value);
		}));
	}
}
