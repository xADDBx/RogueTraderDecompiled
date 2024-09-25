using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;

public class FeedbackPopupItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Label;

	public readonly Sprite Icon;

	private readonly FeedbackPopupItem m_Config;

	public FeedbackPopupItemVM(FeedbackPopupItem config)
	{
		m_Config = config;
		Label = UIStrings.Instance.FeedbackPopupTexts.GetTitleByPopupItemType(config.ItemType);
		Icon = UIConfig.Instance.FeedbackConfig.GetIconByPopupItemType(config.ItemType);
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleClick()
	{
		Application.OpenURL(m_Config.Url);
	}
}
