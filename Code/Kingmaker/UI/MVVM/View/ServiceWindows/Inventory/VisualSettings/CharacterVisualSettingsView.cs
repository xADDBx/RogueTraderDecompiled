using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;

public class CharacterVisualSettingsView<TBoolEntity> : ViewBase<CharacterVisualSettingsVM>, IInitializable where TBoolEntity : CharacterVisualSettingsEntityView
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_Header;

	[Header("Main")]
	[SerializeField]
	protected TBoolEntity m_ClothEntityView;

	[SerializeField]
	protected TBoolEntity m_HelmetEntityView;

	[SerializeField]
	protected TBoolEntity m_BackpackEntityView;

	[SerializeField]
	protected TBoolEntity m_HelmetAboveAllEntityView;

	[SerializeField]
	protected TBoolEntity m_GlovesEntityView;

	[SerializeField]
	protected TBoolEntity m_BootsEntityView;

	[SerializeField]
	protected TBoolEntity m_ArmorEntityView;

	[Header("Color")]
	[SerializeField]
	protected TextureSelectorPagedView m_OutfitMainColorSelectorView;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		m_OutfitMainColorSelectorView.Initialize();
		m_ClothEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowCloth);
		m_HelmetEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowHelmet);
		m_BackpackEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowBackpack);
		m_HelmetAboveAllEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowHelmetAboveAll);
		m_GlovesEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowGloves);
		m_BootsEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowBoots);
		m_ArmorEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowArmor);
	}

	protected override void BindViewImplementation()
	{
		m_Header.text = UIStrings.Instance.CharacterSheet.VisualSettingsTitle;
		m_FadeAnimator.AppearAnimation();
		m_OutfitMainColorSelectorView.Bind(base.ViewModel.OutfitMainColorSelector);
		if (!base.ViewModel.IsPet)
		{
			m_ClothEntityView.Or(null)?.Bind(base.ViewModel.Cloth);
			m_HelmetEntityView.Or(null)?.Bind(base.ViewModel.Helmet);
			m_BackpackEntityView.Or(null)?.Bind(base.ViewModel.Backpack);
			m_HelmetAboveAllEntityView.Or(null)?.Bind(base.ViewModel.HelmetAboveAll);
			m_GlovesEntityView.Or(null)?.Bind(base.ViewModel.Gloves);
			m_BootsEntityView.Or(null)?.Bind(base.ViewModel.Boots);
			m_ArmorEntityView.Or(null)?.Bind(base.ViewModel.Armor);
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
	}
}
