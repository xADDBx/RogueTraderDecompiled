using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Common;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SaveLoad.Base;

public class SaveSlotsExpandableTitleView : ExpandableTitleView, IFunc01ClickHandler, IConsoleEntity, ILongFunc01ClickHandler
{
	[SerializeField]
	private OwlcatMultiButton m_DeleteAllButton;

	[SerializeField]
	private TextMeshProUGUI m_IronManDescription;

	private ContextMenuCollectionEntity m_DeleteAllSavesEntity;

	private SaveSlotsExpandableTitleVM SaveSlotsExpandableTitleVM => base.ViewModel as SaveSlotsExpandableTitleVM;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_DeleteAllButton.SetHint(UIStrings.Instance.SaveLoadTexts.DeleteCharacter));
		SetupContextMenu();
		AddDisposable(m_DeleteAllButton.SetContextMenu(SaveSlotsExpandableTitleVM.ContextMenu, isLeftClick: true));
		SetIronManDescription();
	}

	private void SetIronManDescription()
	{
		bool flag = SaveSlotsExpandableTitleVM.SaveInfo != null;
		m_IronManDescription.Or(null)?.gameObject.SetActive(flag);
		if (flag && !(m_IronManDescription == null))
		{
			SaveSlotsExpandableTitleVM saveSlotsExpandableTitleVM = SaveSlotsExpandableTitleVM;
			string text = ((saveSlotsExpandableTitleVM != null && (saveSlotsExpandableTitleVM.SaveInfo?.GameStartSystemTime).HasValue) ? (SaveSlotsExpandableTitleVM?.SaveInfo?.GameStartSystemTime.Value.ToString("dd/MM/yyyy") + " ") : string.Empty);
			m_IronManDescription.text = "- - -[ " + UIStrings.Instance.SaveLoadTexts.SavePrefixIronman.Text + " " + text + "]- - -";
		}
	}

	protected virtual void SetupContextMenu()
	{
		m_DeleteAllSavesEntity = new ContextMenuCollectionEntity(UIStrings.Instance.SaveLoadTexts.DeleteCharacter, DeleteAllSaves, condition: true);
		SaveSlotsExpandableTitleVM.ContextMenu.Value = new List<ContextMenuCollectionEntity> { m_DeleteAllSavesEntity };
	}

	public void DeleteAllSaves()
	{
		SaveSlotsExpandableTitleVM.DeleteAll();
	}

	public bool CanFunc01Click()
	{
		return true;
	}

	public void OnFunc01Click()
	{
	}

	public string GetFunc01ClickHint()
	{
		return string.Empty;
	}

	public bool CanLongFunc01Click()
	{
		return true;
	}

	public void OnLongFunc01Click()
	{
		DeleteAllSaves();
	}

	public string GetLongFunc01ClickHint()
	{
		return string.Empty;
	}
}
