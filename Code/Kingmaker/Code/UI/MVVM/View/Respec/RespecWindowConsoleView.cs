using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Respec;

public class RespecWindowConsoleView : RespecWindowCommonView
{
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private GridConsoleNavigationBehaviour m_Behaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Behaviour = new GridConsoleNavigationBehaviour());
		CreateInput();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ConsoleHintsWidget.Dispose();
		GamePad.Instance.PopLayer(m_InputLayer);
	}

	private void CreateInput()
	{
		m_Behaviour.SetEntitiesGrid(m_RespecCharactersSelectorView.GetNavigationEntities(), 4);
		m_Behaviour.FocusOnFirstValidEntity();
		m_InputLayer = m_Behaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Respec"
		});
		AddDisposable(m_Behaviour.Focus.Subscribe(Scroll));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			CloseWindow();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8), UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	public void Scroll(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
