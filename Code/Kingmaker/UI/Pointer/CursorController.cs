using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class CursorController : IFocusHandler, ISubscriber, IAbilityTargetSelectionUIHandler, IInteractionHighlightUIHandler, IPartyCharacterHoverHandler
{
	private MapObjectView m_MapObjectView;

	private AbstractUnitEntity m_BaseUnitEntity;

	private bool m_Locked;

	private bool m_OnGui;

	private bool m_PortraitHover;

	private BaseUnitEntity m_PortraitHoveredUnit;

	private CompositeDisposable m_Disposable;

	private string m_UpperTextAP;

	private string m_LowerTextMP;

	public AbilityData SelectedAbility { get; private set; }

	public bool CastMode => SelectedAbility != null;

	private BaseCursor CurrentCursor
	{
		get
		{
			if (!Game.Instance.IsControllerMouse)
			{
				return ConsoleCursor.Instance;
			}
			return PCCursor.Instance;
		}
	}

	public Vector2 CursorPosition => CurrentCursor.Or(null)?.Position ?? ((Vector2)Input.mousePosition);

	public void Activate()
	{
		EventBus.Subscribe(this);
		UpdateCursorMode();
		m_Disposable = new CompositeDisposable();
		m_Disposable.Add(MainThreadDispatcher.UpdateAsObservable().Subscribe(OnUpdate));
		m_Disposable.Add(UIVisibilityState.VisibilityPreset.Subscribe(delegate
		{
			CurrentCursor.Or(null)?.SetActive(CurrentCursor.PrevActiveCursorState);
		}));
	}

	public void Deactivate()
	{
		EventBus.Unsubscribe(this);
		m_Disposable?.Dispose();
		SelectedAbility = null;
	}

	public void SetActive(bool active)
	{
		CurrentCursor.Or(null)?.SetActive(active);
	}

	public void SetCursor(CursorType type, bool force = false)
	{
		if ((!m_Locked && !m_OnGui) || force)
		{
			CurrentCursor.Or(null)?.SetCursor(type);
		}
	}

	public void SetLock(bool @lock)
	{
		m_Locked = @lock;
		if (m_Locked)
		{
			ClearComponents();
		}
	}

	public void ClearCursor(bool force = false)
	{
		if ((!m_Locked && !m_OnGui) || force)
		{
			if (CastMode)
			{
				SetAbilityCursor();
			}
			else
			{
				SetCursor(CursorType.Default, force);
			}
		}
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		SelectedAbility = ability;
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		SelectedAbility = null;
	}

	public void SetAbilityCursor(Sprite abilityIcon)
	{
		if (!m_Locked)
		{
			CurrentCursor.Or(null)?.SetAbilityCursor(abilityIcon);
		}
	}

	public void SetAbilityCursor()
	{
		if (!m_Locked)
		{
			CurrentCursor.Or(null)?.SetAbilityCursor(SelectedAbility.Icon);
		}
	}

	public void SetTexts_APMP(string upperText, string lowerText, bool force = false)
	{
		m_UpperTextAP = upperText;
		m_LowerTextMP = lowerText;
		SetTextsInternal(m_UpperTextAP, m_LowerTextMP, force);
	}

	public void SetTexts_UnavailabilityReason(string upperText)
	{
		SetTextsInternal(upperText, "", force: true);
	}

	public void ClearTexts_UnavailabilityReason()
	{
		SetTextsInternal(m_UpperTextAP, m_LowerTextMP, force: true);
	}

	private void SetTextsInternal(string upperText, string lowerText, bool force = false)
	{
		if ((m_Locked || m_OnGui) && !force)
		{
			CurrentCursor.Or(null)?.SetTexts(null, null);
		}
		else
		{
			CurrentCursor.Or(null)?.SetTexts(upperText, lowerText);
		}
	}

	public void SetNoMoveIcon(bool noMove, bool force = false)
	{
		if ((m_Locked || m_OnGui) && !force)
		{
			CurrentCursor.Or(null)?.SetNoMove(noMove: false);
		}
		else
		{
			CurrentCursor.Or(null)?.SetNoMove(noMove);
		}
	}

	public void ClearComponents()
	{
		CurrentCursor.Or(null)?.ClearComponents();
	}

	private void UpdateCursorMode(bool forceFocused = false)
	{
		Cursor.lockState = CursorLockMode.None;
		if ((Application.isFocused || forceFocused) && (SettingsRoot.Graphics.FullScreenMode.GetValue() == FullScreenMode.ExclusiveFullScreen || (bool)SettingsRoot.Graphics.WindowedCursorLock))
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
	}

	void IFocusHandler.OnApplicationFocusChanged(bool isFocused)
	{
		if (!ApplicationFocusEvents.CursorDisabled)
		{
			if (isFocused)
			{
				UpdateCursorMode(forceFocused: true);
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		if (Game.Instance.Player.IsInCombat)
		{
			SetLock(isOn);
			if (isOn)
			{
				SetCursor(CursorType.Info, force: true);
			}
			else
			{
				ClearCursor(force: true);
			}
		}
	}

	public void SetUnitCursor(AbstractUnitEntity unit, bool isHighlighted)
	{
		if (unit == null)
		{
			return;
		}
		m_BaseUnitEntity = (isHighlighted ? unit : null);
		IAbstractUnitEntity entity = Game.Instance.Player.MainCharacter.Entity;
		if (!CastMode)
		{
			if (!isHighlighted)
			{
				SetCursor(CursorType.Default);
			}
			else if (m_BaseUnitEntity.IsDirectlyControllable)
			{
				SetCursor(CursorType.Default);
			}
			else if (m_BaseUnitEntity.IsDeadAndHasLoot && !TurnController.IsInTurnBasedCombat())
			{
				SetCursor(CursorType.Loot);
			}
			else if (m_BaseUnitEntity.LifeState.IsFinallyDead && Game.Instance.Player.UISettings.ShowInspect)
			{
				SetCursor(CursorType.Info);
			}
			else if (TurnController.IsInTurnBasedCombat())
			{
				SetCursor(CursorType.Default);
			}
			else if (m_BaseUnitEntity.SelectClickInteraction((BaseUnitEntity)entity) != null)
			{
				SetCursor(CursorType.Dialog);
			}
		}
	}

	public void SetMapObjectCursor(MapObjectView mapObjectView, bool isHighlighted)
	{
		m_MapObjectView = (isHighlighted ? mapObjectView : null);
		if (CastMode)
		{
			return;
		}
		if (!isHighlighted)
		{
			SetCursor(CursorType.Default);
			return;
		}
		if (mapObjectView is TrapObjectView)
		{
			SetCursor(CursorType.Trap);
			return;
		}
		InteractionPart interactionPart = mapObjectView.Data.Parts.GetAll<InteractionPart>().FirstOrDefault((InteractionPart i) => i.Enabled);
		InteractionPart interactionPart2 = interactionPart;
		if (!(interactionPart2 is InteractionLootPart) && !(interactionPart2 is InteractionDoorPart))
		{
			if (interactionPart2 is InteractionSkillCheckPart interactionSkillCheckPart)
			{
				if (interactionSkillCheckPart.InteractThroughVariants)
				{
					goto IL_00a2;
				}
			}
			else if (interactionPart2 is InteractionDevicePart && !interactionPart.Settings.ShowOvertip)
			{
				SetCursor(CursorType.Gear);
				return;
			}
			if (interactionPart != null)
			{
				switch (interactionPart.UIInteractionType)
				{
				case UIInteractionType.None:
					SetCursor(CursorType.Gear);
					break;
				case UIInteractionType.Action:
				case UIInteractionType.Credits:
					SetCursor(CursorType.Gear);
					break;
				case UIInteractionType.Move:
					SetCursor(CursorType.Interact);
					break;
				case UIInteractionType.Info:
					SetCursor(CursorType.Bark);
					SetCursor(CursorType.Bark);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			return;
		}
		goto IL_00a2;
		IL_00a2:
		if ((bool)mapObjectView.Data.GetOptional<TechUseRestrictionPart>() || (bool)mapObjectView.Data.GetOptional<LoreXenosRestrictionPart>())
		{
			if (interactionPart.AlreadyUnlocked || interactionPart is InteractionDoorPart { IsOpen: not false })
			{
				SetCursor(CursorType.Unlock);
			}
			else
			{
				SetCursor(CursorType.Lock);
			}
		}
		else if (interactionPart is InteractionDoorPart)
		{
			SetCursor(CursorType.Interact);
		}
		else if (interactionPart is InteractionSkillCheckPart)
		{
			SetCursor(CursorType.Bark);
		}
		else
		{
			SetLootCursor();
		}
	}

	private void SetLootCursor()
	{
		SetCursor((!TurnController.IsInTurnBasedCombat()) ? CursorType.Loot : CursorType.Default);
	}

	public void SetMoveCursor(bool state, bool forbidden = false)
	{
		if (CastMode)
		{
			return;
		}
		if (forbidden)
		{
			SetCursor(CursorType.Restricted);
		}
		else if (!m_MapObjectView && m_BaseUnitEntity == null)
		{
			if (state)
			{
				SetCursor(Game.Instance.IsControllerMouse ? CursorType.Move : CursorType.ConsoleMove);
			}
			else
			{
				ClearCursor();
			}
		}
	}

	public void SetRotateCameraCursor(bool state)
	{
		if (!CastMode)
		{
			if (state)
			{
				SetCursor(CursorType.RotateCamera, force: true);
			}
			else
			{
				ClearCursor(force: true);
			}
		}
	}

	private void OnUpdate()
	{
		if (m_OnGui != PointerController.InGui)
		{
			OnGuiChanged(PointerController.InGui);
		}
		if (SelectedAbility == null)
		{
			return;
		}
		InteractionHighlightController instance = InteractionHighlightController.Instance;
		if (instance == null || !instance.IsHighlighting)
		{
			TargetWrapper targetForDesiredPosition;
			if (!m_PortraitHover)
			{
				PointerController clickEventsController = Game.Instance.ClickEventsController;
				targetForDesiredPosition = Game.Instance.SelectedAbilityHandler.GetTargetForDesiredPosition(clickEventsController.PointerOn, clickEventsController.WorldPosition);
			}
			else
			{
				targetForDesiredPosition = Game.Instance.SelectedAbilityHandler.GetTargetForDesiredPosition(m_PortraitHoveredUnit.View.gameObject, m_PortraitHoveredUnit.View.transform.position);
			}
			if (CheckTarget(SelectedAbility, targetForDesiredPosition))
			{
				SetAbilityCursor();
			}
			else
			{
				SetCursor(CursorType.CastRestricted, force: true);
			}
		}
	}

	private bool CheckTarget(AbilityData ability, TargetWrapper target)
	{
		if (ability == null || target == null)
		{
			return false;
		}
		AbilityData.UnavailabilityReasonType? unavailabilityReason;
		bool num = SelectedAbility.CanTargetFromDesiredPosition(target, out unavailabilityReason);
		bool flag = !Game.Instance.Player.IsInCombat && unavailabilityReason == AbilityData.UnavailabilityReasonType.TargetTooFar;
		if (num || flag)
		{
			ClearTexts_UnavailabilityReason();
			return true;
		}
		if (ability.IsCharge)
		{
			CustomGridNodeBase bestShootingPositionForDesiredPosition = ability.GetBestShootingPositionForDesiredPosition(target);
			string texts_UnavailabilityReason = (unavailabilityReason.HasValue ? ability.GetUnavailabilityReasonString(unavailabilityReason.Value, bestShootingPositionForDesiredPosition.Vector3Position, target) : ((string)LocalizedTexts.Instance.Reasons.UnavailableGeneric));
			SetTexts_UnavailabilityReason(texts_UnavailabilityReason);
		}
		return false;
	}

	private void OnGuiChanged(bool newValue)
	{
		bool flag;
		if (newValue)
		{
			CursorType? cursorType = CurrentCursor.Or(null)?.CurrentType;
			if (cursorType.HasValue)
			{
				CursorType valueOrDefault = cursorType.GetValueOrDefault();
				if ((uint)(valueOrDefault - 14) <= 1u)
				{
					flag = true;
					goto IL_0047;
				}
			}
			flag = false;
			goto IL_0047;
		}
		m_OnGui = false;
		ClearCursor();
		goto IL_006f;
		IL_0047:
		if (flag)
		{
			SetCursor(CursorType.Default);
			ClearComponents();
		}
		m_OnGui = true;
		goto IL_006f;
		IL_006f:
		CurrentCursor.Or(null)?.OnGuiChanged(m_OnGui);
	}

	public void HandlePartyCharacterHover(BaseUnitEntity unit, bool hover)
	{
		m_PortraitHover = hover;
		m_PortraitHoveredUnit = unit;
	}
}
