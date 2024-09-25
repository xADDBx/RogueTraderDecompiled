using Kingmaker.Code.UI.Common;
using Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Base;

public class SystemInfoAllSystemsInformationWindowBaseView : ViewBase<SystemInfoAllSystemsInformationWindowVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_SystemName;

	[Header("Icons")]
	[SerializeField]
	private Image m_ColonizedIcon;

	[SerializeField]
	private Image m_QuestIcon;

	[SerializeField]
	private Image m_RumourIcon;

	[SerializeField]
	private Image m_ExtractumIcon;

	[Header("Research Count")]
	[SerializeField]
	private TextMeshProUGUI m_ResearchCountText;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_SystemName.text = base.ViewModel.SystemName;
		CheckIconsState();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void CheckIconsState()
	{
		m_ColonizedIcon.gameObject.SetActive(base.ViewModel.CheckColonization());
		if (base.ViewModel.CheckColonization())
		{
			AddDisposable(m_ColonizedIcon.SetHint(UIUtilitySpaceColonization.GetColonizationInformation(base.ViewModel.Colony.Value)));
		}
		m_QuestIcon.gameObject.SetActive(base.ViewModel.CheckQuests());
		if (base.ViewModel.CheckQuests())
		{
			AddDisposable(m_QuestIcon.SetHint(base.ViewModel.QuestObjectiveName.Value));
		}
		m_RumourIcon.Or(null)?.gameObject.SetActive(base.ViewModel.CheckRumours());
		if (base.ViewModel.CheckRumours() && m_RumourIcon != null)
		{
			AddDisposable(m_RumourIcon.SetHint(base.ViewModel.RumourObjectiveName.Value));
		}
		m_ExtractumIcon.gameObject.SetActive(base.ViewModel.CheckExtractum());
		if (base.ViewModel.CheckExtractum())
		{
			AddDisposable(m_ExtractumIcon.SetHint(base.ViewModel.ResourcesHint.Value));
		}
	}

	public void SetCameraOnSystem()
	{
		base.ViewModel.SetCameraOnSystem();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((SystemInfoAllSystemsInformationWindowVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SystemInfoAllSystemsInformationWindowVM;
	}
}
