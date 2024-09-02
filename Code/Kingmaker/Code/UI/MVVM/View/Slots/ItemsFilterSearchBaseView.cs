using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public abstract class ItemsFilterSearchBaseView : ViewBase<ItemsFilterSearchVM>
{
	[Header("InputField Part")]
	[SerializeField]
	protected TMP_InputField m_InputField;

	[SerializeField]
	protected TextMeshProUGUI m_Placeholder;

	[SerializeField]
	private float m_UpdateTextDelay = 0.25f;

	protected List<string> DropdownValues;

	private IDisposable m_FinishUpdateTextInvoker;

	public virtual void Initialize()
	{
		string text = LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Scroll);
		string text2 = LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Unlearned);
		DropdownValues = new List<string>
		{
			text,
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Wand),
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Utility),
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Potion),
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Recipe),
			text2
		};
	}

	protected override void BindViewImplementation()
	{
		m_Placeholder.text = UIStrings.Instance.CharGen.EnterSearchTextHere;
	}

	protected override void DestroyViewImplementation()
	{
		m_FinishUpdateTextInvoker?.Dispose();
	}

	public abstract void SetActive(bool value);

	protected void OnSearchStringEdit(string value)
	{
		m_FinishUpdateTextInvoker?.Dispose();
		if (value == string.Empty)
		{
			base.ViewModel.SetSearchString(value);
			return;
		}
		m_FinishUpdateTextInvoker = DelayedInvoker.InvokeInTime(delegate
		{
			base.ViewModel.SetSearchString(value);
		}, m_UpdateTextDelay);
	}
}
