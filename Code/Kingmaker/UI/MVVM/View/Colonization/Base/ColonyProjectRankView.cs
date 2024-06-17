using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectRankView : ViewBase<ColonyProjectRankVM>
{
	[SerializeField]
	private TextMeshProUGUI m_RankTitle;

	[SerializeField]
	private TextMeshProUGUI m_RankValue;

	[SerializeField]
	private RectTransform m_Container;

	public void Initialize()
	{
		m_RankTitle.text = UIStrings.Instance.ColonyProjectsTexts.Rank.Text;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Rank.Subscribe(delegate(ColonyProjectRank val)
		{
			m_RankValue.text = UIUtility.ArabicToRoman((int)val);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetAsFirstSibling()
	{
		m_Container.SetAsFirstSibling();
	}
}
