using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.TermOfUse.Base;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.TermOfUse.Console;

public class TermsOfUseConsoleView : TermsOfUseBaseView
{
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private ConsoleHint m_AcceptHint;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		BuildNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void BuildNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		BuildNavigationImpl(m_NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "TermsOfUseLayer"
		});
		AddDisposable(m_InputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(m_AcceptHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.TermsOfUseAccept();
		}, 8)));
		m_AcceptHint.SetLabel(UIStrings.Instance.TermsOfUseTexts.AcceptBtn);
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
