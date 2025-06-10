using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public abstract class LootCollectorView : ViewBase<LootCollectorVM>
{
	[Header("Loot Objects")]
	[SerializeField]
	public LootObjectView m_LootToInventory;

	[FormerlySerializedAs("m_LootToLoot")]
	[SerializeField]
	public LootObjectView m_LootToCargo;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Skill Check")]
	[SerializeField]
	private GameObject m_SkillCheckContainer;

	[SerializeField]
	private TextMeshProUGUI m_SkillCheckText;

	[FormerlySerializedAs("m_AllToCargoText")]
	[SerializeField]
	protected TextMeshProUGUI m_ToCargoText;

	[FormerlySerializedAs("m_AllToInventoryText")]
	[SerializeField]
	protected TextMeshProUGUI m_ToInventoryText;

	public virtual void Initialize()
	{
		m_LootToInventory.Initialize();
		m_LootToCargo.Initialize();
		Hide();
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_SkillCheckContainer.SetActive(base.ViewModel.HasSkillCheck);
		m_SkillCheckText.text = base.ViewModel.SkillCheckText;
		m_LootToInventory.Bind(base.ViewModel.ContextLoot[0]);
		m_LootToCargo.Bind(base.ViewModel.ContextLoot[1]);
		AddDisposable(base.ViewModel.Loot.LootUpdated.Subscribe(delegate
		{
			m_ScrollRect.ScrollToTop();
		}));
		m_ScrollRect.ScrollToTop();
		m_ToCargoText.text = UIStrings.Instance.LootWindow.TrashLootObject;
		m_ToInventoryText.text = UIStrings.Instance.LootWindow.ItemsLootObject;
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected virtual void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ChangeView()
	{
		base.ViewModel.ChangeView();
	}

	public virtual void AddAllToCargo()
	{
		base.ViewModel.AddAllToCargoPart();
	}

	public virtual void AddAllToInventory()
	{
		base.ViewModel.AddAllToInventoryPart();
	}

	public void CollectAll()
	{
		base.ViewModel.CollectAll();
	}
}
