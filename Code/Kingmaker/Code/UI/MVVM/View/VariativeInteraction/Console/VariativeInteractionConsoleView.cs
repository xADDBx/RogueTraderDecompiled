using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction.Console;

public class VariativeInteractionConsoleView : VariativeInteractionView<InteractionVariantConsoleView>
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInput();
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, Vector2Int.right);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "InteractionVariative"
		});
		m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9);
		CreateNavigation();
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void CreateNavigation()
	{
		List<InteractionVariantConsoleView> list = new List<InteractionVariantConsoleView>();
		for (int num = WidgetList.Entries.Count - 1; num >= 0; num--)
		{
			InteractionVariantConsoleView interactionVariantConsoleView = (InteractionVariantConsoleView)WidgetList.Entries[num];
			list.Add(interactionVariantConsoleView);
			interactionVariantConsoleView.SetInputLayer(m_InputLayer);
		}
		m_NavigationBehaviour.AddRow(list);
		m_NavigationBehaviour.SetCurrentEntity(list.LastOrDefault());
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		GamePad.Instance.PopLayer(m_InputLayer);
	}
}
