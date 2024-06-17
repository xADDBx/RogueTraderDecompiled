using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.NewGame;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Common;

public abstract class TempNewGameCommonView : ViewBase<TempNewGameVM>
{
	[Header("Common")]
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	protected TempPregenSelectorCommonView m_PregenSelectorView;

	[SerializeField]
	protected SettingsEntityDropdownGameDifficultyPCView m_SettingsEntityDropdownGameDifficultyViewPrefab;

	protected InputLayer InputLayer;

	protected GridConsoleNavigationBehaviour NavigationBehaviour;

	public void Initialize()
	{
		m_Animator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_PregenSelectorView.Bind(base.ViewModel.PregenSelectionGroupVM);
		m_SettingsEntityDropdownGameDifficultyViewPrefab.Bind(base.ViewModel.SettingsEntityDropdownGameDifficultyVM);
		BuildNavigation();
		CreateInput();
		m_Animator.AppearAnimation();
	}

	protected override void DestroyViewImplementation()
	{
		NavigationBehaviour.Clear();
		NavigationBehaviour = null;
		InputLayer = null;
		m_Animator.DisappearAnimation();
	}

	protected abstract void BuildNavigation();

	protected virtual void CreateInput()
	{
		InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "TempNewGameInputContext"
		});
		CreateInputImpl(InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	protected abstract void CreateInputImpl(InputLayer inputLayer);
}
