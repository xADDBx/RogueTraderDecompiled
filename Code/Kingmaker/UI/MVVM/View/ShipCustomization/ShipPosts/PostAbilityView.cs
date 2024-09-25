using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostAbilityView : ViewBase<PostAbilityVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_IconGrayScale;

	[SerializeField]
	private Sprite m_EmptySprite;

	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	protected override void BindViewImplementation()
	{
		m_Icon.gameObject.SetActive(base.ViewModel.Icon.Value != null);
		AddDisposable(base.ViewModel.Icon.Subscribe(UpdateIcon));
		m_Selectable.SetActiveLayer((!base.ViewModel.IsUnlocked) ? 1 : 0);
		AddDisposable(m_Icon.SetTooltip(base.ViewModel.TooltipTemplateAbility, new TooltipConfig
		{
			TooltipPlace = (base.transform.parent.transform.parent.transform as RectTransform)
		}));
		UpdateIcon(base.ViewModel.Icon.Value);
	}

	private void UpdateIcon(Sprite sprite)
	{
		m_Icon.sprite = ((sprite != null) ? sprite : m_EmptySprite);
		m_IconGrayScale.sprite = ((sprite != null) ? sprite : null);
	}

	protected override void DestroyViewImplementation()
	{
		m_Icon.sprite = m_EmptySprite;
	}

	public void SetupIcon()
	{
		m_Icon.gameObject.SetActive(base.ViewModel != null);
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Selectable.IsValid();
	}

	public Vector2 GetPosition()
	{
		return m_Selectable.gameObject.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
