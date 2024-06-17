using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.OvertipParts;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public abstract class OvertipMapObjectSimpleView : BaseOvertipMapObjectView
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[SerializeField]
	private MapObjectOvertipNameBlockView m_OvertipNameBlockPCView;

	[SerializeField]
	private MapObjectOvertipSkillCheckBlockView m_OvertipSkillCheckBlockPCView;

	[Header("Common Block")]
	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	protected bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.IsMouseOverUI.Value && !base.ViewModel.MapObjectIsHighlited.Value && !base.ViewModel.ForceHotKeyPressed.Value && !base.ViewModel.ActiveCharacterIsNear && !base.ViewModel.IsBarkActive.Value && !base.ViewModel.HasSurrounding.Value)
			{
				return base.ViewModel.IsChosen.Value;
			}
			return true;
		}
	}

	protected override bool CheckVisibility
	{
		get
		{
			if (base.CheckCanBeVisible)
			{
				return CheckVisibleTrigger;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_OvertipMapObjectSimpleView";
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		m_OvertipNameBlockPCView.Bind(base.ViewModel);
		m_OvertipSkillCheckBlockPCView.Bind(base.ViewModel);
		m_InnerCanvasGroup.blocksRaycasts = false;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_BarkBlockPCView.Unbind();
		m_OvertipNameBlockPCView.Unbind();
		m_OvertipSkillCheckBlockPCView.Unbind();
	}
}
