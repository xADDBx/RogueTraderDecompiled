using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostEntityView : SelectionGroupEntityView<PostEntityVM>
{
	[Header("Common")]
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private GameObject m_EmptyPortrait;

	[SerializeField]
	private Image m_PostSprite;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private CanvasGroup m_PostBlock;

	[SerializeField]
	private int m_EmptyAlpha = 125;

	[SerializeField]
	private int m_NotEmptyAlpha = 255;

	[Header("Abilities")]
	[SerializeField]
	protected GameObject m_AbilitiesBlock;

	[SerializeField]
	protected PostAbilitiesGroupBaseView PostAbilitiesGroupPCView;

	private Tweener m_PostBlockTw;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		FadePost(show: false, 1f);
		m_Portrait.sprite = (base.ViewModel.Portrait ? base.ViewModel.Portrait : null);
		m_Portrait.gameObject.SetActive(base.ViewModel.Portrait != null);
		m_EmptyPortrait.gameObject.SetActive(!m_Portrait.gameObject.activeSelf);
		byte a = (byte)((m_Portrait.sprite == base.ViewModel.Portrait) ? m_NotEmptyAlpha : m_EmptyAlpha);
		m_Portrait.color = new Color32((byte)m_NotEmptyAlpha, (byte)m_NotEmptyAlpha, (byte)m_NotEmptyAlpha, a);
		UISpaceCombatTexts.SpacePostStrings postStrings = UIStrings.Instance.SpaceCombatTexts.GetPostStrings(base.ViewModel.Index);
		m_Title.text = postStrings.Title;
		m_PostSprite.sprite = base.ViewModel.Post.PostData.PostSpriteHolographic;
		PostAbilitiesGroupPCView.Bind(base.ViewModel.AbilitiesGroup);
		AddDisposable(base.ViewModel.IsPostBlocked.Subscribe(ShowPostBlocked));
		AddDisposable(base.ViewModel.OnPostUpdate.Subscribe(OnUpdatePost));
	}

	protected override void DestroyViewImplementation()
	{
		m_PostBlockTw?.Kill();
	}

	private void OnUpdatePost()
	{
		m_Portrait.sprite = (base.ViewModel.Portrait ? base.ViewModel.Portrait : null);
		m_Portrait.gameObject.SetActive(base.ViewModel.Portrait != null);
		m_EmptyPortrait.gameObject.SetActive(!m_Portrait.gameObject.activeSelf);
		byte a = (byte)((m_Portrait.sprite == base.ViewModel.Portrait) ? m_NotEmptyAlpha : m_EmptyAlpha);
		m_Portrait.color = new Color32((byte)m_NotEmptyAlpha, (byte)m_NotEmptyAlpha, (byte)m_NotEmptyAlpha, a);
	}

	private void ShowPostBlocked(bool val)
	{
		FadePost(val, 2f);
		if (val)
		{
			m_AbilitiesBlock.SetActive(value: false);
			m_Portrait.gameObject.SetActive(value: false);
		}
	}

	private void FadePost(bool show, float duration)
	{
		m_PostBlockTw?.Kill();
		m_PostBlock.gameObject.SetActive(show);
		m_PostBlock.DOFade(show ? 1f : 0f, duration);
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.AddEntityHorizontal(this);
		gridConsoleNavigationBehaviour.AddEntityHorizontal(PostAbilitiesGroupPCView.GetNavigation(isReverse: true));
		m_NavigationBehaviour.AddEntityGrid(gridConsoleNavigationBehaviour);
		return m_NavigationBehaviour;
	}

	protected override string GetConfirmActionName()
	{
		return UIStrings.Instance.CommonTexts.Select.Text;
	}
}
