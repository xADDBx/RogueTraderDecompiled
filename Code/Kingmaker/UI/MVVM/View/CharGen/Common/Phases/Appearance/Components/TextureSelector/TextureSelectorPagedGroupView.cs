using Kingmaker.UI.Common;
using Kingmaker.UI.Common.PageNavigation;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorPagedGroupView : TextureSelectorGroupView
{
	[SerializeField]
	protected Paginator m_Paginator;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private int m_RowNumber = 1;

	[SerializeField]
	private float m_RowHeight = 43f;

	protected override void BindViewImplementation()
	{
		m_Paginator.Initialize();
		AddDisposable(m_Paginator);
		SetRowNumber(m_RowNumber);
		AddDisposable(base.ViewModel.SelectedEntity.Subscribe(delegate
		{
			UpdatePageIndex();
		}));
		AddDisposable(m_Paginator.UpdateViewTrigger.Subscribe(delegate
		{
			UpdatePageIndex();
		}));
		base.BindViewImplementation();
	}

	public void OnValidate()
	{
		SetRowNumber(m_RowNumber);
	}

	public void SetRowNumber(int n)
	{
		m_LayoutElement.minHeight = (float)n * m_RowHeight;
		m_LayoutElement.preferredHeight = (float)n * m_RowHeight;
	}

	protected override void DrawEntities()
	{
		base.DrawEntities();
		m_Paginator.UpdateView();
	}

	private void UpdatePageIndex()
	{
		int num = base.ViewModel.EntitiesCollection.FindIndex((TextureSelectorItemVM e) => e == base.ViewModel.SelectedEntity.Value);
		if (num == -1)
		{
			base.ViewModel.SelectedEntity.Value = base.ViewModel.EntitiesCollection.FirstItem();
		}
		else
		{
			m_Paginator.PageIndex.Value = num / m_ItemsPerRow;
		}
	}

	public override bool HandleUp()
	{
		return false;
	}

	public override bool HandleDown()
	{
		return false;
	}

	public override bool HandleLeft()
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			return false;
		}
		return base.ViewModel.SelectPrevValidEntity();
	}

	public override bool HandleRight()
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			return false;
		}
		return base.ViewModel.SelectNextValidEntity();
	}

	public override void SetFocus(bool value)
	{
	}

	public override bool CanConfirmClick()
	{
		return false;
	}

	public override void OnConfirmClick()
	{
	}
}
