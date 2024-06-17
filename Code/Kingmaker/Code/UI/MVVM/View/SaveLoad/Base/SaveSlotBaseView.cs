using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.PC;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveSlotBaseView : SelectionGroupEntityView<SaveSlotVM>, IWidgetView
{
	[SerializeField]
	protected bool m_IsDetailedView;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_LocationLabel;

	[SerializeField]
	private TextMeshProUGUI m_TimeInGameLabel;

	[SerializeField]
	private GameObject m_DlcRequiredBlock;

	[SerializeField]
	private TextMeshProUGUI m_DlcRequiredLabel;

	[SerializeField]
	private TextMeshProUGUI m_DlcRequiredDescription;

	[Header("Decoration")]
	[SerializeField]
	protected RawImage m_ScreenshotImage;

	[SerializeField]
	private GameObject m_AutoSaveMark;

	[SerializeField]
	private GameObject m_QuickSaveMark;

	[Header("Portraits")]
	[SerializeField]
	private SaveLoadPortraitBaseView m_PortraitPrefab;

	[SerializeField]
	private WidgetListMVVM m_WidgetListMvvm;

	protected UISaveLoadTexts Texts => BlueprintRoot.Instance.LocalizedTexts.UserInterfacesText.SaveLoadTexts;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.SetActive(value: true);
		if (!(this is NewSaveSlotPCView) && !(this is NewSaveSlotConsoleView))
		{
			base.ViewModel.SetAvailable(state: true);
		}
		if (m_DlcRequiredLabel != null)
		{
			m_DlcRequiredLabel.text = Texts.DlcRequired;
			AddDisposable(base.ViewModel.ShowDlcRequiredLabel.Subscribe(UpdateDLCState));
		}
		UpdateDLCView();
		if (m_PortraitPrefab != null && m_WidgetListMvvm != null)
		{
			AddDisposable(base.ViewModel.PartyPortraits.Subscribe(SetPortraits));
		}
		AddDisposable(base.ViewModel.SaveName.Subscribe(SetSaveName));
		AddDisposable(base.ViewModel.LocationName.Subscribe(SetLocationName));
		AddDisposable(base.ViewModel.TimeInGame.Subscribe(SetTimeInGame));
		AddDisposable(base.ViewModel.ScreenShot.Subscribe(SetScreenshot));
		AddDisposable(base.ViewModel.ShowAutoSaveMark.Subscribe(ShowAutoSaveMark));
		AddDisposable(base.ViewModel.ShowQuickSaveMark.Subscribe(ShowQuickSaveMark));
		base.ViewModel.UpdateScreenshot();
	}

	protected override void DestroyViewImplementation()
	{
		if (!m_IsDetailedView)
		{
			base.ViewModel.SetAvailable(state: false);
		}
		base.gameObject.SetActive(value: false);
		base.DestroyViewImplementation();
	}

	private void UpdateDLCView()
	{
		if (m_DlcRequiredDescription == null || base.ViewModel.DlcRequiredMap == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (List<string> item in base.ViewModel.DlcRequiredMap)
		{
			bool flag = false;
			foreach (string item2 in item)
			{
				stringBuilder.Append(flag ? (" " + UIStrings.Instance.Tooltips.or.Text + " ") : "- ");
				stringBuilder.Append(item2 ?? "");
				flag = true;
			}
			stringBuilder.Append("\n");
		}
		m_DlcRequiredDescription.text = stringBuilder.ToString();
	}

	private void SetSaveName(string s)
	{
		string text = base.ViewModel.SystemSaveTime.Value.ToString("g");
		m_NameLabel.text = text + " " + s;
	}

	private void SetLocationName(string s)
	{
		m_LocationLabel.transform.gameObject.SetActive(!string.IsNullOrWhiteSpace(s));
		m_LocationLabel.text = s;
	}

	private void SetTimeInGame(string s)
	{
		m_TimeInGameLabel.text = s;
	}

	private void SetScreenshot(Texture2D screenshot)
	{
		m_ScreenshotImage.gameObject.SetActive(screenshot != null);
		if (!(screenshot == null))
		{
			m_ScreenshotImage.texture = screenshot;
			m_ScreenshotImage.GetComponent<AspectRatioFitter>().aspectRatio = screenshot.GetAspect();
		}
	}

	private void SetPortraits(List<SaveLoadPortraitVM> sprites)
	{
		m_WidgetListMvvm.DrawEntries(base.ViewModel.PartyPortraits.Value?.ToArray(), m_PortraitPrefab);
	}

	private void ShowAutoSaveMark(bool b)
	{
		m_AutoSaveMark.SetActive(b);
	}

	private void ShowQuickSaveMark(bool b)
	{
		m_QuickSaveMark.SetActive(b);
	}

	protected virtual void UpdateDLCState(bool b)
	{
		m_DlcRequiredLabel.gameObject.SetActive(b);
		if (m_DlcRequiredBlock != null)
		{
			m_DlcRequiredBlock.SetActive(b);
		}
		UpdateDLCView();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as SaveSlotVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SaveSlotVM;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelectedFromView(value);
		m_Button.CanConfirm = true;
	}
}
