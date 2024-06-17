using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.PC;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.Console;

public class ContextMenuConsoleView : ContextMenuView
{
	private IReadOnlyReactiveProperty<ContextMenuEntityConsoleView> m_CurrentEntity;

	public static readonly string InputLayerContextName = "ContextMenu";

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInputs();
	}

	private void CreateInputs()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(0, 1));
		m_CurrentEntity = gridConsoleNavigationBehaviour.DeepestFocusAsObservable.Select((IConsoleEntity e) => e as ContextMenuEntityConsoleView).ToReactiveProperty();
		List<ContextMenuEntityConsoleView> entitiesVertical = (from e in m_Entities
			select e as ContextMenuEntityConsoleView into e
			where e != null
			select e).ToList();
		gridConsoleNavigationBehaviour.SetEntitiesVertical(entitiesVertical);
		InputLayer inputLayer = gridConsoleNavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		inputLayer.AddButton(Close, 9);
		inputLayer.AddButton(ConfirmCurrentEntity, 8);
		AddDisposable(GamePad.Instance.PushLayer(inputLayer));
		gridConsoleNavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void ConfirmCurrentEntity(InputActionEventData data)
	{
		m_CurrentEntity.Value.Or(null)?.OnConfirm();
		Close(data);
	}

	private void Close(InputActionEventData data)
	{
		ContextMenuHelper.HideContextMenu();
	}
}
