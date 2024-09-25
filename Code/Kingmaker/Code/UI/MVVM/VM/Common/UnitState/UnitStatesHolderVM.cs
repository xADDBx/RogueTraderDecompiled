using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Common.UnitState;

public class UnitStatesHolderVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Dictionary<MechanicEntity, UnitState> m_UnitStates = new Dictionary<MechanicEntity, UnitState>();

	public static UnitStatesHolderVM Instance => Game.Instance.RootUiContext.CommonVM.UnitStatesHolderVM;

	protected override void DisposeImplementation()
	{
		Clear();
	}

	public UnitState GetOrCreateUnitState(MechanicEntity unitEntity)
	{
		if (unitEntity == null)
		{
			UberDebug.LogError("UnitState: Unit is null");
			return null;
		}
		if (!m_UnitStates.TryGetValue(unitEntity, out var value))
		{
			value = new UnitState(unitEntity);
			AddDisposable(value);
			m_UnitStates[unitEntity] = value;
		}
		return value;
	}

	public void RemoveUnitState(MechanicEntity unitEntity)
	{
		if (unitEntity != null && !m_UnitStates.Empty() && m_UnitStates.TryGetValue(unitEntity, out var value))
		{
			value.Dispose();
			m_UnitStates.Remove(unitEntity);
		}
	}

	public void RemoveUnitState(string uniqueId)
	{
		if (string.IsNullOrEmpty(uniqueId) || m_UnitStates.Empty())
		{
			return;
		}
		foreach (KeyValuePair<MechanicEntity, UnitState> unitState in m_UnitStates)
		{
			if (unitState.Key.UniqueId == uniqueId)
			{
				unitState.Value.Dispose();
				m_UnitStates.Remove(unitState.Key);
				break;
			}
		}
	}

	public void Clear()
	{
		m_UnitStates.Values.ForEach(delegate(UnitState unitState)
		{
			unitState.Dispose();
		});
		m_UnitStates.Clear();
	}
}
