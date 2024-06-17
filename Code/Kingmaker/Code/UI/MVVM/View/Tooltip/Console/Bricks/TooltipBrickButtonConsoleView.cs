using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickButtonConsoleView : TooltipBrickButtonView, IConsoleTooltipBrick, IConsoleInputHandler, IMonoBehaviour
{
	[SerializeField]
	private OwlcatMultiButton m_ConsoleButton;

	private SimpleConsoleNavigationEntity m_ButtonEntity;

	private readonly BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_IsFocused.Value = false;
		AddDisposable(m_ConsoleButton.OnFocusAsObservable().Subscribe(delegate(bool value)
		{
			m_IsFocused.Value = value;
		}));
		m_ButtonEntity = new SimpleConsoleNavigationEntity(m_ConsoleButton);
		m_Text.text = base.ViewModel.Text;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ButtonEntity.SetFocus(value: false);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_ButtonEntity;
	}

	public void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClick();
		}, 8, m_IsFocused), UIStrings.Instance.CommonTexts.Accept));
	}

	public void UpdateTooltipBrick()
	{
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
