using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

public static class TooltipHelper
{
	private static readonly LinkedList<BlueprintEncyclopediaGlossaryEntry> History = new LinkedList<BlueprintEncyclopediaGlossaryEntry>();

	public static LinkedListNode<BlueprintEncyclopediaGlossaryEntry> HistoryPointer { get; private set; }

	public static TooltipHandler SetTooltip(this MonoBehaviour component, TooltipBaseTemplate template, TooltipConfig config = default(TooltipConfig))
	{
		if (template == null)
		{
			return null;
		}
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		using (ProfileScope.New("TooltipHandler ctor"))
		{
			return new TooltipHandler(component, template, config);
		}
	}

	public static TooltipHandler SetTooltip(this MonoBehaviour component, List<TooltipBaseTemplate> templates, TooltipConfig config = default(TooltipConfig))
	{
		if (templates.Empty())
		{
			return null;
		}
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		List<TooltipData> list = new List<TooltipData>();
		foreach (TooltipBaseTemplate template in templates)
		{
			if (template != null)
			{
				list.Add(new TooltipData(template, config));
			}
		}
		return new TooltipHandler(component, list, config);
	}

	public static TooltipHandler SetTooltip(this MonoBehaviour component, List<TooltipBaseTemplate> templates, TooltipConfig mainConfig, TooltipConfig compareConfig)
	{
		if (templates.Empty())
		{
			return null;
		}
		if (mainConfig.TooltipPlace == null)
		{
			mainConfig.TooltipPlace = component.transform as RectTransform;
		}
		if (compareConfig.TooltipPlace == null)
		{
			compareConfig.TooltipPlace = component.transform as RectTransform;
		}
		List<TooltipData> list = new List<TooltipData>();
		for (int i = 0; i < templates.Count - 1; i++)
		{
			TooltipBaseTemplate tooltipBaseTemplate = templates[i];
			if (tooltipBaseTemplate != null)
			{
				list.Add(new TooltipData(tooltipBaseTemplate, compareConfig));
			}
		}
		list.Add(new TooltipData(templates.LastOrDefault(), mainConfig));
		return new TooltipHandler(component, list, mainConfig);
	}

	public static void EnsureTooltipPlace(MonoBehaviour component, TooltipConfig config)
	{
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
	}

	public static IDisposable SetTooltip(this MonoBehaviour component, IReadOnlyReactiveProperty<List<TooltipBaseTemplate>> reactiveTemplates, TooltipConfig config = default(TooltipConfig))
	{
		IDisposable tooltipSubscription = null;
		IDisposable templateSubscription = reactiveTemplates.Subscribe(delegate(List<TooltipBaseTemplate> templates)
		{
			tooltipSubscription?.Dispose();
			if (component != null)
			{
				tooltipSubscription = component.SetTooltip(templates, config);
			}
		});
		return Disposable.Create(delegate
		{
			tooltipSubscription?.Dispose();
			templateSubscription?.Dispose();
		});
	}

	public static IDisposable SetTooltip(this MonoBehaviour component, IReadOnlyReactiveProperty<List<TooltipBaseTemplate>> reactiveTemplates, TooltipConfig mainConfig, TooltipConfig compareConfig)
	{
		IDisposable tooltipSubscription = null;
		IDisposable templateSubscription = reactiveTemplates.Subscribe(delegate(List<TooltipBaseTemplate> templates)
		{
			tooltipSubscription?.Dispose();
			if (component != null)
			{
				tooltipSubscription = component.SetTooltip(templates, mainConfig, compareConfig);
			}
		});
		return Disposable.Create(delegate
		{
			tooltipSubscription?.Dispose();
			templateSubscription?.Dispose();
		});
	}

	public static IDisposable SetTooltip(this MonoBehaviour component, IReadOnlyReactiveProperty<TooltipBaseTemplate> reactiveTemplate, TooltipConfig config = default(TooltipConfig))
	{
		IDisposable tooltipSubscription = null;
		IDisposable templateSubscription = reactiveTemplate.Subscribe(delegate(TooltipBaseTemplate template)
		{
			tooltipSubscription?.Dispose();
			tooltipSubscription = component.SetTooltip(template, config);
		});
		return Disposable.Create(delegate
		{
			tooltipSubscription?.Dispose();
			templateSubscription?.Dispose();
		});
	}

