using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickProtocolPetConsoleView : TooltipBrickProtocolPetView, IConsoleTooltipBrick, IConsoleInputHandler
{
	[SerializeField]
	private OwlcatMultiButton m_OwlcatMultiButton;

	private BoolReactiveProperty m_hintVisible = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_OwlcatMultiButton.OnFocus.AddListener(UpdateHintsHandler);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_OwlcatMultiButton.OnFocus.RemoveListener(UpdateHintsHandler);
	}

	private void UpdateHintsHandler(bool value)
	{
		m_hintVisible.Value = m_OwlcatMultiButton.IsFocus;
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_OwlcatMultiButton);
	}

	public void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			this.ShowTooltip(m_Tooltip);
		}, 8, m_hintVisible);
		AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Expand));
		AddDisposable(inputBindStruct);
	}

	public void UpdateTooltipBrick()
	{
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
