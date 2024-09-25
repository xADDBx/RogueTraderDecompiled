namespace Kingmaker.UI.SurfaceCombatHUD;

internal interface ISplinePlotterListener<TSplineMetaData> where TSplineMetaData : unmanaged
{
	void StartLine();

	void PushPoint(in SplinePoint point);

	void FinishLine(in TSplineMetaData metaData);
}
