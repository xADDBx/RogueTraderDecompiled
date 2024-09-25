using System;
using Kingmaker.Globalmap.Colonization;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class ColonyUIComponentVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IColonyUIComponent
{
	protected Colony m_Colony;

	protected bool m_IsColonyManagement;

	protected override void DisposeImplementation()
	{
	}

	public void SetColony(Colony colony, bool isColonyManagement = false)
	{
		m_Colony = colony;
		m_IsColonyManagement = isColonyManagement;
		SetColonyImpl(colony);
	}

	protected virtual void SetColonyImpl(Colony colony)
	{
	}
}
