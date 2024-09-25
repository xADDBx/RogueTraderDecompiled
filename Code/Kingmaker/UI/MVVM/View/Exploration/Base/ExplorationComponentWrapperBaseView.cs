using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public abstract class ExplorationComponentWrapperBaseView<T> : ExplorationComponentBaseView<T> where T : ExplorationUIComponentWrapperVM
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	public abstract IEnumerable<IConsoleNavigationEntity> GetNavigationEntities();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.ActiveOnScreen.Subscribe(SetActiveOnScreen));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetActiveOnScreen(bool isActive)
	{
		m_CanvasGroup.alpha = (isActive ? 1f : 0f);
		m_CanvasGroup.interactable = isActive;
		m_CanvasGroup.blocksRaycasts = isActive;
	}
}
