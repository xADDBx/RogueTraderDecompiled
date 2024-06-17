using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Kingmaker.UnitLogic.Alignments;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment;

public class CharInfoAlignmentVM : CharInfoComponentVM
{
	public readonly ConvictionBarVM ConvictionBar;

	public readonly CharInfoSoulMarksSectorVM FaithSector;

	public readonly CharInfoSoulMarksSectorVM CorruptionSector;

	public readonly CharInfoSoulMarksSectorVM HopeSector;

	private readonly bool m_Initialized;

	private readonly List<CharInfoSoulMarksSectorVM> m_Sectors;

	public const int MaxUnlockedTierId = 2;

	private SoulMarkDirection? m_MainDirection;

	public CharInfoAlignmentVM(IReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		AddDisposable(ConvictionBar = new ConvictionBarVM(unit));
		AddDisposable(FaithSector = new CharInfoSoulMarksSectorVM(unit, SoulMarkDirection.Faith));
		AddDisposable(CorruptionSector = new CharInfoSoulMarksSectorVM(unit, SoulMarkDirection.Corruption));
		AddDisposable(HopeSector = new CharInfoSoulMarksSectorVM(unit, SoulMarkDirection.Hope));
		m_Sectors = new List<CharInfoSoulMarksSectorVM> { FaithSector, CorruptionSector, HopeSector };
		UpdateMainDirection();
		m_Initialized = true;
	}

	protected override void DisposeImplementation()
	{
	}

	protected override void RefreshData()
	{
		if (m_Initialized)
		{
			base.RefreshData();
			FaithSector.UpdateSoulMarkInfo();
			CorruptionSector.UpdateSoulMarkInfo();
			HopeSector.UpdateSoulMarkInfo();
			UpdateMainDirection();
		}
	}

	private void UpdateMainDirection()
	{
		if (m_Sectors == null)
		{
			return;
		}
		m_MainDirection = GetUnlockedDirection();
		foreach (CharInfoSoulMarksSectorVM sector in m_Sectors)
		{
			sector.UpdateMainDirection(m_MainDirection);
		}
	}

	private SoulMarkDirection? GetUnlockedDirection()
	{
		if (m_Sectors == null)
		{
			return null;
		}
		foreach (CharInfoSoulMarksSectorVM sectorVM in m_Sectors)
		{
			if (Unit.Value.Facts.List.FirstOrDefault((EntityFact f) => f.Blueprint == sectorVM.SoulMarks[2]) != null)
			{
				return sectorVM.Direction;
			}
		}
		return null;
	}
}
