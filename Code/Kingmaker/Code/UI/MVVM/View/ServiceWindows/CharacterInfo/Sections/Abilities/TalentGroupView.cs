using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class TalentGroupView : MonoBehaviour
{
	[Serializable]
	public class GroupViewUnit
	{
		public GameObject Container;

		public Image[] Icons;

		public GroupViewUnit(int count)
		{
			Icons = new Image[count];
		}
	}

	[SerializeField]
	public GameObject IconsContainer;

	[SerializeField]
	public Image BackgroundImage;

	[Header("Groups")]
	[SerializeField]
	public GroupViewUnit OneIconGroup = new GroupViewUnit(1);

	[SerializeField]
	public GroupViewUnit TwoIconGroup = new GroupViewUnit(2);

	[SerializeField]
	public GroupViewUnit ThreeIconGroup = new GroupViewUnit(3);

	private List<GroupViewUnit> Groups => new List<GroupViewUnit> { OneIconGroup, TwoIconGroup, ThreeIconGroup };

	public void SetupView(TalentIconInfo iconsInfo)
	{
		if (iconsInfo == null)
		{
			SetActiveState(state: false);
		}
		else
		{
			SetupView(iconsInfo.AllGroups, iconsInfo.MainGroup);
		}
	}

	public void SetupView(TalentGroup allGroups, TalentGroup mainGroup)
	{
		if (allGroups == (TalentGroup)0)
		{
			SetActiveState(state: false);
			return;
		}
		List<TalentGroup> valuesFromFlags = GetValuesFromFlags(allGroups);
		if (allGroups.HasFlag(mainGroup))
		{
			valuesFromFlags.Remove(mainGroup);
			valuesFromFlags.Insert(0, mainGroup);
		}
		int num = Mathf.Clamp(valuesFromFlags.Count, 1, 3);
		GroupViewUnit groupViewUnit = num switch
		{
			1 => OneIconGroup, 
			2 => TwoIconGroup, 
			3 => ThreeIconGroup, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (groupViewUnit.Icons.Length != valuesFromFlags.Count || num > valuesFromFlags.Count)
		{
			PFLog.UI.Error("TalentGroupViewConfig has mismatch groups count");
			return;
		}
		for (int i = 0; i < num; i++)
		{
			TalentGroups.TalentGroupConfig config = UIConfig.Instance.TalentGroups.GetConfig(valuesFromFlags[i]);
			groupViewUnit.Icons[i].sprite = config.Icon;
		}
		foreach (GroupViewUnit group in Groups)
		{
			group.Container.SetActive(group == groupViewUnit);
		}
		if ((bool)BackgroundImage)
		{
			BackgroundImage.sprite = UIConfig.Instance.UIIcons.EmptyAbilityIcon;
			BackgroundImage.color = UIConfig.Instance.TalentGroups.GetConfig(valuesFromFlags[0]).BgrColor;
		}
		SetActiveState(state: true);
	}

	private List<TalentGroup> GetValuesFromFlags(TalentGroup flags)
	{
		List<TalentGroup> list = new List<TalentGroup>();
		foreach (TalentGroup value in Enum.GetValues(typeof(TalentGroup)))
		{
			if (flags.HasFlag(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public void SetActiveState(bool state)
	{
		IconsContainer.Or(null)?.SetActive(state);
	}
}
