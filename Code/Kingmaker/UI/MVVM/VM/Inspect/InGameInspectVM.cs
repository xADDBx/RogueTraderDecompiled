using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Inspect;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Inspect;

public class InGameInspectVM : InspectVM
{
	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>();

	private readonly InspectReactiveData m_InspectReactiveData = new InspectReactiveData();

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	private UnitInspectInfoByPart m_InspectInfo;

	private IDisposable m_Disposable;

	protected override void HideInspect()
	{
		base.HideInspect();
		m_Disposable?.Dispose();
	}

	protected override void OnUnitInvoke(AbstractUnitEntity entity)
	{
		BaseUnitEntity baseUnitEntity = entity as BaseUnitEntity;
		if (baseUnitEntity == null)
		{
			return;
		}
		m_Unit.Value = baseUnitEntity;
		Game.Instance.Player.UISettings.ShowInspect = true;
		if (Game.Instance.CurrentMode == GameModeType.SpaceCombat)
		{
			Tooltip.Value = new TooltipTemplateSpaceUnitInspect(baseUnitEntity);
			return;
		}
		m_Disposable?.Dispose();
		m_Disposable = MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateInspect(baseUnitEntity);
		});
		UpdateInspect(baseUnitEntity);
		Tooltip.Value = new TooltipTemplateUnitInspect(m_Unit, m_InspectReactiveData);
	}

	private void UpdateInspect(BaseUnitEntity unit)
	{
		bool num = m_UnitUIWrapper.MechanicEntity != unit;
		if (num)
		{
			m_UnitUIWrapper = new MechanicEntityUIWrapper(unit);
		}
		if (num || m_InspectInfo?.DefencePart == null)
		{
			m_InspectInfo = InspectUnitsHelper.GetInfo(unit.BlueprintForInspection, force: true);
		}
		UpdateWounds();
		UpdateDeflection(unit);
		UpdateArmor(unit);
		UpdateDodge(unit);
		UpdateMovementPoints(unit);
		UpdateBuffs(unit);
	}

	protected virtual void UpdateWounds()
	{
		if (m_InspectInfo.DefencePart != null && InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue, out var woundsAddValue))
		{
			m_InspectReactiveData.WoundsValue.Value = woundsValue;
			m_InspectReactiveData.WoundsAddValue.Value = woundsAddValue;
		}
	}

	protected virtual void UpdateDeflection(BaseUnitEntity unit)
	{
		m_InspectReactiveData.DeflectionValue.Value = InspectExtensions.GetDeflection(unit);
	}

	protected virtual void UpdateArmor(BaseUnitEntity unit)
	{
		m_InspectReactiveData.ArmorValue.Value = InspectExtensions.GetArmor(unit);
	}

	protected virtual void UpdateDodge(BaseUnitEntity unit)
	{
		m_InspectReactiveData.DodgeValue.Value = InspectExtensions.GetDodge(unit);
	}

	protected virtual void UpdateMovementPoints(BaseUnitEntity unit)
	{
		m_InspectReactiveData.MovementPointsValue.Value = InspectExtensions.GetMovementPoints(unit);
	}

	private void UpdateBuffs(BaseUnitEntity unit)
	{
		List<TooltipBrickBuff> buffs = InspectExtensions.GetBuffs(unit);
		bool flag = false;
		if (m_InspectReactiveData.TooltipBrickBuffs.Count != buffs.Count)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < m_InspectReactiveData.TooltipBrickBuffs.Count; i++)
			{
				TooltipBrickBuff obj = m_InspectReactiveData.TooltipBrickBuffs[i] as TooltipBrickBuff;
				TooltipBrickBuff tooltipBrickBuff = buffs[i];
				if (obj.Buff != tooltipBrickBuff.Buff)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			m_InspectReactiveData.TooltipBrickBuffs.Clear();
			buffs.ForEach(delegate(TooltipBrickBuff buff)
			{
				m_InspectReactiveData.TooltipBrickBuffs.Add(buff);
			});
		}
	}
}
