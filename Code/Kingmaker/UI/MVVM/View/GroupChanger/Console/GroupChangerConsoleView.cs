using Kingmaker.Blueprints.Root.Strings;
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
		AddDisposable(m_NavigationBehaviour?.DeepestFocusAsObservable.Subscribe(OnFocus));
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
		int num = Mathf.CeilToInt((float)RemoteCharacterViews.Count / 6f);
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 6;
			int count = Mathf.Min(6, RemoteCharacterViews.Count - num2);
			m_NavigationBehaviour.AddRow(RemoteCharacterViews.GetRange(num2, count));
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private InputLayer GetInputLayer()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "GroupChanger"
		}, null, leftStick: true, rightStick: true);
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnCancel();
		}, 9, base.ViewModel.CloseActionsIsSame.Not().And(base.ViewModel.CloseEnabled).And(base.ViewModel.IsMainCharacter)
			.ToReactiveProperty()), UIStrings.Instance.CommonTexts.Cancel));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnAccept();
		}, 10, base.ViewModel.IsMainCharacter, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnSelectedClick();
		}, 8, base.ViewModel.IsMainCharacter), UIStrings.Instance.CommonTexts.Select));
		return m_InputLayer;
	}

	private void OnFocus(IConsoleEntity entity)
	{
		GroupChangerCharacterConsoleView groupChangerCharacterConsoleView = entity as GroupChangerCharacterConsoleView;
		if ((bool)groupChangerCharacterConsoleView)
		{
			RectTransform targetRect = groupChangerCharacterConsoleView.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	public void EntityFocused(IConsoleEntity entity)
	{
	}
}
