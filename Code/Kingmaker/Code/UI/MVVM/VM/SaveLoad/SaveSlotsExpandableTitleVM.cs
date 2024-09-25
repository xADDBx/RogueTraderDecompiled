using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveSlotsExpandableTitleVM : ExpandableTitleVM
{
	private readonly Action m_DeleteAll;

	public readonly ReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	public readonly SaveInfo SaveInfo;

	public SaveSlotsExpandableTitleVM(string title, Action<bool> @switch, bool defaultExpanded = true, Action deleteAll = null, SaveInfo saveInfo = null)
		: base(title, @switch, defaultExpanded)
	{
		m_DeleteAll = deleteAll;
		SaveInfo = saveInfo;
	}

	public void DeleteAll()
	{
		string deleteWarning = string.Concat("<b>", UIStrings.Instance.SaveLoadTexts.AreYouSureDeleteCharacter, Environment.NewLine, UIStrings.Instance.CommonTexts.ThisActionCantBeCanceled, "</b>");
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(deleteWarning.ToUpper(), DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton respond)
			{
				if (respond == DialogMessageBoxBase.BoxButton.Yes)
				{
					m_DeleteAll?.Invoke();
				}
			}, null, UIStrings.Instance.SaveLoadTexts.DeleteCharacter);
		});
	}
}
