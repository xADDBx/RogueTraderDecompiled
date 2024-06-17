using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;

public class NewSaveSlotConsoleView : SaveSlotConsoleView
{
	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(base.gameObject.SetActive));
		m_ConsoleInputField.Bind(base.ViewModel.SaveName.Value, base.ViewModel.TrySetSaveName);
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (!value)
		{
			m_ConsoleInputField.Abort();
		}
	}

	protected override void OnClick()
	{
		m_ConsoleInputField.Abort();
		base.OnClick();
	}

	public override bool IsValid()
	{
		return base.ViewModel.IsAvailable.Value;
	}

	protected override void HandleFunc01Click()
	{
		m_ConsoleInputField.Select();
	}
}
