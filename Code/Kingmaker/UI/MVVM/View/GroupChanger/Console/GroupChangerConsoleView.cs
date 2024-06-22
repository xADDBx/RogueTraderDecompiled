using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.GroupChanger.Console;

public class GroupChangerConsoleView : GroupChangerBaseView, IConsoleNavigationOwner, IConsoleEntity
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private const int ColumnsCount = 6;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour(this, null, Vector2Int.one));
		AddDisposable(GamePad.Instance.PushLayer(GetInputLayer()));
		CreateNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void CreateNavigation()
	{
		if (RemoteCharacterViews.Count <= 6)
		{
			m_NavigationBehaviour.AddRow(RemoteCharacterViews);
		}
		else
		{
			m_NavigationBehaviour.AddRow(RemoteCharacterViews.GetRange(0, 6));
			m_NavigationBehaviour.AddRow(RemoteCharacterViews.GetRange(6, RemoteCharacterViews.Count - 6));
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private InputLayer GetInputLayer()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "GroupChanger"
		}, null, leftStick: true, rightStick: true);
		if (UINetUtility.IsControlMainCharacter())
		{
			AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				OnCancel();
			}, 9, base.ViewModel.CloseActionsIsSame.Not().And(base.ViewModel.CloseEnabled).ToReactiveProperty()), UIStrings.Instance.CommonTexts.Cancel));
			AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				OnAccept();
			}, 10, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.CommonTexts.Accept));
			AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				OnSelectedClick();
			}, 8), UIStrings.Instance.CommonTexts.Select));
		}
		return m_InputLayer;
	}

	public void EntityFocused(IConsoleEntity entity)
	{
	}
}
