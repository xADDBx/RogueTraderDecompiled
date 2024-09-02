using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.SwitchOnDlcs;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;

public class DlcManagerTabSwitchOnDlcsBaseView : ViewBase<DlcManagerTabSwitchOnDlcsVM>
{
	[Header("Common")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Texts")]
	[SerializeField]
	protected TextMeshProUGUI m_InstalledDlcsHeaderLabel;

	[SerializeField]
	protected TextMeshProUGUI m_YouDontHaveAnyDlcsLabel;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		ScrollToTop();
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				ScrollToTop();
				base.ViewModel.SelectedEntity.Value?.ShowDescription(state: true);
			}
		}));
		m_InstalledDlcsHeaderLabel.text = UIStrings.Instance.DlcManager.InstalledDlcs;
		m_YouDontHaveAnyDlcsLabel.text = UIStrings.Instance.DlcManager.YouDontHaveAnyInstalledDlcs;
		m_YouDontHaveAnyDlcsLabel.transform.parent.gameObject.SetActive(!base.ViewModel.HaveDlcs);
		if (base.ViewModel.SelectionGroup.EntitiesCollection.Any())
		{
			m_InfoView.Bind(base.ViewModel.InfoVM);
		}
		SetTextFontSize(base.ViewModel.FontMultiplier);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void ScrollToTop()
	{
		m_ScrollRect.Or(null)?.ScrollToTop();
		m_InfoView.Or(null)?.ScrollRectExtended.Or(null)?.ScrollToTop();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.Or(null)?.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	protected virtual void SetTextFontSize(float multiplier)
	{
	}
}
