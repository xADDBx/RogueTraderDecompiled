using System.Collections;

namespace UnityEngine.UI.Extensions;

public class ImageFlicker : MonoBehaviour
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private float minAlpha;

	[SerializeField]
	private float maxAlpha;

	[SerializeField]
	private float delay;

	private bool isFlickActive = true;

	private Coroutine m_FlickRoutine;

	public void StartFlickering()
	{
		m_FlickRoutine = StartCoroutine(Flick());
	}

	public void StopFlickering()
	{
		StopCoroutine(Flick());
	}

	private void OnDisable()
	{
		isFlickActive = false;
		StopCoroutine(m_FlickRoutine);
	}

	private void OnEnable()
	{
		isFlickActive = true;
		StartFlickering();
	}

	private void SetTransparency(Image p_image, float p_transparency)
	{
		p_image = m_Image;
		if (p_image != null)
		{
			Color color = p_image.color;
			color.a = p_transparency;
			p_image.color = color;
		}
	}

	private IEnumerator Flick()
	{
		WaitForSecondsRealtime wait = new WaitForSecondsRealtime(delay);
		while (isFlickActive)
		{
			float p_transparency = Random.Range(minAlpha, maxAlpha);
			SetTransparency(m_Image, p_transparency);
			yield return wait;
		}
	}
}
