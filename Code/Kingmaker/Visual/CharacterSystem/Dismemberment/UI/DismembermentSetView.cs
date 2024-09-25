using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentSetView : ViewBase<DismembermentSetVM>, IWidgetView
{
	public TMP_Dropdown DropdownType;

	public Toggle ToggleSelect;

	public Button ButtonRemove;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as DismembermentSetVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DismembermentSetVM;
	}

	protected override void BindViewImplementation()
	{
		ToggleSelect.group = base.transform.parent.GetComponent<ToggleGroup>();
		ToggleSelect.onValueChanged.AddListener(base.ViewModel.OnSelectedChanged);
		ButtonRemove.onClick.AddListener(OnRemove);
		string[] names = Enum.GetNames(typeof(DismembermentLimbsApartType));
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		for (int i = 0; i < names.Length; i++)
		{
			list.Add(new TMP_Dropdown.OptionData(names[i]));
		}
		DropdownType.ClearOptions();
		DropdownType.AddOptions(list);
		DropdownType.value = (int)base.ViewModel.Set.Type;
		DropdownType.onValueChanged.AddListener(base.ViewModel.OnTypeChanged);
	}

	private void OnRemove()
	{
		ToggleSelect.group.SetAllTogglesOff();
		base.ViewModel.OnRemove();
	}

	protected override void DestroyViewImplementation()
	{
		ToggleSelect.onValueChanged.RemoveAllListeners();
		ButtonRemove.onClick.RemoveAllListeners();
		DropdownType.onValueChanged.RemoveAllListeners();
	}
}
