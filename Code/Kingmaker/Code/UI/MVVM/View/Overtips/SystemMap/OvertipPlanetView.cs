using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common;
using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;

public abstract class OvertipPlanetView : BaseOvertipView<OvertipEntityPlanetVM>, IChangePlanetTypeHandler, ISubscriber<PlanetEntity>, ISubscriber, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, ISystemMapOvertip
{
	[SerializeField]
	protected OwlcatButton m_PlanetButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_PlanetNameLabel;

	[SerializeField]
	private Image m_UnknownPlanetNameImage;

	[Header("States Icons")]
	[SerializeField]
	private RectTransform m_StateIconCanvas;

	[SerializeField]
	private Image m_ColonizeStateIcon;

	[SerializeField]
	private Image m_QuestStateIcon;

	[SerializeField]
	private Image m_CrownIcon;

	[SerializeField]
	private Image m_ResourceStateIcon;

	[SerializeField]
	private Image m_ExtractorStateIcon;

	[SerializeField]
	private Image m_PoiStateIcon;

	[SerializeField]
	private Image m_RumourStateIcon;

	[SerializeField]
	private Image m_NotExploredStateIcon;

	[Header("Overtip Size Control")]
	[SerializeField]
	private RectTransform m_CanvasRectTransform;

	[Header("Resources Hint Block")]
	[SerializeField]
	private OwlcatSelectable m_ResourceIconSelectable;

	[SerializeField]
	private FadeAnimator m_ResourceCustomHint;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private TextMeshProUGUI m_ResourcesTitle;

	[SerializeField]
	private SystemMapPlanetResourcesPCView m_SystemMapPlanetResourcesPCViewPrefab;

	[Header("Extractor Hint Block")]
	[SerializeField]
	private OwlcatSelectable m_ExtractorIconSelectable;

	[SerializeField]
	private FadeAnimator m_ExtractorCustomHint;

	[SerializeField]
	private WidgetListMVVM m_ExtractorWidgetList;

	[SerializeField]
	private TextMeshProUGUI m_ExtractorResourcesTitle;

	[Header("Other")]
	[SerializeField]
	protected Image m_TooltipTaker;

	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	[SerializeField]
	private List<Image> m_AdditionalTargetPingImages;

	[Header("Noise")]
	[SerializeField]
	private Image m_NoiseAroundImage;

	[SerializeField]
	private Sprite[] m_RandomNoises;

	[Header("Bottom Image")]
	[SerializeField]
	private Image m_BottomImage;

	[SerializeField]
	private Sprite[] m_RandomBottomSprites;

	[Header("Top Information")]
	[SerializeField]
	private TextMeshProUGUI m_TopInformation;

	private SolarSystemStellarBodyVisual m_PlanetOrbitsVisual;

	private readonly string[] m_ObjectLatinLetters = new string[24]
	{
		"ARI", "CIN", "BAR", "BOR", "ARD", "TEM", "FRO", "GLA", "PRA", "IGN",
		"MNL", "LAP", "AEC", "SAX", "ERE", "DIA", "NIX", "EGR", "TOX", "SIL",
		"TUN", "FOD", "MOR", "RIM"
	};

	protected MapObjectView SystemObject;

	private IDisposable m_PingDelay;

	private bool m_Visible;

	protected override bool CheckVisibility => m_Visible;

