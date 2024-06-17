using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

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
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
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
		foreach (InteractionVariantConsoleView entry in WidgetList.Entries)
		{
			list.Add(entry);
			entry.SetInputLayer(m_InputLayer);
		}
		if (list.Count > 1)
		{
			int num = ((list.Count % 2 == 0) ? 1 : 2);
			for (int i = 0; i < list.Count / 2 + ((list.Count % 2 != 0) ? 1 : 0); i++)
			{
				if (i % 2 == 0)
				{
					InteractionVariantConsoleView value = list[i];
					int index = i;
					int num2 = num;
					list[index] = list[list.Count - num2];
					num2 = num;
					list[list.Count - num2] = value;
				}
			}
		}
		m_NavigationBehaviour.AddRow(list);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		GamePad.Instance.PopLayer(m_InputLayer);
	}
}
