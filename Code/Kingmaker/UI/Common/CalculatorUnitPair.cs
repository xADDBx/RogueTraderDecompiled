using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.StatefulRandom;
using UniRx;

namespace Kingmaker.UI.Common;

public class CalculatorUnitPair : IDisposable, ILevelUpCompleteUIHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private readonly ReactiveProperty<BaseUnitEntity> m_SelectedUnit;

	private readonly CompositeDisposable m_Disposables = new CompositeDisposable();

	public BaseUnitEntity CalculatorUnit { get; private set; }

	public BaseUnitEntity CurrentSelectedUnit => m_SelectedUnit.Value;

	public CalculatorUnitPair(ReactiveProperty<BaseUnitEntity> selectedUnit)
	{
		m_SelectedUnit = selectedUnit;
		m_Disposables.Add(m_SelectedUnit.Subscribe(delegate
		{
			UpdateCalculator();
		}));
		m_Disposables.Add(EventBus.Subscribe(this));
	}

	private void UpdateCalculator()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					CalculatorUnit?.Dispose();
					using (ContextData<ItemSlot.IgnoreLock>.Request())
					{
						using (ContextData<GameCommandHelper.PreviewItem>.Request())
						{
							BaseUnitEntity @this = m_SelectedUnit.Value ?? Game.Instance.Player.MainCharacterEntity;
							CalculatorUnit = @this.CreatePreview(createView: false, forceEnableBuffs: true);
						}
					}
				}
			}
		}
	}

	public void Dispose()
	{
		CalculatorUnit?.Dispose();
		m_Disposables?.Dispose();
	}

	public void HandleLevelUpComplete(bool isChargen)
	{
		UpdateCalculator();
	}
}
