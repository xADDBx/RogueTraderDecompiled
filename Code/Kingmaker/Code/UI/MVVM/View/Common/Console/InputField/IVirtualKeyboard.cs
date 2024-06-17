using System;
using Kingmaker.Localization.Enums;

namespace Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;

public interface IVirtualKeyboard
{
	void OpenKeyboard(Action<string> successCallback, Action cancelCallback, string titleText, string inputText, string placeholderText, Locale language, uint maxTextLength);

	void Abort();
}
