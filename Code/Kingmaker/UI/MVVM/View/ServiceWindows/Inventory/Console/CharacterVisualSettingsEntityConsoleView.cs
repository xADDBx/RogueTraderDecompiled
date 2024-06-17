using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class CharacterVisualSettingsEntityConsoleView : CharacterVisualSettingsEntityView, IConsoleNavigationEntity, IConsoleEntity
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_FocusButton;

	[SerializeField]
	private ConsoleHint m_LeftHint;

	[SerializeField]
	private ConsoleHint m_RightHint;

	private readonly BoolReactiveProperty m_Focused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Focused.Value = false;
	}

	public void AddInput(InputLayer inputLayer)
	{
		if (base.ViewModel != null)
		{
			IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = m_Focused.And(base.ViewModel.Locked.Not()).ToReactiveProperty();
			AddDisposable(m_LeftHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 4, readOnlyReactiveProperty)));
			AddDisposable(m_RightHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 5, readOnlyReactiveProperty)));
			AddDisposable(inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 0, readOnlyReactiveProperty));
			AddDisposable(inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 0, readOnlyReactiveProperty, InputActionEventType.NegativeButtonJustPressed));
		}
	}

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
		m_Focused.Value = value;
	}

	public bool IsValid()
	{
		return m_FocusButton.IsValid();
	}
}
