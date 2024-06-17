namespace Owlcat.Runtime.UI.Utility;

public interface IWidget
{
	void OnWidgetInstantiated();

	void OnWidgetTaken();

	void OnWidgetReturned();
}