	public static IDisposable SetGlossaryTooltip(this MonoBehaviour component, string key, TooltipConfig config = default(TooltipConfig))
	{
		return component.SetTooltip(new TooltipTemplateGlossary(key, config.IsGlossary), config);
	}

	public static void ShowTooltip(this MonoBehaviour component, TooltipBaseTemplate template, TooltipConfig config = default(TooltipConfig), ReactiveCommand closeCommand = null, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false)
	{
		if (template == null)
		{
			HideTooltip();
			return;
		}
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		TooltipData tooltipData = new TooltipData(template, config, closeCommand, ownerNavigationBehaviour);
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleTooltipRequest(tooltipData, shouldNotHideLittleTooltip);
		});
	}

	public static void ShowConsoleTooltip(this MonoBehaviour component, TooltipBaseTemplate template, ConsoleNavigationBehaviour navigationBehaviour, TooltipConfig config = default(TooltipConfig), bool shouldNotHideLittleTooltip = false, bool showScrollbar = false)
	{
		if (template == null)
		{
			HideTooltip();
			return;
		}
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		TooltipData tooltipData = new TooltipData(template, config, null, navigationBehaviour);
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleTooltipRequest(tooltipData, shouldNotHideLittleTooltip, showScrollbar);
		});
	}

	public static void ShowConsoleTooltip(this MonoBehaviour component, List<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour navigationBehaviour, TooltipConfig config = default(TooltipConfig))
	{
		if (templates == null || templates.Count == 0)
		{
			HideTooltip();
			return;
		}
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		CombinedTooltipData tooltipData = new CombinedTooltipData(templates, config, null, navigationBehaviour);
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleTooltipRequest(tooltipData);
		});
	}

	public static void ShowComparativeTooltip(this MonoBehaviour component, List<TooltipBaseTemplate> templates, TooltipConfig mainConfig, TooltipConfig comparativeConfig, bool showScrollbar)
	{
		if (templates.Empty())
		{
			HideTooltip();
			return;
		}
		if (mainConfig.TooltipPlace == null)
		{
			mainConfig.TooltipPlace = component.transform as RectTransform;
		}
		if (comparativeConfig.TooltipPlace == null)
		{
			comparativeConfig.TooltipPlace = component.transform as RectTransform;
		}
		TooltipBaseTemplate lastTemplate = templates.LastOrDefault();
		List<TooltipData> tooltipData = (from t in templates
			where t != null
			select new TooltipData(t, (t == lastTemplate) ? mainConfig : comparativeConfig)).ToList();
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleComparativeTooltipRequest(tooltipData, showScrollbar);
		});
	}

	public static void HideTooltip()
	{
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleTooltipRequest(null);
		});
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleComparativeTooltipRequest(null);
		});
	}

	public static void ShowInfo(TooltipBaseTemplate template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false)
	{
		HideTooltip();
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleInfoRequest(template, ownerNavigationBehaviour, shouldNotHideLittleTooltip);
		});
	}

	public static void ShowInfo(IEnumerable<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		HideTooltip();
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleMultipleInfoRequest(templates, ownerNavigationBehaviour);
		});
	}

	public static void ShowGlossaryInfo(TooltipTemplateGlossary template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		HideTooltip();
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleGlossaryInfoRequest(template, ownerNavigationBehaviour);
		});
	}

	public static void HideInfo()
	{
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleInfoRequest(null);
		});
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleGlossaryInfoRequest(null);
		});
	}

	public static void ShowLinkTooltip(this MonoBehaviour component, string[] keys, List<SkillCheckDC> skillCheckDcs = null, List<SkillCheckResult> skillCheckResults = null, TooltipConfig config = default(TooltipConfig))
	{
		TooltipBaseTemplate linkTooltipTemplate = GetLinkTooltipTemplate(keys, skillCheckDcs, skillCheckResults);
		component.ShowTooltip(linkTooltipTemplate, config);
	}

	public static void ShowLinkTooltip(this MonoBehaviour component, string key, List<SkillCheckDC> skillCheckDcs = null, List<SkillCheckResult> skillCheckResults = null, TooltipConfig config = default(TooltipConfig))
	{
		component.ShowLinkTooltip(UIUtility.GetKeysFromLink(key), skillCheckDcs, skillCheckResults, config);
	}

	public static IDisposable SetLinkTooltip(this TextMeshProUGUI text, List<SkillCheckDC> skillCheckDcs = null, List<SkillCheckResult> skillCheckResults = null, TooltipConfig config = default(TooltipConfig), MechanicEntity mechanicEntity = null)
	{
		if (text == null)
		{
			return Disposable.Empty;
		}
		bool entered = TMP_TextUtilities.IsIntersectingRectTransform(text.rectTransform, Input.mousePosition, UICamera.Claim());
		int? linkIndex = null;
		string[] linkKeys = null;
		IDisposable enter = text.OnPointerEnterAsObservable().Subscribe(delegate
		{
			entered = true;
		});
		IDisposable exit = text.OnPointerExitAsObservable().Subscribe(delegate
		{
			entered = false;
		});
		IDisposable update = ObservableExtensions.Subscribe(text.UpdateAsObservable(), delegate
		{
			int num2;
			if (!entered || (num2 = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, UICamera.Claim())) == -1)
			{
				if (linkIndex.HasValue)
				{
					EventBus.RaiseEvent(delegate(ITooltipHandler h)
					{
						h.HandleTooltipRequest(null);
					});
					linkIndex = null;
					linkKeys = null;
				}
			}
			else if (num2 != linkIndex)
			{
				linkIndex = num2;
				linkKeys = UIUtility.GetKeysFromLink(text.textInfo.linkInfo[linkIndex.Value].GetLinkID());
				TooltipBaseTemplate template = GetLinkTooltipTemplate(linkKeys, skillCheckDcs, skillCheckResults, config.IsEncyclopedia, mechanicEntity);
				if (template != null)
				{
					ref RectTransform tooltipPlace = ref config.TooltipPlace;
					if ((object)tooltipPlace == null)
					{
						tooltipPlace = text.transform as RectTransform;
					}
					EventBus.RaiseEvent(delegate(ITooltipHandler h)
					{
						h.HandleTooltipRequest(new TooltipData(template, config));
					});
				}
			}
		});
		IDisposable click = null;
		bool needInfo = config.InfoCallPCMethod != InfoCallPCMethod.None;
		bool isEncyclopedia = config.IsEncyclopedia;
		bool isGlossary = config.IsGlossary;
		if (needInfo || isEncyclopedia || isGlossary)
		{
			click = text.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
			{
				if (linkIndex.HasValue)
				{
					bool num = data.button == PointerEventData.InputButton.Left && isEncyclopedia;
					bool flag = data.button == PointerEventData.InputButton.Right && needInfo && !isEncyclopedia;
					if (num)
					{
						if (linkKeys != null && linkKeys.Length == 2 && EntityLink.GetEntityType(linkKeys[0]) == EntityLink.Type.Encyclopedia)
						{
							EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
							{
								x.HandleEncyclopediaPage(linkKeys[1]);
							});
							EventBus.RaiseEvent(delegate(IInfoWindowHandler h)
							{
								h.HandleCloseTooltipInfoWindow();
							});
							HideTooltip();
						}
					}
					else if (flag)
					{
						TooltipBaseTemplate linkTooltipTemplate = GetLinkTooltipTemplate(linkKeys, skillCheckDcs, skillCheckResults);
						if (linkTooltipTemplate is TooltipTemplateGlossary template)
						{
							ShowGlossaryInfo(template);
						}
						else
						{
							ShowInfo(linkTooltipTemplate);
						}
					}
				}
			});
		}
		return Disposable.Create(delegate
		{
			if (entered)
			{
				entered = false;
				EventBus.RaiseEvent(delegate(ITooltipHandler h)
				{
					h.HandleTooltipRequest(null);
				});
			}
			enter?.Dispose();
			exit?.Dispose();
			update?.Dispose();
			click?.Dispose();
		});
	}

	public static TooltipBaseTemplate GetLinkTooltipTemplate(string key, List<SkillCheckResult> skillCheckResults)
	{
		return GetLinkTooltipTemplate(UIUtility.GetKeysFromLink(key), null, skillCheckResults);
	}

	public static TooltipBaseTemplate GetLinkTooltipTemplate(string key, List<SkillCheckDC> skillCheckDcs)
	{
		return GetLinkTooltipTemplate(UIUtility.GetKeysFromLink(key), skillCheckDcs);
	}

	public static TooltipBaseTemplate GetLinkTooltipTemplate(string key)
	{
		return GetLinkTooltipTemplate(UIUtility.GetKeysFromLink(key));
	}

	private static TooltipBaseTemplate GetLinkTooltipTemplate(string[] keys, List<SkillCheckDC> skillCheckDcs = null, List<SkillCheckResult> skillCheckResults = null, bool isEncyclopedia = false, MechanicEntity mechanicEntity = null)
	{
		if (keys.Empty())
		{
			return null;
		}
		switch (EntityLink.GetEntityType(keys[0]))
		{
		case EntityLink.Type.Unit:
			return new TooltipTemplateUnitInspect(LinksHelper.GetUnit(keys[1]));
		case EntityLink.Type.Item:
			return new TooltipTemplateItem(LinksHelper.GetItem(keys[1]));
		case EntityLink.Type.ItemBlueprint:
			return new TooltipTemplateItem(LinksHelper.GetBlueprintItem(keys[1]));
		case EntityLink.Type.CargoBlueprint:
			return new TooltipTemplateCargo(LinksHelper.GetBlueprintCargo(keys[1]));
		case EntityLink.Type.SoulMarkShiftDirection:
		{
			if (!Enum.TryParse<SoulMarkDirection>(keys[1], out var result))
			{
				return null;
			}
			return new TooltipTemplateSoulMarkHeader(Game.Instance.Player.MainCharacterEntity, result);
		}
		case EntityLink.Type.UnitFact:
		{
			BlueprintUnitFact unitFact = LinksHelper.GetUnitFact(keys[1]);
			if (!(unitFact is BlueprintFeature feature))
			{
				if (!(unitFact is BlueprintAbility blueprintAbility))
				{
					if (!(unitFact is BlueprintActivatableAbility activatableAbility))
					{
						if (unitFact is BlueprintBuff blueprintBuff)
						{
							return new TooltipTemplateBuff(blueprintBuff, mechanicEntity);
						}
						return null;
					}
					return new TooltipTemplateActivatableAbility(activatableAbility);
				}
				return new TooltipTemplateAbility(blueprintAbility, null, mechanicEntity);
			}
			return new TooltipTemplateFeature(feature, withVariants: false, mechanicEntity);
		}
		case EntityLink.Type.GroupAbility:
			return new TooltipTemplateAbility(LinksHelper.GetPartyAbility(keys[1]));
		case EntityLink.Type.UI:
			return new TooltipTemplateGlossary(keys);
		case EntityLink.Type.Encyclopedia:
			return new TooltipTemplateGlossary(keys, isHistory: true, isEncyclopedia);
		case EntityLink.Type.SkillcheckResult:
			if (skillCheckResults == null)
			{
				return null;
			}
			return new TooltipTemplateSkillCheckResult(skillCheckResults, keys);
		case EntityLink.Type.SkillcheckDC:
			if (skillCheckDcs == null)
			{
				return null;
			}
			return new TooltipTemplateSkillCheckDC(skillCheckDcs);
		case EntityLink.Type.DialogExchange:
			return new TooltipTemplateAnswerExchange(LinksHelper.GetAnswer(keys[1]));
		case EntityLink.Type.DialogConditions:
			return new TooltipTemplateAnswerConditions(LinksHelper.GetAnswer(keys[1]));
		case EntityLink.Type.UIProperty:
			return new TooltipTemplateUIProperty(LinksHelper.GetUISettings(keys));
		case EntityLink.Type.Unknown:
			return new TooltipTemplateGlossary(keys);
		case EntityLink.Type.Empty:
			return null;
		case EntityLink.Type.UnitStat:
			return new TooltipTemplateStat(LinksHelper.GetStatData(keys[1], keys[2]));
		default:
			return null;
		}
	}

	public static void AddGlossaryHistory(BlueprintEncyclopediaGlossaryEntry glossaryEntry)
	{
		if (glossaryEntry == null)
		{
			PFLog.UI.Log("UI", "InfoWindowVM.AddHistory: glossaryEntry is null");
			return;
		}
		if (HistoryPointer != null)
		{
			if (HistoryPointer.Value == glossaryEntry)
			{
				return;
			}
			while (HistoryPointer.Next != null)
			{
				History.Remove(HistoryPointer.Next);
			}
		}
		LinkedListNode<BlueprintEncyclopediaGlossaryEntry> linkedListNode = History.Find(glossaryEntry);
		if (linkedListNode != null)
		{
			History.Remove(linkedListNode);
		}
		if (History.Count > 20)
		{
			History.RemoveFirst();
		}
		History.AddLast(new LinkedListNode<BlueprintEncyclopediaGlossaryEntry>(glossaryEntry));
		HistoryPointer = History.Last;
	}

	public static void GlossaryHistoryNext(GridConsoleNavigationBehaviour ownerBehaviour)
	{
		if (HistoryPointer.Next != null)
		{
			HistoryPointer = HistoryPointer.Next;
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleGlossaryInfoRequest(new TooltipTemplateGlossary(HistoryPointer.Value, isHistory: true), ownerBehaviour);
			});
		}
	}

	public static void GlossaryHistoryPrevious(GridConsoleNavigationBehaviour ownerBehaviour)
	{
		if (HistoryPointer.Previous != null)
		{
			HistoryPointer = HistoryPointer.Previous;
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleGlossaryInfoRequest(new TooltipTemplateGlossary(HistoryPointer.Value, isHistory: true), ownerBehaviour);
			});
		}
	}

	public static void CloseGlossaryInfoWindow()
	{
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleGlossaryInfoRequest(null);
		});
	}

	public static IDisposable SetHint(this MonoBehaviour component, IReadOnlyReactiveProperty<string> reactiveText, string bindingName = null, Color color = default(Color))
	{
		IDisposable hintSubscription = null;
		IDisposable textSubscription = reactiveText.Subscribe(delegate(string text)
		{
			hintSubscription?.Dispose();
			hintSubscription = component.SetHint(text, bindingName, color);
		});
		return Disposable.Create(delegate
		{
			hintSubscription?.Dispose();
			textSubscription?.Dispose();
		});
	}

	public static IDisposable SetHint(this MonoBehaviour component, string text, string bindingName = null, Color color = default(Color), bool shouldShow = true)
	{
		if (component == null)
		{
			return null;
		}
		if (text.IsNullOrEmpty())
		{
			return Disposable.Empty;
		}
		HintData hintData = new HintData
		{
			Text = text,
			BindingName = bindingName,
			Color = ((color == default(Color)) ? Color.white : color)
		};
		bool show = false;
		IDisposable enter = component.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (Cursor.visible || UIKitRewiredCursorController.Enabled)
			{
				show = true;
				EventBus.RaiseEvent(delegate(ITooltipHandler h)
				{
					h.HandleHintRequest(hintData, shouldShow);
				});
			}
		});
		Action exitAction = delegate
		{
			if (Cursor.visible || UIKitRewiredCursorController.Enabled)
			{
				show = false;
				EventBus.RaiseEvent(delegate(ITooltipHandler h)
				{
					h.HandleHintRequest(null, shouldShow: true);
				});
			}
		};
		IDisposable exit = component.OnPointerExitAsObservable().Subscribe(delegate
		{
			exitAction();
		});
		IDisposable disable = ObservableExtensions.Subscribe(component.OnDisableAsObservable(), delegate
		{
			exitAction();
		});
		return Disposable.Create(delegate
		{
			if (show)
			{
				exitAction();
			}
			enter?.Dispose();
			exit?.Dispose();
			disable?.Dispose();
		});
	}
}
