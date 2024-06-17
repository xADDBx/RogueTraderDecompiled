using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NetRoles.Console;

public class NetRolesPlayerCharacterConsoleView : NetRolesPlayerCharacterBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private RectTransform m_FocusButton;

	[SerializeField]
	private ConsoleHint m_HintUp;

	[SerializeField]
	private ConsoleHint m_HintDown;

	private readonly BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.PlayerRoleMe.Skip(1).Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(INetRolesConsoleHandler h)
			{
				h.HandleUpdateCharactersNavigation(base.Character);
			});
		}));
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
		m_IsFocused.Value = value;
		m_FocusButton.gameObject.SetActive(value);
	}

	public bool IsValid()
	{
		return m_Portrait.gameObject.activeInHierarchy;
	}

	public void AddPlayerInput(InputLayer inputLayer)
	{
		if (base.ViewModel != null)
		{
			AddDisposable(m_HintUp.BindCustomAction(6, inputLayer, base.ViewModel.CanUp.And(m_IsFocused).ToReactiveProperty()));
			AddDisposable(inputLayer.AddButton(delegate
			{
				base.ViewModel.MoveRoleCharacterUp();
			}, 6, base.ViewModel.CanUp.And(m_IsFocused).ToReactiveProperty()));
			AddDisposable(m_HintDown.BindCustomAction(7, inputLayer, base.ViewModel.CanDown.And(m_IsFocused).ToReactiveProperty()));
			AddDisposable(inputLayer.AddButton(delegate
			{
				base.ViewModel.MoveRoleCharacterDown();
			}, 7, base.ViewModel.CanDown.And(m_IsFocused).ToReactiveProperty()));
		}
	}
}
