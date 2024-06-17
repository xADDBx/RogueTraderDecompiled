using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Common;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SaveLoad.Base;

public class SaveSlotsExpandableTitleView : ExpandableTitleView, IFunc01ClickHandler, IConsoleEntity, ILongFunc01ClickHandler
{
	[SerializeField]
	private OwlcatMultiButton m_DeleteAllButton;

	private ContextMenuCollectionEntity m_DeleteAllSavesEntity;

	private SaveSlotsExpandableTitleVM SaveSlotsExpandableTitleVM => base.ViewModel as SaveSlotsExpandableTitleVM;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_DeleteAllButton.SetHint(UIStrings.Instance.SaveLoadTexts.DeleteCharacter));
		SetupContextMenu();
		AddDisposable(m_DeleteAllButton.SetContextMenu(SaveSlotsExpandableTitleVM.ContextMenu, isLeftClick: true));
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
