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

	[Header("Color")]
	[SerializeField]
	protected TextureSelectorPagedView m_OutfitMainColorSelectorView;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		m_OutfitMainColorSelectorView.Initialize();
		m_ClothEntityView.Or(null)?.Initialize(characterSheet.VisualSettingsShowCloth);
		m_HelmetEntityView.Initialize(characterSheet.VisualSettingsShowHelmet);
		m_BackpackEntityView.Initialize(characterSheet.VisualSettingsShowBackpack);
	}

	protected override void BindViewImplementation()
	{
		m_Header.text = UIStrings.Instance.CharacterSheet.VisualSettingsTitle;
		m_FadeAnimator.AppearAnimation();
		m_OutfitMainColorSelectorView.Bind(base.ViewModel.OutfitMainColorSelector);
		m_ClothEntityView.Or(null)?.Bind(base.ViewModel.Cloth);
		m_HelmetEntityView.Bind(base.ViewModel.Helmet);
		m_BackpackEntityView.Bind(base.ViewModel.Backpack);
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
	}
}
