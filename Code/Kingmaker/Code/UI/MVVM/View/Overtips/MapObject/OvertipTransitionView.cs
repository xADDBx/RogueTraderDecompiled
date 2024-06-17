using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public abstract class OvertipTransitionView : BaseOvertipView<OvertipTransitionVM>
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_TitleBlock;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private CanvasGroup m_ActiveLayer;

	protected override bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.IsVisibleForPlayer.Value && !base.ViewModel.MapObjectEntity.Suppressed && !base.ViewModel.IsCutscene)
			{
				if (Game.Instance.Player.IsInCombat)
				{
					if (Game.Instance.Player.IsInCombat)
					{
						return base.ViewModel.EnableInCombat;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}));
		m_Title.autoSizeTextContainer = false;
		m_Title.autoSizeTextContainer = true;
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_TransitionOvertip";
		AddDisposable(base.ViewModel.HasCharactersMovingToHere.Subscribe(delegate(bool value)
		{
			m_ActiveLayer.alpha = (value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_TitleBlock.alpha = ((!value.IsNullOrEmpty()) ? 1 : 0);
			m_Title.text = value;
		}));
	}
}
