using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class CharacterVisualSettingsConsoleView : CharacterVisualSettingsView<CharacterVisualSettingsEntityConsoleView>
{
	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	protected ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private DollRoomTargetController m_RoomTargetController;

	private float m_RotateFactor = 1f;

	private float m_ZoomFactor = 1f;

	private float m_ZoomThresholdValue = 0.01f;

	public void SetDollRoomController(DollRoomTargetController roomTargetController, float rotateFactor, float zoomFactor, float zoomThresholdValue)
	{
		m_RoomTargetController = roomTargetController;
		m_RotateFactor = rotateFactor;
		m_ZoomFactor = zoomFactor;
		m_ZoomThresholdValue = zoomThresholdValue;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		List<IConsoleEntity> entities = new List<IConsoleEntity> { m_OutfitMainColorSelectorView, m_ClothEntityView, m_HelmetEntityView, m_BackpackEntityView };
		m_NavigationBehaviour.AddColumn(entities);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharacterVisualSettings"
		});
		m_ClothEntityView.Or(null)?.AddInput(m_InputLayer);
		m_HelmetEntityView.AddInput(m_InputLayer);
		m_BackpackEntityView.AddInput(m_InputLayer);
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_InputLayer.AddAxis(RotateDoll, 2));
		AddDisposable(m_InputLayer.AddAxis(ZoomDoll, 3));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 16);
		if (m_CloseHint != null)
		{
			AddDisposable(m_CloseHint.Bind(inputBindStruct));
			m_CloseHint.SetLabel(UIStrings.Instance.CharGen.HideVisualSettings);
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_RoomTargetController.Or(null)?.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_RoomTargetController.Or(null)?.Zoom(x * m_ZoomFactor);
		}
	}
}
