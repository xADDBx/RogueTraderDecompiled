using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;

public class DlcManagerTabDlcsVM : DlcManagerTabBaseVM
{
	public readonly ReactiveProperty<DlcManagerDlcEntityVM> SelectedEntity = new ReactiveProperty<DlcManagerDlcEntityVM>();

	public readonly SelectionGroupRadioVM<DlcManagerDlcEntityVM> SelectionGroup;

	private BlueprintDlc m_CurrentDlc;

	public readonly ReactiveCommand ChangeStory = new ReactiveCommand();

	public readonly BoolReactiveProperty DlcIsBought = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsAvailableToPurchase = new BoolReactiveProperty();

	public ReactiveProperty<Sprite> Art { get; } = new ReactiveProperty<Sprite>();


	public ReactiveProperty<VideoClip> Video { get; } = new ReactiveProperty<VideoClip>();


	public ReactiveProperty<string> SoundStart { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> SoundStop { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();


	public DlcManagerTabDlcsVM()
	{
		List<DlcManagerDlcEntityVM> list = new List<DlcManagerDlcEntityVM>();
		foreach (IBlueprintDlc purchasableDLC in StoreManager.GetPurchasableDLCs())
		{
			BlueprintDlc dlc = purchasableDLC as BlueprintDlc;
			if (dlc == null || !dlc.HideDlc)
			{
				DlcManagerDlcEntityVM dlcManagerDlcEntityVM = new DlcManagerDlcEntityVM(dlc, delegate
				{
					SetDlc(dlc);
				});
				list.Add(dlcManagerDlcEntityVM);
				AddDisposable(dlcManagerDlcEntityVM);
			}
		}
		List<DlcManagerDlcEntityVM> list2 = list.OrderBy((DlcManagerDlcEntityVM dlc) => dlc.DlcType).ToList();
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerDlcEntityVM>(list2, SelectedEntity);
		AddDisposable(SelectionGroup);
		SelectedEntity.Value = list2.FirstOrDefault();
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetDlc(BlueprintDlc blueprintDlc)
	{
		if (blueprintDlc != null)
		{
			m_CurrentDlc = blueprintDlc;
			Video.Value = blueprintDlc.DefaultVideo;
			SoundStart.Value = blueprintDlc.SoundStartEvent;
			SoundStop.Value = blueprintDlc.SoundStopEvent;
			Art.Value = ((Video.Value != null) ? null : ((blueprintDlc.DefaultKeyArt != null) ? blueprintDlc.DefaultKeyArt : UIConfig.Instance.KeyArt));
			Description.Value = blueprintDlc.DlcDescription;
			DlcIsBought.Value = blueprintDlc.IsPurchased;
			DlcIsAvailableToPurchase.Value = blueprintDlc.GetPurchaseState() != BlueprintDlc.DlcPurchaseState.ComingSoon;
			ChangeStory.Execute();
		}
	}

	public void ShowInStore()
	{
		if (m_CurrentDlc != null)
		{
			PFLog.UI.Log($"Open {m_CurrentDlc} store");
			StoreManager.OpenShopFor(m_CurrentDlc);
		}
	}
}
