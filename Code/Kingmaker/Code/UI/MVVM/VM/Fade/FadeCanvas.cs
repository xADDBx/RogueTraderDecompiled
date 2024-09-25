namespace Kingmaker.Code.UI.MVVM.VM.Fade;

public static class FadeCanvas
{
	public static FadeVM Instance;

	public static void Fadeout(bool fade)
	{
		Instance?.Fadeout(fade);
	}
}
