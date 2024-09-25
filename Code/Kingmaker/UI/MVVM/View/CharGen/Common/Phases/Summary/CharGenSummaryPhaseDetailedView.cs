using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Summary;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Summary;

public class CharGenSummaryPhaseDetailedView : CharGenPhaseDetailedView<CharGenSummaryPhaseVM>
{
	[Header("Character Info")]
	[SerializeField]
	private CharGenNameBaseView m_CharGenNameView;

	[SerializeField]
	private CharInfoLevelClassScoresPCView m_LevelClassScoresView;

	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_SkillsBlockView;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	protected override bool HasYScrollBindInternal => false;

	public override void Initialize()
	{
		base.Initialize();
		m_LevelClassScoresView.Initialize();
		m_SkillsBlockView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CharGenNameView.Bind(base.ViewModel.CharGenNameVM);
		m_LevelClassScoresView.Bind(base.ViewModel.LevelClassScoresVM);
		m_SkillsBlockView.Bind(base.ViewModel.CharInfoSkillsBlockVM);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		AddDisposable(base.ViewModel.InterruptHandler.Subscribe(base.ViewModel.CharGenNameVM.ShowChangeNameMessageBox));
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
	}

	public InputLayer GetInputLayer(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		return inputLayer;
	}
}
