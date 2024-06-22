using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.Stores;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;

public class DlcManagerDlcEntityVM : SelectionGroupEntityVM
{
	public readonly BlueprintDlc BlueprintDlc;

	private Action m_SetDlc;

	public readonly string Title;

	public readonly Sprite Art;

	public readonly DlcTypeEnum DlcType;

	private readonly string ICH_HABE_ES_GESEHEN_PREF_KEY;

	public readonly BoolReactiveProperty SawThisDlc = new BoolReactiveProperty();

	private bool CurrentSawValue => PlayerPrefs.GetInt(ICH_HABE_ES_GESEHEN_PREF_KEY, 0) == 1;

	public DlcManagerDlcEntityVM(BlueprintDlc blueprintDlc, Action setDlc)
		: base(allowSwitchOff: false)
	{
		BlueprintDlc = blueprintDlc;
		m_SetDlc = setDlc;
		Title = blueprintDlc.GetDlcName();
		Art = ((blueprintDlc.DlcItemArtLink != null) ? blueprintDlc.DlcItemArtLink : UIConfig.Instance.DlcEntityKeyArt);
		DlcType = blueprintDlc.DlcType;
		ICH_HABE_ES_GESEHEN_PREF_KEY = "DLCMANAGER_I_SAW_" + blueprintDlc.name;
		SawThisDlc.Value = CurrentSawValue;
	}

	protected override void DisposeImplementation()
	{
		m_SetDlc = null;
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
		m_SetDlc?.Invoke();
		if (!CurrentSawValue)
		{
			PlayerPrefs.SetInt(ICH_HABE_ES_GESEHEN_PREF_KEY, 1);
			PlayerPrefs.Save();
			if (SawThisDlc.Value != CurrentSawValue)
			{
				SawThisDlc.Value = CurrentSawValue;
			}
		}
	}
}
