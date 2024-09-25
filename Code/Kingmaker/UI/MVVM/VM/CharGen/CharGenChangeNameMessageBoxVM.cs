using System;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public class CharGenChangeNameMessageBoxVM : MessageBoxVM
{
	private readonly Func<string> m_GetRandomName;

	public CharGenChangeNameMessageBoxVM(string messageText, string yesLabel, Action<string> onTextClose, string inputText, Func<string> getRandomName, Action disposeAction)
		: base(messageText, DialogMessageBoxBase.BoxType.TextField, null, null, yesLabel, null, onTextClose, inputText, null, 0, disposeAction, null, null, null)
	{
		m_GetRandomName = getRandomName;
	}

	public void SetRandomName()
	{
		InputText.Value = m_GetRandomName();
	}
}
