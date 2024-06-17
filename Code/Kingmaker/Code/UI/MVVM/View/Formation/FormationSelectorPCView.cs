using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Formations;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Formation;

public class FormationSelectorPCView : ViewBase<SelectionGroupRadioVM<FormationSelectionItemVM>>
{
	[SerializeField]
	private FormationSelectionItemPCView m_Item0;

	[SerializeField]
	private FormationSelectionItemPCView m_Item1;

	[SerializeField]
	private FormationSelectionItemPCView m_Item2;

	[SerializeField]
	private FormationSelectionItemPCView m_Item3;

	[SerializeField]
	private FormationSelectionItemPCView m_Item4;

	[SerializeField]
	private FormationSelectionItemPCView m_Item5;

	private Dictionary<int, FormationSelectionItemPCView> m_ItemViews;

	public void Initialize()
	{
		m_ItemViews = new Dictionary<int, FormationSelectionItemPCView>
		{
			{ 0, m_Item0 },
			{ 1, m_Item1 },
			{ 2, m_Item2 },
			{ 3, m_Item3 },
			{ 4, m_Item4 },
			{ 5, m_Item5 }
		};
	}

	protected override void BindViewImplementation()
	{
		ReferenceArrayProxy<BlueprintPartyFormation> predefinedFormations = BlueprintRoot.Instance.Formations.PredefinedFormations;
		foreach (KeyValuePair<int, FormationSelectionItemPCView> itemView in m_ItemViews)
		{
			var (key, formationSelectionItemPCView2) = (KeyValuePair<int, FormationSelectionItemPCView>)(ref itemView);
			formationSelectionItemPCView2.Bind(base.ViewModel.EntitiesCollection.FirstOrDefault((FormationSelectionItemVM s) => s.FormationIndex == key));
			_ = key;
			_ = predefinedFormations.Length;
		}
		UILog.ViewBinded("FormationSelectorPCView");
	}

	protected override void DestroyViewImplementation()
	{
		UILog.ViewUnbinded("FormationSelectorPCView");
	}
}
