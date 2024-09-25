using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveLoadPortraitBaseView : ViewBase<SaveLoadPortraitVM>, IWidgetView
{
	[SerializeField]
	private Image m_Portrait;

	[Header("Character Rank")]
	[SerializeField]
	private GameObject m_RankGameObject;

	[SerializeField]
	private TextMeshProUGUI m_RankLabel;

	private bool m_IsInit;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.Or(null)?.SetActive(value: true);
		m_Portrait.sprite = base.ViewModel.Portrait;
		SetRank();
	}

	protected override void DestroyViewImplementation()
	{
		if (this != null && base.gameObject != null)
		{
			base.gameObject.Or(null)?.SetActive(value: false);
		}
	}

	private void SetRank()
	{
		if (!(m_RankGameObject == null) && !(m_RankLabel == null))
		{
			m_RankGameObject.Or(null)?.SetActive(!string.IsNullOrEmpty(base.ViewModel.Rank));
			m_RankLabel.text = base.ViewModel.Rank;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as SaveLoadPortraitVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SaveLoadPortraitVM;
	}
}
