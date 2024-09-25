using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base.DlcList;
using Kingmaker.UI.MVVM.VM.NetLobby.DlcList;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console.DlcList;

public class NetLobbyDlcListConsoleView : NetLobbyDlcListBaseView
{
	[SerializeField]
	private NetLobbyDlcListDlcEntityConsoleView m_DlcEntityConsoleViewPrefab;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInput();
	}

	protected override void DrawDlcsImpl()
	{
		base.DrawDlcsImpl();
		NetLobbyDlcListDlcEntityVM[] array = base.ViewModel.Dlcs.ToArray();
		if (array.Any())
		{
			m_DlcsWidgetList.DrawEntries(array, m_DlcEntityConsoleViewPrefab);
		}
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "NetLobbyDlcListConsoleView"
		};
		CreateInputImpl(m_InputLayer, m_HintsWidget);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.CloseWindow();
		}, 9, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(inputLayer.AddAxis(Scroll, 3, repeat: true));
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
