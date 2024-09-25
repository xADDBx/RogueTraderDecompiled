using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;

public class CharGenAttributesPhaseDetailedView : CharGenPhaseDetailedView<CharGenAttributesPhaseVM>
{
	[Header("Selector")]
	[SerializeField]
	private TextMeshProUGUI m_AvailablePointsLabel;

	[SerializeField]
	protected CharGenAttributesPhaseSelectorView m_CharGenAttributesPhaseSelectorView;

	[Header("Skills")]
	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_CharInfoSkillsBlockView;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CharInfoSkillsBlockView.Bind(base.ViewModel.CharInfoSkillsBlock);
		m_CharGenAttributesPhaseSelectorView.Bind(base.ViewModel.SelectionGroup);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		AddDisposable(base.ViewModel.AvailablePointsLeft.Subscribe(delegate(int value)
		{
			m_AvailablePointsLabel.text = value.ToString();
		}));
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
	}
}
