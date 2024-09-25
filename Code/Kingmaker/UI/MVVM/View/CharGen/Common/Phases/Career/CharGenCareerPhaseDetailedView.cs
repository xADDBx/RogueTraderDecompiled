using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Career;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Career;

public class CharGenCareerPhaseDetailedView : CharGenPhaseDetailedView<CharGenCareerPhaseVM>
{
	[SerializeField]
	protected UnitProgressionCommonView m_UnitProgressionView;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
		m_UnitProgressionView.Initialize(delegate
		{
		});
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_UnitProgressionView.Bind(base.ViewModel.UnitProgressionVM);
		m_InfoView.Bind(base.ViewModel.InfoVM);
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
	}
}
