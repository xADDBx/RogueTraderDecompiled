using System;
using Kingmaker.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsPersonElement : CreditElement, ICreditsElement
{
	public enum Align
	{
		None,
		Left,
		Right,
		Center
	}

	[SerializeField]
	protected TextMeshProUGUI NameLabel;

	[SerializeField]
	protected TextMeshProUGUI RoleLabel;

	public virtual void Initialize(string personName, string role, Align align, ICreditsView view)
	{
		base.gameObject.SetActive(value: true);
		NameLabel.text = personName;
		bool flag = !string.IsNullOrEmpty(role);
		if (align != 0 && !flag)
		{
			TextMeshProUGUI nameLabel = NameLabel;
			nameLabel.alignment = align switch
			{
				Align.Left => TextAlignmentOptions.Left, 
				Align.Right => TextAlignmentOptions.Right, 
				Align.Center => TextAlignmentOptions.Center, 
				_ => NameLabel.alignment, 
			};
		}
		if ((bool)RoleLabel)
		{
			RoleLabel.gameObject.SetActive(flag);
			int num = 1;
			if (flag)
			{
				RoleLabel.text = role;
				num = role.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
			}
			LayoutElement component = GetComponent<LayoutElement>();
			float preferredHeight = component.preferredHeight;
			component.preferredHeight = preferredHeight * (float)num;
			component.minWidth = preferredHeight * (float)num;
		}
		base.transform.SetParent(view.Content);
		base.transform.ResetAll();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	public virtual void Ping()
	{
	}
}
