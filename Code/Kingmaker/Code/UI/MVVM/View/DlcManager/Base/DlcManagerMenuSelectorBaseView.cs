using Kingmaker.Code.UI.MVVM.VM.DlcManager;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Base;

public class DlcManagerMenuSelectorBaseView : ViewBase<SelectionGroupRadioVM<DlcManagerMenuEntityVM>>
{
	[SerializeField]
	private DlcManagerMenuEntityBaseView m_DlcsButton;

	[SerializeField]
	private DlcManagerMenuEntityBaseView m_ModsButton;

	[SerializeField]
	private GameObject m_Selector;

	[SerializeField]
	private float m_LensSwitchAnimationDuration = 0.55f;

	[SerializeField]
	private RectTransform m_DlcsButtonSpinArrow;

	private bool m_IsInit;

	private bool m_OnlyMods;

	private bool m_IsConsole;

	public void Initialize(bool onlyMods, bool isConsole)
	{
		if (!m_IsInit)
		{
			m_OnlyMods = onlyMods;
			m_IsConsole = isConsole;
			m_DlcsButton.gameObject.SetActive(!onlyMods);
			m_DlcsButtonSpinArrow.gameObject.SetActive(!onlyMods && !isConsole);
			m_ModsButton.gameObject.SetActive(!isConsole);
			if (!onlyMods)
			{
				m_DlcsButton.Initialize();
			}
			if (!isConsole)
			{
				m_ModsButton.Initialize();
			}
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		if (!m_OnlyMods)
		{
			m_DlcsButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((DlcManagerMenuEntityVM e) => e.DlcManagerTabVM is DlcManagerTabDlcsVM));
		}
		if (!m_IsConsole)
		{
			m_ModsButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((DlcManagerMenuEntityVM e) => e.DlcManagerTabVM is DlcManagerTabModsVM));
		}
		AddDisposable(base.ViewModel.SelectedEntity.Skip(1).Subscribe(delegate(DlcManagerMenuEntityVM selectedEntity)
		{
			DlcManagerMenuEntityBaseView dlcManagerMenuEntityBaseView = ((selectedEntity.DlcManagerTabVM is DlcManagerTabDlcsVM && !m_OnlyMods) ? m_DlcsButton : ((!m_IsConsole) ? m_ModsButton : null));
			if (!(dlcManagerMenuEntityBaseView == null) && m_Selector.transform.localPosition.x != dlcManagerMenuEntityBaseView.transform.localPosition.x)
			{
				UIUtility.MoveXLensPosition(m_Selector.transform, dlcManagerMenuEntityBaseView.transform.localPosition.x, m_LensSwitchAnimationDuration);
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
			UIUtility.MoveLensPosition(m_Selector.transform, (!m_OnlyMods) ? m_DlcsButton.transform.localPosition : m_ModsButton.transform.localPosition, m_LensSwitchAnimationDuration);
		}, 1);
	}

	public void OnNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void OnPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}
}
