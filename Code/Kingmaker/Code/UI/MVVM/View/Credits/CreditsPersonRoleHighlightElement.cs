using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsPersonRoleHighlightElement : CreditsPersonElement
{
	[Header("Highlight")]
	[SerializeField]
	protected TextMeshProUGUI NameLabelHighlighted;

	[SerializeField]
	protected TextMeshProUGUI RoleLabelHighlighted;

	[SerializeField]
	protected DOTweenAnimation m_Ping;

	public override void Initialize(string personName, string role, Align align, ICreditsView view)
	{
		base.Initialize(personName, role, align, view);
		NameLabelHighlighted.text = personName;
		bool flag = !string.IsNullOrEmpty(role);
		if (align != 0 && !flag)
		{
			TextMeshProUGUI nameLabelHighlighted = NameLabelHighlighted;
			nameLabelHighlighted.alignment = align switch
			{
				Align.Left => TextAlignmentOptions.Left, 
				Align.Right => TextAlignmentOptions.Right, 
				Align.Center => TextAlignmentOptions.Center, 
				_ => NameLabelHighlighted.alignment, 
			};
		}
		if ((bool)RoleLabelHighlighted)
		{
			RoleLabelHighlighted.gameObject.SetActive(flag);
			if (flag)
			{
				RoleLabelHighlighted.text = role;
			}
		}
	}

	public override void Ping()
	{
		if ((bool)m_Ping)
		{
			m_Ping.DORewind();
			m_Ping.DOPlay();
		}
	}
}
