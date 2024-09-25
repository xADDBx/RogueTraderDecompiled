using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;

public abstract class SequentialSelectorVM<TEntity> : BaseCharGenAppearancePageComponentVM, INetLobbyPlayersHandler, ISubscriber where TEntity : SequentialEntity
{
	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	protected readonly List<TEntity> ValueList = new List<TEntity>();

	private readonly ReactiveProperty<int> m_CurrentIndex = new ReactiveProperty<int>(-1);

	public readonly ReactiveCommand ValuesUpdated = new ReactiveCommand();

	private readonly bool m_Cyclical;

	private Action<Action> m_PreMoveCheck;

	public readonly ReactiveCommand<bool> CheckCoopControls = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	public int TotalCount => ValueList.Count;

	public IReadOnlyReactiveProperty<string> Title => m_Title;

	public IReadOnlyReactiveProperty<int> CurrentIndex => m_CurrentIndex;

	protected SequentialSelectorVM(bool cyclical = true)
	{
		m_Cyclical = cyclical;
		AddDisposable(m_CurrentIndex.Subscribe(delegate(int value)
		{
			if (value >= 0 && ValueList.Count != 0)
			{
				if (UINetUtility.IsControlMainCharacter())
				{
					ValueList[m_CurrentIndex.Value].Setter?.Invoke();
					Changed();
				}
				SetCurrentEntity();
			}
		}));
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected SequentialSelectorVM(List<TEntity> valueList, TEntity current = null, bool cyclical = true)
		: this(cyclical)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		SetValues(valueList, current);
		UILog.VMCreated("SequentialSelectorVM");
	}

	protected override void DisposeImplementation()
	{
		UILog.VMDisposed("SequentialSelectorVM");
	}

	protected abstract void SetCurrentEntity();

	public void SetPreMoveCheck(Action<Action> preMoveCheck)
	{
		m_PreMoveCheck = preMoveCheck;
	}

	public void SetValues(List<TEntity> valueList, TEntity current = null)
	{
		m_IsAvailable.Value = valueList != null && valueList.Count > 1;
		ValueList.Clear();
		if (m_IsAvailable.Value)
		{
			ValueList.AddRange(valueList);
			m_CurrentIndex.Value = ((current != null && ValueList.Contains(current)) ? ValueList.FindIndex((TEntity v) => v == current) : 0);
			ValuesUpdated.Execute();
		}
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void OnLeft()
	{
		if (IsMainCharacter.Value)
		{
			if (m_PreMoveCheck != null)
			{
				m_PreMoveCheck(OnLeftInternal);
			}
			else
			{
				OnLeftInternal();
			}
		}
	}

	private void OnLeftInternal()
	{
		int value = m_CurrentIndex.Value;
		m_CurrentIndex.Value = ((value > 0) ? (value - 1) : (m_Cyclical ? (ValueList.Count - 1) : 0));
		UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
	}

	public void OnRight()
	{
		if (IsMainCharacter.Value)
		{
			if (m_PreMoveCheck != null)
			{
				m_PreMoveCheck(OnRightInternal);
			}
			else
			{
				OnRightInternal();
			}
		}
	}

	private void OnRightInternal()
	{
		int value = m_CurrentIndex.Value;
		m_CurrentIndex.Value = ((value < ValueList.Count - 1) ? (value + 1) : ((!m_Cyclical) ? (ValueList.Count - 1) : 0));
		UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
	}

	public bool SetCurrentIndex(int index)
	{
		if (index < 0 || index >= ValueList.Count)
		{
			return false;
		}
		m_CurrentIndex.Value = index;
		return true;
	}

	public bool IsValid()
	{
		return ValueList.Count > 1;
	}

	protected override void SetSelectUIHead(EquipmentEntityLink head, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.FaceType && !IsMainCharacter.Value)
		{
			SetCurrentIndex(index);
		}
	}

	protected override void SetSelectUIRace(BlueprintRaceVisualPreset blueprint, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.BodyType && !IsMainCharacter.Value)
		{
			SetCurrentIndex(index);
		}
	}

	protected override void SetUIScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.ScarsType && !IsMainCharacter.Value)
		{
			SetCurrentIndex(index);
		}
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		CheckCoopControls.Execute(UINetUtility.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