	public void Initialize()
	{
		m_ResourceCustomHint.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_TargetPingEntity.Or(null)?.CanvasGroup != null)
		{
			m_TargetPingEntity.CanvasGroup.alpha = 0f;
		}
		m_TargetPingEntity.Or(null)?.DisappearAnimation();
		AddDisposable(ObservableExtensions.Subscribe(m_PlanetButton.Or(null)?.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.RequestVisit();
		}));
		SystemObject = base.ViewModel.PlanetObject?.View;
		m_Visible = base.ViewModel.PlanetIsVisible.Value;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.PlanetIsVisible.Subscribe(delegate(bool value)
		{
			UpdateVisibility(value);
		}));
		AddDisposable(base.ViewModel.PlanetIsScanned.Subscribe(delegate
		{
			SetPlanetName();
		}));
		m_ResourcesTitle.text = UIStrings.Instance.ExplorationTexts.DiscoveredResources;
		m_ExtractorResourcesTitle.text = UIStrings.Instance.ExplorationTexts.ResourceMining;
		SetPlanetIconStates();
		SetOvertipSize();
		AddDisposable(m_PlanetButton.Or(null)?.OnHoverAsObservable().Subscribe(SwitchPlanetOrbits));
		AddDisposable(base.ViewModel.CoopPingEntity.Subscribe(delegate((NetPlayer player, Entity entity) value)
		{
			PingEntity(value.player, value.entity);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_PlanetOrbitsVisual = null;
		m_PingDelay?.Dispose();
		m_PingDelay = null;
	}

	private void SwitchPlanetOrbits(bool state)
	{
		if (m_PlanetOrbitsVisual == null)
		{
			if (SystemObject.Or(null)?.transform.parent != null && SystemObject.Or(null)?.transform.parent.transform.parent != null)
			{
				m_PlanetOrbitsVisual = SystemObject.Or(null)?.transform.parent.transform.parent.GetComponent<SolarSystemStellarBodyVisual>();
			}
			if (m_PlanetOrbitsVisual == null)
			{
				return;
			}
		}
		SolarSystemStellarBodyVisual solarSystemStellarBodyVisual = m_PlanetOrbitsVisual.Or(null);
		if ((object)solarSystemStellarBodyVisual != null)
		{
			solarSystemStellarBodyVisual.SelectorMarkerRing.Or(null)?.gameObject.SetActive(!state);
		}
		SolarSystemStellarBodyVisual solarSystemStellarBodyVisual2 = m_PlanetOrbitsVisual.Or(null);
		if ((object)solarSystemStellarBodyVisual2 != null && solarSystemStellarBodyVisual2.SecondaryOrbits?.Count > 0)
		{
			m_PlanetOrbitsVisual.Or(null)?.SecondaryOrbits?.ForEach(delegate(LineRenderer o)
			{
				o.gameObject.SetActive(!state);
			});
		}
		SolarSystemStellarBodyVisual solarSystemStellarBodyVisual3 = m_PlanetOrbitsVisual.Or(null);
		Material material = (((object)solarSystemStellarBodyVisual3 == null) ? null : solarSystemStellarBodyVisual3.Visual.Or(null)?.GetComponentInChildren<MeshRenderer>()?.material);
		if (material != null)
		{
			material.SetInt("_Rim_light", state ? 1 : 0);
		}
	}

	void IChangePlanetTypeHandler.HandleChangePlanetType()
	{
		if (EventInvokerExtensions.Entity == SystemObject.Or(null)?.Data)
		{
			SetPlanetName();
		}
	}

	public void SetFocus(bool value)
	{
		m_PlanetButton.Or(null)?.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.isActiveAndEnabled)
		{
			return base.CanvasGroup.alpha > 0f;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public void UnfocusButton()
	{
		m_PlanetButton.Or(null)?.OnPointerExit();
	}

	private void UpdateVisibility(bool isVisible)
	{
		m_Visible = isVisible;
	}

	public void SetPlanetName()
	{
		bool value = base.ViewModel.PlanetIsScanned.Value;
		m_PlanetNameLabel.Or(null)?.gameObject.SetActive(value);
		m_UnknownPlanetNameImage.Or(null)?.gameObject.SetActive(!value);
		if (!value)
		{
			m_UnknownPlanetNameImage.sprite = UIConfig.Instance.UIIcons.TooltipIcons.UnknownPlanet;
			m_UnknownPlanetNameImage.color = UIConfig.Instance.OvertipSystemObjectColors.GetColorByState(GetState());
		}
		else
		{
			m_PlanetNameLabel.text = base.ViewModel.PlanetName.Value;
			m_PlanetNameLabel.color = UIConfig.Instance.OvertipSystemObjectColors.GetColorByState(GetState());
		}
	}

	private OvertipSystemObjectState GetState()
	{
		PlanetView value = base.ViewModel.PlanetView.Value;
		bool flag = value.Data.IsFullyExplored && (value.Data.IsScanned ^ value.Data.Blueprint.IsScannedOnStart);
		if (value.Data.Blueprint.IsQuestObject)
		{
			if (!flag)
			{
				return OvertipSystemObjectState.QuestActive;
			}
			return OvertipSystemObjectState.QuestInactive;
		}
		if (!flag)
		{
			return OvertipSystemObjectState.DefaultActive;
		}
		return OvertipSystemObjectState.DefaultInactive;
	}

	private void SetOvertipSize()
	{
		m_NoiseAroundImage.sprite = m_RandomNoises[UnityEngine.Random.Range(0, m_RandomNoises.Length)];
		m_BottomImage.sprite = m_RandomBottomSprites[UnityEngine.Random.Range(0, m_RandomBottomSprites.Length)];
		m_TopInformation.text = GetTopInformationText();
		float num = UnityEngine.Random.Range(15f, 25f);
		m_TopInformation.transform.parent.transform.rotation *= Quaternion.Euler(0f, 0f, num);
		m_TopInformation.transform.rotation *= Quaternion.Euler(0f, 0f, 0f - num);
		SphereCollider sphereCollider = SystemObject.Or(null)?.gameObject.GetComponent<SphereCollider>();
		if (!(sphereCollider == null))
		{
			Vector3 size = sphereCollider.bounds.size;
			m_CanvasRectTransform.sizeDelta = new Vector2(size.x * 12f, size.y * 12f);
		}
	}

	private string GetTopInformationText()
	{
		string text = "UNKNOWN";
		if (Game.Instance.CurrentMode != GameModeType.StarSystem || !(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap))
		{
			return text;
		}
		string text2 = ((!(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap blueprintStarSystemMap)) ? text : UIUtility.ArabicToRoman(blueprintStarSystemMap.RomeSystemNumber));
		int num = Array.IndexOf(Enum.GetValues(typeof(BlueprintPlanet.PlanetType)), base.ViewModel.PlanetView.Value.Data.Blueprint.Type);
		string text3 = ((num < 0 || num >= m_ObjectLatinLetters.Length) ? text : m_ObjectLatinLetters[num]);
		string text4 = UnityEngine.Random.Range(100, 1000).ToString();
		return "Notitium collectionis:\n// RES //\n" + text2 + "-" + text3 + "-" + text4;
	}

	private void DrawResources()
	{
		m_WidgetList.Clear();
		TooltipBrickResourceInfoVM[] vmCollection = base.ViewModel.Resources.ToArray();
		m_WidgetList.DrawEntries(vmCollection, m_SystemMapPlanetResourcesPCViewPrefab);
	}

	private void DrawExtractorResources()
	{
		m_ExtractorWidgetList.Clear();
		TooltipBrickResourceInfoVM[] vmCollection = base.ViewModel.ExtractorResources.ToArray();
		m_ExtractorWidgetList.DrawEntries(vmCollection, m_SystemMapPlanetResourcesPCViewPrefab);
	}

	private void SetPlanetIconStates()
	{
		AddDisposable(base.ViewModel.HasColony.CombineLatest(base.ViewModel.HasQuest, base.ViewModel.HasRumour, base.ViewModel.HasResource, base.ViewModel.HasExtractor, base.ViewModel.HasPoi, (bool isColonized, bool isQuest, bool isRumour, bool isResource, bool isExtractorResource, bool isPoi) => isColonized || isQuest || isRumour || isResource || isExtractorResource || isPoi).Subscribe(m_StateIconCanvas.gameObject.SetActive));
		AddDisposable(base.ViewModel.HasColony.Subscribe(m_ColonizeStateIcon.gameObject.SetActive));
		AddDisposable(base.ViewModel.UpdateColonizeHint.Subscribe(delegate
		{
			if (base.ViewModel.HasColony.Value)
			{
				AddDisposable(m_ColonizeStateIcon.SetHint(UIUtilitySpaceColonization.GetColonizationInformation(base.ViewModel.Colony.Value)));
			}
		}));
		AddDisposable(base.ViewModel.HasQuest.CombineLatest(base.ViewModel.QuestObjectiveName, (bool isQuest, string questObjectiveName) => new { isQuest, questObjectiveName }).Subscribe(value =>
		{
			m_QuestStateIcon.gameObject.SetActive(value.isQuest);
			if (value.isQuest && value.questObjectiveName != null)
			{
				AddDisposable(m_QuestStateIcon.SetHint(base.ViewModel.QuestObjectiveName.Value));
			}
		}));
		AddDisposable(base.ViewModel.HasRumour.CombineLatest(base.ViewModel.RumourObjectiveName, (bool isRumour, string rumourObjectiveName) => new { isRumour, rumourObjectiveName }).Subscribe(value =>
		{
			m_RumourStateIcon.gameObject.SetActive(value.isRumour);
			if (value.isRumour && value.rumourObjectiveName != null)
			{
				AddDisposable(m_RumourStateIcon.SetHint(base.ViewModel.RumourObjectiveName.Value));
			}
		}));
		AddDisposable(base.ViewModel.HasResource.Subscribe(delegate(bool state)
		{
			m_ResourceStateIcon.gameObject.SetActive(state);
			if (state)
			{
				DrawResources();
				AddDisposable(m_ResourceIconSelectable.OnHoverAsObservable().Subscribe(delegate(bool value)
				{
					if (value)
					{
						m_ResourceCustomHint.AppearAnimation();
					}
					else
					{
						m_ResourceCustomHint.DisappearAnimation();
					}
				}));
			}
		}));
		AddDisposable(base.ViewModel.HasExtractor.Subscribe(delegate(bool state)
		{
			m_ExtractorStateIcon.gameObject.SetActive(state);
			if (state)
			{
				DrawExtractorResources();
				AddDisposable(m_ExtractorIconSelectable.OnHoverAsObservable().Subscribe(delegate(bool value)
				{
					if (value)
					{
						m_ExtractorCustomHint.AppearAnimation();
					}
					else
					{
						m_ExtractorCustomHint.DisappearAnimation();
					}
				}));
			}
		}));
		AddDisposable(base.ViewModel.HasPoi.Subscribe(delegate(bool state)
		{
			m_PoiStateIcon.gameObject.SetActive(state);
			if (state)
			{
				string text = string.Join(Environment.NewLine, base.ViewModel.PoiNamesList);
				AddDisposable(m_PoiStateIcon.SetHint(string.Concat(UIStrings.Instance.SystemMap.PoIsDetected, ":\n", text)));
			}
		}));
		AddDisposable(base.ViewModel.Colony.Subscribe(delegate(Colony colony)
		{
			bool active = colony?.ColonyTraits.Any((KeyValuePair<BlueprintColonyTrait, TimeSpan> t) => t.Key.name == "DLC2_DargonusTithe_Trait") ?? false;
			m_CrownIcon.gameObject.SetActive(active);
		}));
		AddDisposable(base.ViewModel.Colony.Subscribe(delegate
		{
			UpdateCrownIcon();
		}));
		AddDisposable(base.ViewModel.UpdateColonizeHint.Subscribe(delegate
		{
			UpdateCrownIcon();
		}));
	}

	private void PingEntity(NetPlayer player, Entity entity)
	{
		m_PingDelay?.Dispose();
		if (entity != base.ViewModel.PlanetObject)
		{
			m_TargetPingEntity.DisappearAnimation();
			return;
		}
		int index = player.Index - 1;
		Image component = m_TargetPingEntity.GetComponent<Image>();
		Color currentColor = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
		if (component != null)
		{
			component.color = currentColor;
		}
		if (m_AdditionalTargetPingImages != null && m_AdditionalTargetPingImages.Any())
		{
			m_AdditionalTargetPingImages.ForEach(delegate(Image i)
			{
				i.color = currentColor;
			});
		}
		m_TargetPingEntity.AppearAnimation();
		m_PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingEntity.DisappearAnimation();
		}, 7.5f);
	}

	private void UpdateCrownIcon()
	{
		bool active = base.ViewModel.Colony.Value?.ColonyTraits.Any((KeyValuePair<BlueprintColonyTrait, TimeSpan> t) => t.Key.name == "DLC2_DargonusTithe_Trait") ?? false;
		m_CrownIcon.gameObject.SetActive(active);
	}
}
