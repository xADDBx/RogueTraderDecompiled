using System;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;

public class CharInfoSoulMarkShiftRecordVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly SoulMarkShift m_SoulMarkShift;

	public LocalizedString Description => m_SoulMarkShift.Description;

	public SoulMarkDirection Direction => m_SoulMarkShift.Direction;

	public int Amount => m_SoulMarkShift.Value;

	public CharInfoSoulMarkShiftRecordVM(SoulMarkShift soulMarkShift)
	{
		m_SoulMarkShift = soulMarkShift;
	}

	protected override void DisposeImplementation()
	{
	}
}
