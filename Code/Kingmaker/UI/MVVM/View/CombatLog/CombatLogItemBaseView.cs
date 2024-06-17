using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.MVVM.VM.CombatLog;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CombatLog;

public abstract class CombatLogItemBaseView : VirtualListElementViewBase<CombatLogItemVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	private CanvasGroup m_PrefixCanvasGroup;

	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_NumberText;

	[Space]
	[SerializeField]
	private VirtualListLayoutElementSettings m_VirtualListSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_VirtualListSettings;

	protected override void BindViewImplementation()
	{
		m_Text.text = base.ViewModel.MessageText;
		SetTextColor(base.ViewModel.TextColor);
		SetIcon();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetIcon()
	{
		Sprite icon = GameLogUtility.GetIcon((base.ViewModel.ShotNumber > 0) ? PrefixIcon.Empty : base.ViewModel.PrefixIcon);
		if (icon != null)
		{
			m_IconImage.sprite = icon;
		}
		TextMeshProUGUI numberText = m_NumberText;
		int shotNumber = base.ViewModel.ShotNumber;
		numberText.text = shotNumber.ToString();
		m_NumberText.alpha = ((base.ViewModel.ShotNumber > 0) ? 1f : 0f);
		CanvasGroup prefixCanvasGroup = m_PrefixCanvasGroup;
		PrefixIcon prefixIcon = base.ViewModel.PrefixIcon;
		prefixCanvasGroup.alpha = ((prefixIcon == PrefixIcon.None || prefixIcon == PrefixIcon.Invisible) ? 0f : 1f);
	}

	private void SetTextColor(Color color)
	{
		m_Text.color = ((color.a > 0f) ? color : ((Color)GameLogStrings.Instance.DefaultColor));
	}

	public virtual void UpdateTextSize(float multiplier)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
