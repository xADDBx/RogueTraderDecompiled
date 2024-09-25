using Kingmaker.Code.UI.MVVM.VM.NewGame.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Menu;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGameMenuSelectorBaseView : ViewBase<SelectionGroupRadioVM<NewGameMenuEntityVM>>
{
	[SerializeField]
	private NewGameMenuEntityBaseView m_GameModeButton;

	[SerializeField]
	private NewGameMenuEntityBaseView m_DifficultyButton;

	[SerializeField]
	private GameObject m_Selector;

	[SerializeField]
	private float m_LensSwitchAnimationDuration = 0.55f;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_GameModeButton.Initialize();
			m_DifficultyButton.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_GameModeButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((NewGameMenuEntityVM e) => e.NewGamePhaseVM is NewGamePhaseStoryVM));
		m_DifficultyButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((NewGameMenuEntityVM e) => e.NewGamePhaseVM is NewGamePhaseDifficultyVM));
		AddDisposable(base.ViewModel.SelectedEntity.Skip(1).Subscribe(delegate(NewGameMenuEntityVM selectedEntity)
		{
			NewGameMenuEntityBaseView newGameMenuEntityBaseView = ((selectedEntity.NewGamePhaseVM is NewGamePhaseStoryVM) ? m_GameModeButton : m_DifficultyButton);
			if (m_Selector.transform.localPosition.x != newGameMenuEntityBaseView.transform.localPosition.x)
			{
				UIUtility.MoveXLensPosition(m_Selector.transform, newGameMenuEntityBaseView.transform.localPosition.x, m_LensSwitchAnimationDuration);
			}
		}));
		ResetLensPosition();
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
	}

	private void ResetLensPosition()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			UIUtility.MoveLensPosition(m_Selector.transform, m_GameModeButton.transform.localPosition, m_LensSwitchAnimationDuration);
		}, 1);
	}
}
