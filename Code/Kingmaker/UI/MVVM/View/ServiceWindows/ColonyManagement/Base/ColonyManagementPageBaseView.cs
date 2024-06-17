using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;

public class ColonyManagementPageBaseView : ViewBase<ColonyManagementPageVM>, IColonizationProjectsUIHandler, ISubscriber
{
	[SerializeField]
	private CanvasGroup m_ColonyGroup;

	[SerializeField]
	private CanvasGroup m_ProjectsGroup;

	public void Initialize()
	{
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		SwitchGroup(isProjects: false);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void HandleColonyProjectsUIOpen(Colony colony)
	{
		SwitchGroup(isProjects: true);
	}

	public void HandleColonyProjectsUIClose()
	{
		SwitchGroup(isProjects: false);
	}

	public void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject)
	{
	}

	public void SwitchGroup(bool isProjects)
	{
		SetGroupActive(m_ColonyGroup, !isProjects);
		SetGroupActive(m_ProjectsGroup, isProjects);
	}

	private static void SetGroupActive(CanvasGroup canvasGroup, bool isActive)
	{
		canvasGroup.alpha = (isActive ? 1f : 0f);
		canvasGroup.interactable = isActive;
		canvasGroup.blocksRaycasts = isActive;
	}
}
