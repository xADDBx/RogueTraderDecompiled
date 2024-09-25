using System;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap.Collections;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

public class SectorMapOvertipsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly SystemOvertipsCollectionVM SystemOvertipsCollectionVM;

	public readonly RumoursOvertipsCollectionVM RumoursOvertipsCollectionVM;

	public readonly ReactiveProperty<bool> BlockAllPopups = new ReactiveProperty<bool>();

	public readonly ReactiveCommand CloseAllPopups = new ReactiveCommand();

	public readonly ReactiveProperty<bool> CloseAllPopupsCanvas = new ReactiveProperty<bool>(initialValue: false);

	public static SectorMapOvertipsVM Instance;

	public SectorMapOvertipsVM()
	{
		Instance = this;
		AddDisposable(SystemOvertipsCollectionVM = new SystemOvertipsCollectionVM());
		AddDisposable(RumoursOvertipsCollectionVM = new RumoursOvertipsCollectionVM());
	}

	protected override void DisposeImplementation()
	{
		Instance = null;
		ClosePopupsCanvas(state: false);
	}

	public void BlockPopups(bool state)
	{
		BlockAllPopups.Value = state;
	}

	public void ClosePopupsCanvas(bool state)
	{
		CloseAllPopupsCanvas.Value = state;
	}

	public void ClosePopups()
	{
		CloseAllPopups.Execute();
	}
}
