using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DG.Tweening;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Interaction;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps.Simple;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public static class UIUtility
{
	private class TweenDisposer : IDisposable
	{
		private readonly Tween m_Tween;

		private bool m_Disposed;

		public TweenDisposer(Tween tween)
		{
			m_Tween = tween;
		}

		public void Dispose()
		{
			if (!m_Disposed)
			{
				m_Disposed = true;
				if (m_Tween.IsActive() && m_Tween.IsPlaying())
				{
					m_Tween.Complete(withCallbacks: true);
					m_Tween.Kill(complete: true);
				}
			}
		}
	}

	public enum EndDragAction
	{
		Put,
		Swap,
		Split,
		HalfSplit,
		Abort,
		Merge
	}

	public static class EntityLinkActions
	{
		public static bool IsPossibleGoToEncyclopedia
		{
			get
			{
				if (RootUIContext.Instance.IsMainMenu)
				{
					return false;
				}
				return true;
			}
		}

		public static void ShowUnit(BaseUnitEntity unit)
		{
			if (unit != null)
			{
				CameraRig.Instance.ScrollToImmediately(unit.Position);
			}
		}

		public static void ShowEncyclopediaPage(string pageKey)
		{
			ShowEncyclopediaPage(ChapterList.GetPage(pageKey));
		}

		public static void ShowEncyclopediaPage(INode page)
		{
			if (!IsPossibleGoToEncyclopedia)
			{
				return;
			}
			Game.Instance.Player.UISettings.CurrentEncyclopediaPage = page as IPage;
			if (RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.Encyclopedia)
			{
				EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
				{
					h.HandleOpenEncyclopedia(page);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(IEncyclopediaHandler h)
				{
					h.HandleEncyclopediaPage(page);
				});
			}
		}
	}

	public static float DefaultBarkTime = 5f;

	public static string DefaultSaveName
	{
		get
		{
			TimeSpan gameTime = Game.Instance.Player.GameTime;
			return string.Concat(SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedArea, null)?.AreaDisplayName, " -", UIStrings.Instance.SaveLoadTexts.SaveDefaultName, " -", $"{gameTime.Hours:D2}:{gameTime.Minutes:D2}:{gameTime.Seconds:D2}");
		}
	}

	public static BaseUnitEntity GetCurrentSelectedUnit()
	{
		return Game.Instance.SelectionCharacter.SelectedUnitInUI.Value;
	}

	public static Bounds GetMaxBounds(GameObject g, Renderer[] rend)
	{
		Bounds result = new Bounds(g.transform.position, Vector3.zero);
		foreach (Renderer renderer in rend)
		{
			result.Encapsulate(renderer.bounds);
		}
		return result;
	}

	public static string ArabicToRoman(int number)
	{
		if (number == 0)
		{
			return "-";
		}
		StringBuilder stringBuilder = new StringBuilder();
		int[] array = new int[13]
		{
			1, 4, 5, 9, 10, 40, 50, 90, 100, 400,
			500, 900, 1000
		};
		string[] array2 = new string[13]
		{
			"I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD",
			"D", "CM", "M"
		};
		while (number > 0)
		{
			for (int num = array.Length - 1; num >= 0; num--)
			{
				if (number / array[num] >= 1)
				{
					number -= array[num];
					stringBuilder.Append(array2[num]);
					break;
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static (BlueprintAbility HeroicAct, BlueprintAbility DesperateMeasure) GetUltimateAbilities(BlueprintUnitFact blueprintUnitFact)
	{
		BlueprintAbility item = null;
		BlueprintAbility item2 = null;
		AddFacts component = blueprintUnitFact.GetComponent<AddFacts>();
		if (component != null)
		{
			foreach (BlueprintAbility item3 in component.Facts.Cast<BlueprintAbility>().NotNull())
			{
				if (item3.IsHeroicAct)
				{
					item = item3;
				}
				if (item3.IsDesperateMeasure)
				{
					item2 = item3;
				}
			}
		}
		return (HeroicAct: item, DesperateMeasure: item2);
	}

	public static Color32 GetColorByText(string text)
	{
		Color32[] randomColors = Game.Instance.BlueprintRoot.UIConfig.RandomColors;
		if (text == null)
		{
			return randomColors[0];
		}
		int num = text.Length - randomColors.Length * Convert.ToInt32(Math.Floor(Convert.ToDecimal(text.Length / randomColors.Length)));
		return randomColors[num];
	}

	public static Sprite GetIconByText(string text)
	{
		Sprite[] abilityPlaceholderIcon = Game.Instance.BlueprintRoot.UIConfig.UIIcons.AbilityPlaceholderIcon;
		if (text == null)
		{
			return abilityPlaceholderIcon[0];
		}
		int num = text.Length - abilityPlaceholderIcon.Length * Convert.ToInt32(Math.Floor(Convert.ToDecimal(text.Length / abilityPlaceholderIcon.Length)));
		return abilityPlaceholderIcon[num];
	}

	public static string GetBookFormat(string text, TMP_FontAsset font, Color color = default(Color), int size = 140, float voffset = 0f, Material fontMaterial = null)
	{
		string text2 = text.Trim();
		if (string.IsNullOrEmpty(text) || font == null)
		{
			return text;
		}
		int i;
		for (i = 0; i < text2.Length && !IsLetterOrOpeningTag(text2[i]); i++)
		{
		}
		if (i >= text2.Length)
		{
			return text;
		}
		char c2 = text2[i];
		if (c2 == '<')
		{
			int j;
			for (j = text2.IndexOf('>', i); j < text2.Length && !char.IsLetter(text2[j]); j++)
			{
			}
			if (j < text2.Length)
			{
				c2 = text2[j];
				i = j;
			}
		}
		string text3 = ColorUtility.ToHtmlStringRGB((color != default(Color)) ? color : BlueprintRoot.Instance.UIConfig.PaperSaberColor);
		string text4 = text2.Substring(0, i);
		string text5 = ((fontMaterial != null) ? (" material=\"" + fontMaterial.name + "\"") : string.Empty);
		object[] obj = new object[8] { text4, voffset, text3, size, font.name, text5, c2, null };
		string text6 = text2;
		int num = i + 1;
		obj[7] = text6.Substring(num, text6.Length - num);
		return string.Format("{0}<voffset={1}em><color=#{2}><size={3}%><font=\"{4}\"{5}>{6}</font></size></color></voffset>{7}", obj);
		static bool IsLetterOrOpeningTag(char c)
		{
			if (!char.IsLetter(c))
			{
				return c == '<';
			}
			return true;
		}
	}

	public static float CalculateBarkWidth(string text, float symWidth)
	{
		int length = text.Length;
		float num = 0f;
		if (text.Length > 25)
		{
			string[] array = text.Split(new char[1] { ' ' });
			int num2 = 0;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				num2 += text2.Length;
				if (num2 > 20)
				{
					break;
				}
				num2++;
			}
			num = symWidth * 0.58f * (float)num2;
			return Mathf.Max(Mathf.Ceil(symWidth * (float)length / Mathf.Sqrt(0.625f * (float)length)), num);
		}
		return Mathf.Ceil(symWidth * 0.58f * (float)length);
	}

	public static void ShowMessageBox(string messageText, DialogMessageBoxBase.BoxType boxType, Action<DialogMessageBoxBase.BoxButton> onClose, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, int waitTime = 0)
	{
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(messageText, boxType, onClose, onLinkInvoke, yesLabel, noLabel, null, null, null, waitTime);
		});
	}

	public static string GetAbilityAcronym(BlueprintFeatureBase featureBase)
	{
		if (!(featureBase is BlueprintFeature { Acronym: var acronym }))
		{
			return GetAbilityAcronym(featureBase.Name);
		}
		if (string.IsNullOrEmpty(acronym))
		{
			return GetAbilityAcronym(featureBase.Name);
		}
		return acronym;
	}

	public static string GetAbilityAcronym(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		Locale currentLocale = LocalizationManager.Instance.CurrentLocale;
		name = UIConfig.Instance.AcronymsConfig.GetWordsToExcludeFor(currentLocale).Aggregate(name, (string current, string excludeWord) => current.Replace(excludeWord, " "));
		name = name.Replace("-", " ");
		if (LocalizationManager.Instance.CurrentLocale == Locale.enGB)
		{
			for (int i = 0; i < name.Length; i++)
			{
				if (char.IsUpper(name, i))
				{
					stringBuilder.Append(name[i]);
					stringBuilder.Append(" ");
				}
			}
		}
		else if (name.Length > 0)
		{
			bool flag = true;
			bool flag2 = false;
			string text = name;
			foreach (char c in text)
			{
				switch (c)
				{
				case ' ':
				case '(':
				case ')':
					flag = true;
					continue;
				case '<':
				case '[':
				case '{':
					flag2 = true;
					continue;
				}
				if (flag2)
				{
					if (c == ']')
					{
						flag2 = false;
					}
				}
				else if (flag)
				{
					stringBuilder.Append(c);
					stringBuilder.Append(" ");
					flag = false;
				}
			}
		}
		string text2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringBuilder.ToString());
		text2 = new string((from s in text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
			select s[0]).ToArray());
		UIConfig.Instance.AcronymsConfig.GetLettersInAcronym(currentLocale, out var maxLettersCount, out var preferredLettersCount);
		maxLettersCount = Math.Max(1, maxLettersCount);
		if (text2.Length > maxLettersCount)
		{
			text2 = text2.Remove(preferredLettersCount);
		}
		return text2;
	}

	public static void ShowProfitFactorModifiedNotification(float max, ProfitFactorModifier modifier)
	{
		string text = string.Format(modifier.IsNegative ? UIStrings.Instance.ProfitFactorTexts.ProfitFatorLostNotification : UIStrings.Instance.ProfitFactorTexts.ProfitFatorGainedNotification, modifier.Value);
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(text, addToLog: false);
		});
	}

	public static string TimeSpanToInGameTime(TimeSpan timeSpan)
	{
		return string.Format(UIStrings.Instance.SaveLoadTexts.InGameFormat, (timeSpan.TotalDays >= 1.0) ? timeSpan.ToString("d' days 'hh':'mm") : timeSpan.ToString("hh':'mm"));
	}

	public static List<BaseUnitEntity> GetGroup(bool withRemote = false, bool withPet = false)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		GetGroup(list, withRemote, withPet);
		return list;
	}

	public static void GetGroup(List<BaseUnitEntity> characters, bool withRemote = false, bool withPet = false)
	{
		characters.Clear();
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.Player.Party.Where((BaseUnitEntity u) => !u.Facts.HasComponent<TransientPartyMemberFlag>());
		if (withRemote)
		{
			characters.AddRange(enumerable);
			List<BaseUnitEntity> list = Game.Instance.Player.RemoteCompanions.ToList();
			foreach (BaseUnitEntity entity in list.ToList())
			{
				if (entity.IsMaster)
				{
					BaseUnitEntity item = list.Find((BaseUnitEntity e) => e.Master == entity);
					list.RemoveAt(list.IndexOf(item));
					list.Insert(list.IndexOf(entity) + 1, item);
				}
			}
			characters.AddRange(list);
		}
		else
		{
			characters.AddRange(enumerable.Where(IsViewActiveUnit));
		}
		if (!withPet)
		{
			return;
		}
		foreach (BaseUnitEntity item2 in Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsPet && IsViewActiveUnit(c)))
		{
			BaseUnitEntity master = item2.Master;
			int num = characters.FindIndex((BaseUnitEntity m) => m == master);
			if (enumerable.Contains(item2.Master))
			{
				if (num < 0 || num + 1 >= characters.Count)
				{
					characters.Add(item2);
				}
				else
				{
					characters.Insert(num + 1, item2);
				}
			}
		}
	}

	public static void GetActualGroup(List<BaseUnitEntity> characters)
	{
		characters.Clear();
		IEnumerable<BaseUnitEntity> collection = Game.Instance.SelectionCharacter.ActualGroup.Where((BaseUnitEntity u) => !u.Facts.HasComponent<TransientPartyMemberFlag>());
		characters.AddRange(collection);
	}

	public static bool IsViewActiveUnit(BaseUnitEntity unit)
	{
		if (!unit.IsViewActive)
		{
			return !RootUIContext.Instance.IsSurface;
		}
		return true;
	}

	public static bool IsGlobalMap()
	{
		return Game.Instance.CurrentlyLoadedArea?.IsGlobalmapArea ?? false;
	}

	public static bool IsShipArea()
	{
		return Game.Instance.CurrentlyLoadedArea?.IsShipArea ?? false;
	}

	public static float GetBarkDuration(string text)
	{
		Locale currentLocale = LocalizationManager.Instance.CurrentLocale;
		if (currentLocale == Locale.zhCN || currentLocale == Locale.jaJP)
		{
			return (float)text.Length * 0.3f;
		}
		return (float)text.Split(' ').Length * 0.6f;
	}

	public static void SetPopupWindowPosition(RectTransform windowTransform, RectTransform sourceTransform, Vector2 shiftPosition, List<Vector2> priorityPivots = null)
	{
		if (sourceTransform == null)
		{
			return;
		}
		List<Vector2> list = new List<Vector2>();
		TooltipEngine.SetPivots(list, priorityPivots);
		Vector3 vector = UICamera.Instance.WorldToViewportPoint(sourceTransform.position);
		RectTransform component = windowTransform.parent.gameObject.GetComponent<RectTransform>();
		vector = new Vector2(vector.x - sourceTransform.rect.width * (sourceTransform.pivot.x - 0.5f) / component.rect.width, vector.y - sourceTransform.rect.height * (sourceTransform.pivot.y - 0.5f) / component.rect.height);
		Vector2 anchorMax = (windowTransform.anchorMin = vector);
		windowTransform.anchorMax = anchorMax;
		Vector2 vector3 = new Vector2(sourceTransform.rect.width / 2f + shiftPosition.x, sourceTransform.rect.height / 2f + shiftPosition.y);
		Vector2 vector4 = new Vector2(vector3.x / component.rect.width, vector3.y / component.rect.height);
		foreach (Vector2 item in list)
		{
			Vector2 vector5 = new Vector2(1f - item.x * 2f, 1f - item.y * 2f);
			Vector2 vector6 = new Vector2(vector.x + vector4.x * vector5.x, vector.y + vector4.y * vector5.y);
			anchorMax = (windowTransform.anchorMin = vector6);
			windowTransform.anchorMax = anchorMax;
			windowTransform.pivot = item;
			windowTransform.anchoredPosition = new Vector2(0f, 0f);
			if (IsTransformInScreen(windowTransform))
			{
				return;
			}
		}
		SetInTheMiddle(windowTransform);
	}

	private static void SetInTheMiddle(RectTransform windowTransform)
	{
		Vector2 anchorMax = (windowTransform.anchorMin = new Vector2(0.5f, 0.5f));
		windowTransform.anchorMax = anchorMax;
		windowTransform.pivot = new Vector2(0.5f, 0.5f);
		windowTransform.anchoredPosition = new Vector2(0f, 0f);
	}

	public static bool IsTransformInScreen(Transform transform)
	{
		RectTransform component = transform.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		Canvas.ForceUpdateCanvases();
		component.GetWorldCorners(array);
		return array.Select((Vector3 v) => RectTransformUtility.WorldToScreenPoint(UICamera.Instance, v)).All(CheckIfFitsToScreen);
	}

	private static bool CheckIfFitsToScreen(Vector2 screenPoint)
	{
		if (screenPoint.x < 0f || screenPoint.y < 0f)
		{
			return false;
		}
		if (screenPoint.x > (float)UICamera.Instance.pixelWidth)
		{
			return false;
		}
		if (screenPoint.y > (float)UICamera.Instance.pixelHeight)
		{
			return false;
		}
		return true;
	}

	public static LocalizedString GetSoulMarkDirectionText(SoulMarkDirection direction)
	{
		UITextAlignment alignment = UIStrings.Instance.Alignment;
		return direction switch
		{
			SoulMarkDirection.Faith => alignment.Imperialis, 
			SoulMarkDirection.Hope => alignment.Benevolentia, 
			SoulMarkDirection.Corruption => alignment.Hereticus, 
			SoulMarkDirection.Reason => alignment.Reason, 
			SoulMarkDirection.None => new LocalizedString(), 
			_ => new LocalizedString(), 
		};
	}

	public static LocalizedString GetSoulMarkRankText(int index)
	{
		UITextAlignment alignment = UIStrings.Instance.Alignment;
		if (index > 0)
		{
			return index switch
			{
				1 => alignment.SoulMarkRankTier1, 
				2 => alignment.SoulMarkRankTier2, 
				3 => alignment.SoulMarkRankTier3, 
				4 => alignment.SoulMarkRankTier4, 
				5 => alignment.SoulMarkRankTier5, 
				_ => new LocalizedString(), 
			};
		}
		return alignment.SoulMarkRankTierNone;
	}

	public static string GetStatText(StatType statType)
	{
		return LocalizedTexts.Instance.Stats.GetText(statType);
	}

	public static string SkillCheckText(List<SkillCheckResult> skillCheck, DialogColors dialogColors)
	{
		string text = "";
		if ((bool)SettingsRoot.Game.Dialogs.ShowSkillcheckResult)
		{
			if (skillCheck == null)
			{
				return text;
			}
			foreach (SkillCheckResult item in skillCheck)
			{
				text += SkillCheckText(item, dialogColors);
			}
		}
		return text;
	}

	public static string SkillCheckText(SkillCheckResult skillCheck, DialogColors dialogColors)
	{
		if (skillCheck == null)
		{
			return null;
		}
		string arg;
		string text;
		if (Game.Instance.DialogController.Dialog.Type == DialogType.Book)
		{
			arg = ColorUtility.ToHtmlStringRGB(skillCheck.Passed ? dialogColors.SkillCheckSuccessfulBE : dialogColors.SkillCheckFailedBE);
			text = "\n\n";
		}
		else
		{
			arg = ColorUtility.ToHtmlStringRGB(skillCheck.Passed ? dialogColors.SkillCheckSuccessfulDialogue : dialogColors.SkillCheckFailedDialogue);
			text = " ";
		}
		UIDialog dialog = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Dialog;
		string text2 = (skillCheck.Passed ? dialog.Succeeded : dialog.Failed);
		string text3 = EntityLink.Type.UnitStat.GetTag() + ":" + skillCheck.StatType.ToString() + ":" + skillCheck.ActingUnit.UniqueId;
		string format = (skillCheck.Passed ? dialog.SucccedeedCheckFormat : dialog.FailedCheckFormat);
		string arg2 = "<link=\"" + EntityLink.Type.SkillcheckResult.ToString() + "\">" + text2 + "</link>";
		string arg3 = "<link=\"" + text3 + "\">" + LocalizedTexts.Instance.Stats.GetText(skillCheck.StatType) + "</link>";
		return string.Format(format, arg2, arg, arg3) + text;
	}

	public static void MoveLensPosition(Transform lens, Vector3 target, float duration, bool withSound = true)
	{
		lens.DOLocalMove(target, duration).OnStart(delegate
		{
			if (withSound)
			{
				PlaySelectorSound();
			}
		}).OnComplete(delegate
		{
			if (withSound)
			{
				StopSelectorSound();
			}
		})
			.SetUpdate(isIndependentUpdate: true);
	}

	public static IDisposable CreateMoveXLensPosition(Transform lens, float target, float duration, bool withSound = true)
	{
		return new TweenDisposer(lens.DOLocalMoveX(target, duration).OnStart(delegate
		{
			if (withSound)
			{
				PlaySelectorSound();
			}
		}).OnComplete(delegate
		{
			if (withSound)
			{
				StopSelectorSound();
			}
		})
			.SetUpdate(isIndependentUpdate: true));
	}

	private static void PlaySelectorSound()
	{
		UISounds.Instance.Sounds.Selector.SelectorStart.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStart.Play();
	}

	private static void StopSelectorSound()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
	}

	public static string SoulMarkShiftsText(List<SoulMarkShift> shifts, DialogColors dialogColors)
	{
		string text = "";
		if (shifts == null)
		{
			return text;
		}
		foreach (SoulMarkShift shift in shifts)
		{
			if (!shift.Empty)
			{
				SoulMarkDirection direction = shift.Direction;
				int value = shift.Value;
				string arg = ColorUtility.ToHtmlStringRGB((shift.Value > 0) ? dialogColors.SoulMarkShiftBePositive : dialogColors.SoulMarkShiftBeNegative);
				text += string.Format(UIStrings.Instance.Dialog.SoulMarkShiftFormat.Text, GetSoulMarkDirectionText(direction).Text, value, arg);
				text += "\n\n";
			}
		}
		return text;
	}

	public static List<PrerequisiteEntryVM> GetPrerequisiteEntries(CalculatedPrerequisite prerequisite, bool addTooltip = false)
	{
		List<PrerequisiteEntryVM> list = new List<PrerequisiteEntryVM>();
		if (!(prerequisite is CalculatedPrerequisiteFact calculatedPrerequisiteFact))
		{
			if (!(prerequisite is CalculatedPrerequisiteComposite calculatedPrerequisiteComposite))
			{
				if (!(prerequisite is CalculatedPrerequisiteStat calculatedPrerequisiteStat))
				{
					if (!(prerequisite is CalculatedPrerequisiteMaxRankNotReached calculatedPrerequisiteMaxRankNotReached))
					{
						if (prerequisite is CalculatedPrerequisiteLevel calculatedPrerequisiteLevel)
						{
							string text = UIStrings.Instance.Tooltips.PrerequisiteLevel;
							bool value = calculatedPrerequisiteLevel.Value;
							bool not = calculatedPrerequisiteLevel.Not;
							int level = calculatedPrerequisiteLevel.Level;
							list.Add(new PrerequisiteEntryVM(text, value, not, level.ToString()));
						}
					}
					else
					{
						list.Add(new PrerequisiteEntryVM(UIStrings.Instance.Tooltips.PrerequisiteRank, calculatedPrerequisiteMaxRankNotReached.Value, calculatedPrerequisiteMaxRankNotReached.Not, null, isTitle: true));
					}
				}
				else
				{
					list.Add(new PrerequisiteEntryVM(LocalizedTexts.Instance.Stats.GetText(calculatedPrerequisiteStat.Stat), calculatedPrerequisiteStat.Value, calculatedPrerequisiteStat.Not, calculatedPrerequisiteStat.MinValue.ToString()));
				}
			}
			else if (CanUnpackComposite(calculatedPrerequisiteComposite))
			{
				list.Add(UnpackPrerequisiteComposite(calculatedPrerequisiteComposite, addTooltip));
			}
			else
			{
				foreach (CalculatedPrerequisite item in calculatedPrerequisiteComposite.Prerequisites.Where((CalculatedPrerequisite i) => !(i is CalculatedPrerequisiteComposite)))
				{
					list.AddRange(GetPrerequisiteEntries(item, addTooltip));
				}
				foreach (CalculatedPrerequisite item2 in calculatedPrerequisiteComposite.Prerequisites.Where((CalculatedPrerequisite i) => i is CalculatedPrerequisiteComposite))
				{
					list.Add(new PrerequisiteEntryVM(GetPrerequisiteEntries(item2, addTooltip), calculatedPrerequisiteComposite.Value, calculatedPrerequisiteComposite.Composition == FeaturePrerequisiteComposition.Or));
				}
			}
		}
		else if (!calculatedPrerequisiteFact.IsDlcRestrictedContent)
		{
			BlueprintUnitFact fact = calculatedPrerequisiteFact.Fact;
			string text3;
			if (fact is BlueprintFeature && fact.name == "AeldariRace")
			{
				if (calculatedPrerequisiteFact.Not)
				{
					string text2 = UIStrings.Instance.Tooltips.not.Text;
					text3 = string.Format(text2, GetFactName(calculatedPrerequisiteFact, addTooltip)) ?? "";
				}
				else
				{
					text3 = GetPrerequisiteFactName(calculatedPrerequisiteFact) + " " + GetFactName(calculatedPrerequisiteFact, addTooltip) + ".";
				}
			}
			else
			{
				string text2 = (calculatedPrerequisiteFact.Not ? UIStrings.Instance.Tooltips.notSimple.Text : "{0}");
				text3 = GetPrerequisiteFactName(calculatedPrerequisiteFact) + " " + string.Format(text2, GetFactName(calculatedPrerequisiteFact, addTooltip)) + ".";
			}
			list.Add(new PrerequisiteEntryVM(text3, calculatedPrerequisiteFact.Value, calculatedPrerequisiteFact.Not));
		}
		return list;
	}

	private static bool CanUnpackComposite(CalculatedPrerequisiteComposite prerequisiteComposite)
	{
		return prerequisiteComposite.Prerequisites.All((CalculatedPrerequisite i) => i is CalculatedPrerequisiteFact);
	}

	private static PrerequisiteEntryVM UnpackPrerequisiteComposite(CalculatedPrerequisiteComposite prerequisiteComposite, bool addTooltip)
	{
		StringBuilder stringBuilder = new StringBuilder(GetPrerequisiteFactName(prerequisiteComposite.Prerequisites.First() as CalculatedPrerequisiteFact) + ".");
		string separator = ((prerequisiteComposite.Composition == FeaturePrerequisiteComposition.Or) ? (" " + UIStrings.Instance.Tooltips.or.Text + " ") : (" " + UIStrings.Instance.Tooltips.and.Text + " "));
		List<string> list = new List<string>();
		foreach (CalculatedPrerequisite prerequisite in prerequisiteComposite.Prerequisites)
		{
			if (prerequisite is CalculatedPrerequisiteFact calculatedPrerequisiteFact)
			{
				if (!calculatedPrerequisiteFact.IsDlcRestrictedContent)
				{
					string text = (calculatedPrerequisiteFact.Not ? UIStrings.Instance.Tooltips.notSimple.Text : "{0}");
					if (prerequisiteComposite.Composition == FeaturePrerequisiteComposition.And)
					{
						separator = ".\n";
					}
					else
					{
						text = text.ToLower();
					}
					list.Add(string.Format(text, GetFactName(calculatedPrerequisiteFact, addTooltip)));
				}
				continue;
			}
			throw new ArgumentOutOfRangeException();
		}
		stringBuilder.Append(string.Join(separator, list));
		stringBuilder.Append(".");
		return new PrerequisiteEntryVM(stringBuilder.ToString(), prerequisiteComposite.Value, prerequisiteComposite.Not);
	}

	private static string GetFactName(CalculatedPrerequisiteFact prerequisiteFact, bool addTooltip)
	{
		string text = "<b>" + prerequisiteFact.Fact.Name + "</b>";
		if (prerequisiteFact.Fact is BlueprintFeature blueprintFeature)
		{
			text = (addTooltip ? ("<link=\"f:" + blueprintFeature.AssetGuid + "\">" + text + "</link>") : text);
		}
		return text;
	}

	private static string GetPrerequisiteFactName(CalculatedPrerequisiteFact prerequisiteFact)
	{
		BlueprintUnitFact fact = prerequisiteFact.Fact;
		LocalizedString localizedString;
		if (!(fact is BlueprintAbility))
		{
			if (!(fact is BlueprintCareerPath))
			{
				if (!(fact is BlueprintFeature))
				{
					throw new ArgumentOutOfRangeException();
				}
				localizedString = UIStrings.Instance.Tooltips.PrerequisiteFeatures;
			}
			else
			{
				localizedString = UIStrings.Instance.Tooltips.PrerequisiteCareers;
			}
		}
		else
		{
			localizedString = UIStrings.Instance.Tooltips.PrerequisiteAbilities;
		}
		return localizedString;
	}

	public static bool IsMagicItem(ItemEntity item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.Blueprint.IsNotable)
		{
			return false;
		}
		if (!item.IsIdentified)
		{
			return true;
		}
		if (item is ItemEntityUsable)
		{
			return true;
		}
		if (item.Blueprint is BlueprintItemEquipment blueprintItemEquipment && blueprintItemEquipment.Abilities.Any())
		{
			return true;
		}
		if (item is ItemEntityWeapon weapon)
		{
			return GameHelper.GetItemEnhancementBonus(weapon) > 0;
		}
		if (item is ItemEntityArmor itemEntityArmor)
		{
			if (GameHelper.GetItemEnhancementBonus(itemEntityArmor) > 0)
			{
				return true;
			}
			return Enumerable.Any(itemEntityArmor.Enchantments, (ItemEnchantment p) => p.Blueprint.IdentifyDC > 0);
		}
		if (item is ItemEntityShield item2)
		{
			return GameHelper.GetItemEnhancementBonus(item2) > 0;
		}
		if (item.Blueprint is BlueprintItemEquipmentSimple blueprintItemEquipmentSimple && Enumerable.Any(blueprintItemEquipmentSimple.Enchantments))
		{
			return true;
		}
		return false;
	}

	public static string PackKeys(params object[] word)
	{
		string text = "";
		for (int i = 0; i < word.Length; i++)
		{
			text += string.Format((i + 1 == word.Length) ? "{0}" : "{0}:", word[i].ToString());
		}
		return text;
	}

	public static string[] GetKeysFromLink(string linkID)
	{
		string[] source = linkID.Split(':');
		_ = new string[0];
		return source.Where((string x) => !string.IsNullOrEmpty(x)).ToArray();
	}

	public static string GetGlossaryEntryName(string key)
	{
		LocalizedString localizedString = GetGlossaryEntry(key)?.Title;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	public static BlueprintEncyclopediaGlossaryEntry GetGlossaryEntry(string key)
	{
		BlueprintEncyclopediaPage page = ChapterList.GetPage(key);
		if (!(page is BlueprintEncyclopediaGlossaryEntry result))
		{
			if (page != null)
			{
				return (BlueprintEncyclopediaGlossaryEntry)page.GlossaryEntry;
			}
			return null;
		}
		return result;
	}

	public static bool CheckLinkKeyHasContent(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		string[] keysFromLink = GetKeysFromLink(key);
		if (EntityLink.GetEntityType(keysFromLink[0]) != EntityLink.Type.Encyclopedia)
		{
			return true;
		}
		if (keysFromLink.Length < 2)
		{
			return false;
		}
		BlueprintEncyclopediaGlossaryEntry glossaryEntry = GetGlossaryEntry(keysFromLink[1]);
		if (glossaryEntry == null)
		{
			return false;
		}
		if (!glossaryEntry.GetDescription().IsSet() && !glossaryEntry.Title.IsSet())
		{
			return false;
		}
		return true;
	}

	private static void CalculateGlossaryPointCoordinates(TMP_CharacterInfo firstLetter, TMP_CharacterInfo lastLetter, out float height, out float width, out Vector2 middlePoint)
	{
		height = Mathf.Abs(Mathf.Abs(firstLetter.bottomRight.y) - Mathf.Abs(firstLetter.topRight.y));
		width = lastLetter.topLeft.x - firstLetter.topLeft.x;
		middlePoint = new Vector2(firstLetter.topLeft.x + width / 2f, firstLetter.topRight.y - height / 2f);
	}

	public static string AddSign(float? value)
	{
		if (!value.HasValue)
		{
			return "–";
		}
		if (!(value >= 0f))
		{
			return "–" + (0f - value);
		}
		float? num = value;
		return "+" + num;
	}

	public static Vector2 LimitPositionRectInRect(Vector2 nPos, RectTransform parent, RectTransform child)
	{
		float width = parent.rect.width;
		float height = parent.rect.height;
		float width2 = child.rect.width;
		float height2 = child.rect.height;
		Vector2 pivot = child.pivot;
		if (nPos.x + width / 2f - pivot.x * width2 <= 0f)
		{
			nPos.x = (0f - width) / 2f + pivot.x * width2;
		}
		else if (nPos.x + width / 2f + (1f - pivot.x) * width2 >= width)
		{
			nPos.x = width / 2f - (1f - pivot.x) * width2;
		}
		if (nPos.y + height / 2f - pivot.y * height2 <= 0f)
		{
			nPos.y = (0f - height) / 2f + pivot.x * height2;
		}
		else if (nPos.y + height / 2f + (1f - pivot.y) * height2 >= height)
		{
			nPos.y = height / 2f - (1f - pivot.y) * height2;
		}
		return nPos;
	}

	public static void DoTaskLater(float timer, TweenCallback action, bool update = true)
	{
		if (Math.Abs(timer) < Mathf.Epsilon)
		{
			action?.Invoke();
			return;
		}
		DOTween.To(() => 1f, delegate
		{
		}, 0f, timer).SetUpdate(update).OnComplete(action);
	}

	public static string GetInteractionVariantActorText(IInteractionVariantActor interactionActor, List<BaseUnitEntity> units, out bool needChanceText)
	{
		needChanceText = false;
		if (interactionActor == null)
		{
			return string.Empty;
		}
		int? interactionDC = interactionActor.InteractionDC;
		string interactionName = interactionActor.GetInteractionName();
		if (!interactionDC.HasValue)
		{
			return "[" + interactionName + "]";
		}
		StatType skill = interactionActor.Skill;
		needChanceText = true;
		BaseUnitEntity baseUnitEntity = interactionActor.InteractionPart?.SelectUnit(units, muteEvents: false, skill);
		interactionDC = InteractionHelper.GetInteractionSkillCheckChance(baseUnitEntity, skill, interactionDC.Value);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(interactionName + ": ");
		if (baseUnitEntity != null)
		{
			stringBuilder.Append($"{interactionDC}% [{baseUnitEntity.Name}]");
		}
		else
		{
			stringBuilder.Append("Locked%");
		}
		return stringBuilder.ToString();
	}

	public static string GetOvertipSkillCheckText(InteractionSkillCheckPart skillCheck, List<BaseUnitEntity> units, out bool needChanceText)
	{
		needChanceText = false;
		if (skillCheck == null)
		{
			return string.Empty;
		}
		StatType statType = (skillCheck.SkillOverride.IsSkill() ? skillCheck.SkillOverride : skillCheck.Settings.Skill);
		if (statType == StatType.Unknown)
		{
			return string.Empty;
		}
		string text = BlueprintRoot.Instance.LocalizedTexts.Stats.GetText(statType);
		if (skillCheck.Settings.HideDC)
		{
			if (skillCheck.GetSkill() != StatType.SkillAwareness)
			{
				return "[" + text + "]";
			}
			return "";
		}
		int num = skillCheck.DCOverride;
		if (num == 0)
		{
			num = skillCheck.Settings.GetDC();
		}
		needChanceText = true;
		num = InteractionHelper.GetInteractionSkillCheckChance(skillCheck.SelectUnit(units), statType, num);
		return $"[{text}: {num}%]";
	}

	public static string GetTrapSkillCheckText(DisableTrapInteractionPart trap, List<BaseUnitEntity> units)
	{
		if (trap == null)
		{
			return string.Empty;
		}
		SimpleTrapObjectView simpleTrapObjectView = trap.View as SimpleTrapObjectView;
		SimpleTrapObjectInfo simpleTrapObjectInfo = simpleTrapObjectView?.Info;
		if (simpleTrapObjectInfo == null)
		{
			return string.Empty;
		}
		StatType disarmSkill = simpleTrapObjectInfo.DisarmSkill;
		string text = BlueprintRoot.Instance.LocalizedTexts.Stats.GetText(disarmSkill);
		int disableDC = simpleTrapObjectView.Data.DisableDC;
		int interactionSkillCheckChance = InteractionHelper.GetInteractionSkillCheckChance(trap.SelectUnit(units), disarmSkill, disableDC);
		return $"[{text}: {interactionSkillCheckChance}%]";
	}

	public static void SendWarning(string message)
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(message);
		});
		PFLog.UI.Log("Send UI Warning: " + message);
	}

	public static string GetHpText(MechanicEntityUIWrapper unit, bool isDead, int hpLeftSize = 80)
	{
		int num = (isDead ? unit.Attributes.WarhammerToughness.ModifiedValue : unit.Health.MaxHitPoints);
		int temporaryHitPoints = unit.Health.TemporaryHitPoints;
		int hitPointsLeft = unit.Health.HitPointsLeft;
		string arg = $"{num}";
		string arg2 = $"{hitPointsLeft + temporaryHitPoints}";
		return $"{arg2}/<size={hpLeftSize}%>{arg}";
	}

	public static string GetCharGenPortraitTabLabel(CharGenPortraitTab tab)
	{
		return tab switch
		{
			CharGenPortraitTab.Default => UIStrings.Instance.CharGen.PortraitCategoryDefault, 
			CharGenPortraitTab.Custom => UIStrings.Instance.CharGen.PortraitCategoryCustom, 
			_ => string.Empty, 
		};
	}

	public static string GetCharGenPortraitCategoryLabel(PortraitCategory category)
	{
		return category switch
		{
			PortraitCategory.None => string.Empty, 
			PortraitCategory.Warhammer => UIStrings.Instance.CharGen.PortraitCategoryWarhammer, 
			PortraitCategory.Navigator => UIStrings.Instance.CharGen.PortraitCategoryNavigator, 
			_ => string.Empty, 
		};
	}

	public static string GetProfitFactorFormatted(float value)
	{
		return value.ToString("0.#");
	}

	public static bool IsAnyKeyboardKeyDown()
	{
		if (!Input.GetKeyDown(KeyCode.Backspace) && !Input.GetKeyDown(KeyCode.Tab) && !Input.GetKeyDown(KeyCode.Clear) && !Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Pause) && !Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Exclaim) && !Input.GetKeyDown(KeyCode.DoubleQuote) && !Input.GetKeyDown(KeyCode.Hash) && !Input.GetKeyDown(KeyCode.Dollar) && !Input.GetKeyDown(KeyCode.Percent) && !Input.GetKeyDown(KeyCode.Ampersand) && !Input.GetKeyDown(KeyCode.Quote) && !Input.GetKeyDown(KeyCode.LeftParen) && !Input.GetKeyDown(KeyCode.RightParen) && !Input.GetKeyDown(KeyCode.Asterisk) && !Input.GetKeyDown(KeyCode.Plus) && !Input.GetKeyDown(KeyCode.Comma) && !Input.GetKeyDown(KeyCode.Minus) && !Input.GetKeyDown(KeyCode.Period) && !Input.GetKeyDown(KeyCode.Slash) && !Input.GetKeyDown(KeyCode.Alpha0) && !Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKeyDown(KeyCode.Alpha2) && !Input.GetKeyDown(KeyCode.Alpha3) && !Input.GetKeyDown(KeyCode.Alpha4) && !Input.GetKeyDown(KeyCode.Alpha5) && !Input.GetKeyDown(KeyCode.Alpha6) && !Input.GetKeyDown(KeyCode.Alpha7) && !Input.GetKeyDown(KeyCode.Alpha8) && !Input.GetKeyDown(KeyCode.Alpha9) && !Input.GetKeyDown(KeyCode.Colon) && !Input.GetKeyDown(KeyCode.Semicolon) && !Input.GetKeyDown(KeyCode.Less) && !Input.GetKeyDown(KeyCode.Equals) && !Input.GetKeyDown(KeyCode.Greater) && !Input.GetKeyDown(KeyCode.Question) && !Input.GetKeyDown(KeyCode.At) && !Input.GetKeyDown(KeyCode.LeftBracket) && !Input.GetKeyDown(KeyCode.Backslash) && !Input.GetKeyDown(KeyCode.RightBracket) && !Input.GetKeyDown(KeyCode.Caret) && !Input.GetKeyDown(KeyCode.Underscore) && !Input.GetKeyDown(KeyCode.BackQuote) && !Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.B) && !Input.GetKeyDown(KeyCode.C) && !Input.GetKeyDown(KeyCode.D) && !Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.F) && !Input.GetKeyDown(KeyCode.G) && !Input.GetKeyDown(KeyCode.H) && !Input.GetKeyDown(KeyCode.I) && !Input.GetKeyDown(KeyCode.J) && !Input.GetKeyDown(KeyCode.K) && !Input.GetKeyDown(KeyCode.L) && !Input.GetKeyDown(KeyCode.M) && !Input.GetKeyDown(KeyCode.N) && !Input.GetKeyDown(KeyCode.O) && !Input.GetKeyDown(KeyCode.P) && !Input.GetKeyDown(KeyCode.Q) && !Input.GetKeyDown(KeyCode.R) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.T) && !Input.GetKeyDown(KeyCode.U) && !Input.GetKeyDown(KeyCode.V) && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.X) && !Input.GetKeyDown(KeyCode.Y) && !Input.GetKeyDown(KeyCode.Z) && !Input.GetKeyDown(KeyCode.LeftCurlyBracket) && !Input.GetKeyDown(KeyCode.Pipe) && !Input.GetKeyDown(KeyCode.RightCurlyBracket) && !Input.GetKeyDown(KeyCode.Tilde) && !Input.GetKeyDown(KeyCode.Delete) && !Input.GetKeyDown(KeyCode.Keypad0) && !Input.GetKeyDown(KeyCode.Keypad1) && !Input.GetKeyDown(KeyCode.Keypad2) && !Input.GetKeyDown(KeyCode.Keypad3) && !Input.GetKeyDown(KeyCode.Keypad4) && !Input.GetKeyDown(KeyCode.Keypad5) && !Input.GetKeyDown(KeyCode.Keypad6) && !Input.GetKeyDown(KeyCode.Keypad7) && !Input.GetKeyDown(KeyCode.Keypad8) && !Input.GetKeyDown(KeyCode.Keypad9) && !Input.GetKeyDown(KeyCode.KeypadPeriod) && !Input.GetKeyDown(KeyCode.KeypadDivide) && !Input.GetKeyDown(KeyCode.KeypadMultiply) && !Input.GetKeyDown(KeyCode.KeypadMinus) && !Input.GetKeyDown(KeyCode.KeypadPlus) && !Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetKeyDown(KeyCode.KeypadEquals) && !Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKeyDown(KeyCode.DownArrow) && !Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.Insert) && !Input.GetKeyDown(KeyCode.Home) && !Input.GetKeyDown(KeyCode.End) && !Input.GetKeyDown(KeyCode.PageUp) && !Input.GetKeyDown(KeyCode.PageDown) && !Input.GetKeyDown(KeyCode.F1) && !Input.GetKeyDown(KeyCode.F2) && !Input.GetKeyDown(KeyCode.F3) && !Input.GetKeyDown(KeyCode.F4) && !Input.GetKeyDown(KeyCode.F5) && !Input.GetKeyDown(KeyCode.F6) && !Input.GetKeyDown(KeyCode.F7) && !Input.GetKeyDown(KeyCode.F8) && !Input.GetKeyDown(KeyCode.F9) && !Input.GetKeyDown(KeyCode.F10) && !Input.GetKeyDown(KeyCode.F11) && !Input.GetKeyDown(KeyCode.F12) && !Input.GetKeyDown(KeyCode.F13) && !Input.GetKeyDown(KeyCode.F14) && !Input.GetKeyDown(KeyCode.F15) && !Input.GetKeyDown(KeyCode.Numlock) && !Input.GetKeyDown(KeyCode.CapsLock) && !Input.GetKeyDown(KeyCode.ScrollLock) && !Input.GetKeyDown(KeyCode.RightShift) && !Input.GetKeyDown(KeyCode.LeftShift) && !Input.GetKeyDown(KeyCode.RightControl) && !Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.RightAlt))
		{
			return Input.GetKeyDown(KeyCode.LeftAlt);
		}
		return true;
	}

	public static IDisposable SetTextLink(TextMeshProUGUI text, Camera camera = null)
	{
		if (text == null)
		{
			return Disposable.Empty;
		}
		bool entered = false;
		int? linkIndex = null;
		string[] linkKeys = null;
		IDisposable enter = UniRxExtensionMethods.OnPointerEnterAsObservable(text).Subscribe(delegate
		{
			entered = true;
		});
		IDisposable exit = UniRxExtensionMethods.OnPointerExitAsObservable(text).Subscribe(delegate
		{
			entered = false;
		});
		IDisposable update = ObservableExtensions.Subscribe(text.UpdateAsObservable(), delegate
		{
			int num;
			if (!entered || (num = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, (camera != null) ? camera : UICamera.Claim())) == -1)
			{
				if (linkIndex.HasValue)
				{
					linkIndex = null;
					linkKeys = null;
				}
			}
			else if (num != linkIndex)
			{
				linkIndex = num;
				linkKeys = GetKeysFromLink(text.textInfo.linkInfo[linkIndex.Value].GetLinkID());
			}
		});
		IDisposable click = UniRxExtensionMethods.OnPointerClickAsObservable(text).Subscribe(delegate(PointerEventData data)
		{
			if (linkKeys.Any() && data.button == PointerEventData.InputButton.Left)
			{
				string text2 = linkKeys[0];
				if (text2 == "http" || text2 == "https")
				{
					text2 = linkKeys[1];
				}
				if (text2.Contains("://") && !text2.Contains("http"))
				{
					text2 = "http" + text2;
				}
				else if (!text2.Contains("http://") && !text2.Contains("https://"))
				{
					text2 = "http://" + text2;
				}
				Application.OpenURL(text2);
			}
		});
		return Disposable.Create(delegate
		{
			if (entered)
			{
				entered = false;
			}
			enter?.Dispose();
			exit?.Dispose();
			update?.Dispose();
			click?.Dispose();
		});
	}

	public static TextAlignmentOptions TextAnchorToTextAlignmentOptions(TextAnchor textAnchor)
	{
		switch (textAnchor)
		{
		case TextAnchor.UpperLeft:
			return TextAlignmentOptions.TopLeft;
		case TextAnchor.UpperCenter:
			return TextAlignmentOptions.Top;
		case TextAnchor.UpperRight:
			return TextAlignmentOptions.TopRight;
		case TextAnchor.MiddleLeft:
			return TextAlignmentOptions.Left;
		case TextAnchor.MiddleCenter:
			return TextAlignmentOptions.Center;
		case TextAnchor.MiddleRight:
			return TextAlignmentOptions.Right;
		case TextAnchor.LowerLeft:
			return TextAlignmentOptions.BottomLeft;
		case TextAnchor.LowerCenter:
			return TextAlignmentOptions.Bottom;
		case TextAnchor.LowerRight:
			return TextAlignmentOptions.BottomRight;
		default:
			Debug.LogWarning("Unhandled text anchor " + textAnchor);
			return TextAlignmentOptions.TopLeft;
		}
	}

	public static TextAnchor TextAlignmentOptionsToTextAnchor(TextAlignmentOptions textAlignmentOptions)
	{
		switch (textAlignmentOptions)
		{
		case TextAlignmentOptions.TopLeft:
			return TextAnchor.UpperLeft;
		case TextAlignmentOptions.Top:
			return TextAnchor.UpperCenter;
		case TextAlignmentOptions.TopRight:
			return TextAnchor.UpperRight;
		case TextAlignmentOptions.Left:
			return TextAnchor.MiddleLeft;
		case TextAlignmentOptions.Center:
			return TextAnchor.MiddleCenter;
		case TextAlignmentOptions.Right:
			return TextAnchor.MiddleRight;
		case TextAlignmentOptions.BottomLeft:
			return TextAnchor.LowerLeft;
		case TextAlignmentOptions.Bottom:
			return TextAnchor.LowerCenter;
		case TextAlignmentOptions.BottomRight:
			return TextAnchor.LowerRight;
		default:
			Debug.LogWarning("Unhandled text alignment options " + textAlignmentOptions);
			return TextAnchor.UpperLeft;
		}
	}

	public static void FindMinMaxPositions(in List<UIVertex> vertexStream, out Vector3 min, out Vector3 max, out float width, out float height, bool useSorting = false)
	{
		if (vertexStream == null || vertexStream.Count <= 1)
		{
			min = Vector3.zero;
			max = Vector3.zero;
			width = 0f;
			height = 0f;
			return;
		}
		if (!useSorting)
		{
			min = vertexStream[0].position;
			max = vertexStream[vertexStream.Count - 1].position;
		}
		else
		{
			min = vertexStream.MinBy((UIVertex vert) => vert.position.x + vert.position.y).position;
			max = vertexStream.MaxBy((UIVertex vert) => vert.position.x + vert.position.y).position;
		}
		width = max.x - min.x;
		height = max.y - min.y;
	}

	public static StatType? TryGetStatType(string key)
	{
		try
		{
			return (StatType)Enum.Parse(typeof(StatType), key);
		}
		catch (Exception)
		{
			return null;
		}
	}
}
