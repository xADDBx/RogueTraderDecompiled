using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;

public class FeaturesFilterBaseView : ViewBase<FeaturesFilterVM>
{
	[Serializable]
	private struct FilterView
	{
		public OwlcatToggle Toggle;

		public Image Icon;
	}

	[Header("Toggles")]
	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[SerializeField]
	private FilterView m_None;

	[SerializeField]
	private FilterView m_RecommendedFilter;

	[SerializeField]
	private FilterView m_ChoosedFilter;

	[SerializeField]
	private FilterView m_OffenseFilter;

	[SerializeField]
	private FilterView m_DefenseFilter;

	[SerializeField]
	private FilterView m_SupportFilter;

	[SerializeField]
	private FilterView m_UniversalFilter;

	[SerializeField]
	private FilterView m_ArchetypeFilter;

	[SerializeField]
	private FilterView m_OriginFilter;

	[SerializeField]
	private FilterView m_WarpFilter;

	[SerializeField]
	private TextMeshProUGUI m_FilterHint;

	private Dictionary<FeaturesFilter.FeatureFilterType, FilterView> m_FiltersMap = new Dictionary<FeaturesFilter.FeatureFilterType, FilterView>();

	private Dictionary<FeaturesFilter.FeatureFilterType, LocalizedString> m_FiltersNames = new Dictionary<FeaturesFilter.FeatureFilterType, LocalizedString>();

	private AccessibilityTextHelper m_TextHelper;

	public virtual void Initialize()
	{
		Hide();
		m_TextHelper = new AccessibilityTextHelper(m_FilterHint);
		m_FiltersMap = new Dictionary<FeaturesFilter.FeatureFilterType, FilterView>
		{
			{
				FeaturesFilter.FeatureFilterType.None,
				m_None
			},
			{
				FeaturesFilter.FeatureFilterType.RecommendedFilter,
				m_RecommendedFilter
			},
			{
				FeaturesFilter.FeatureFilterType.FavoritesFilter,
				m_ChoosedFilter
			},
			{
				FeaturesFilter.FeatureFilterType.OffenseFilter,
				m_OffenseFilter
			},
			{
				FeaturesFilter.FeatureFilterType.DefenseFilter,
				m_DefenseFilter
			},
			{
				FeaturesFilter.FeatureFilterType.SupportFilter,
				m_SupportFilter
			},
			{
				FeaturesFilter.FeatureFilterType.UniversalFilter,
				m_UniversalFilter
			},
			{
				FeaturesFilter.FeatureFilterType.ArchetypeFilter,
				m_ArchetypeFilter
			},
			{
				FeaturesFilter.FeatureFilterType.OriginFilter,
				m_OriginFilter
			},
			{
				FeaturesFilter.FeatureFilterType.WarpFilter,
				m_WarpFilter
			}
		};
		m_FiltersNames = new Dictionary<FeaturesFilter.FeatureFilterType, LocalizedString>
		{
			{
				FeaturesFilter.FeatureFilterType.None,
				UIStrings.Instance.CharacterSheet.NoneHint
			},
			{
				FeaturesFilter.FeatureFilterType.RecommendedFilter,
				UIStrings.Instance.CharacterSheet.RecommendedFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.FavoritesFilter,
				UIStrings.Instance.CharacterSheet.FavoritesFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.OffenseFilter,
				UIStrings.Instance.CharacterSheet.OffenseFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.DefenseFilter,
				UIStrings.Instance.CharacterSheet.DefenseFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.SupportFilter,
				UIStrings.Instance.CharacterSheet.SupportFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.UniversalFilter,
				UIStrings.Instance.CharacterSheet.UniversalFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.ArchetypeFilter,
				UIStrings.Instance.CharacterSheet.ArchetypeFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.OriginFilter,
				UIStrings.Instance.CharacterSheet.OriginFilterHint
			},
			{
				FeaturesFilter.FeatureFilterType.WarpFilter,
				UIStrings.Instance.CharacterSheet.WarpFilterHint
			}
		};
		SetupIcons();
	}

	private void SetupIcons()
	{
		foreach (KeyValuePair<FeaturesFilter.FeatureFilterType, FilterView> item in m_FiltersMap)
		{
			item.Deconstruct(out var key, out var value);
			FeaturesFilter.FeatureFilterType filter = key;
			value.Icon.sprite = UIConfig.Instance.FiltersIcons.GetIconFor(filter);
		}
	}

	protected override void BindViewImplementation()
	{
		Show();
		SetHints();
		AddDisposable(m_FiltersToggleGroup.ActiveToggle.Subscribe(HandleFilterToggle));
		KeyValuePair<FeaturesFilter.FeatureFilterType, FilterView> keyValuePair = m_FiltersMap.FirstOrDefault((KeyValuePair<FeaturesFilter.FeatureFilterType, FilterView> f) => f.Key == FeaturesFilterVM.ThisSessionFilter);
		if (keyValuePair.Value.Toggle != null)
		{
			keyValuePair.Value.Toggle.Set(value: true);
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_TextHelper.Dispose();
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_FiltersToggleGroup.GetNavigationBehaviour();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void HandleFilterToggle(OwlcatToggle activeToggle)
	{
		if (activeToggle == null)
		{
			return;
		}
		foreach (KeyValuePair<FeaturesFilter.FeatureFilterType, FilterView> item in m_FiltersMap)
		{
			item.Deconstruct(out var key, out var value);
			FeaturesFilter.FeatureFilterType featureFilterType = key;
			if (!(value.Toggle != activeToggle))
			{
				base.ViewModel.SetCurrentFilter(featureFilterType);
				m_FilterHint.text = m_FiltersNames[featureFilterType];
				break;
			}
		}
	}

	private void SetHints()
	{
		foreach (var (key, localizedString2) in m_FiltersNames)
		{
			if (m_FiltersMap.TryGetValue(key, out var value))
			{
				AddDisposable(value.Toggle.SetHint(localizedString2));
			}
		}
	}
}
