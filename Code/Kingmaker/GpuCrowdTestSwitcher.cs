using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker;

public class GpuCrowdTestSwitcher : MonoBehaviour
{
	public List<VisualEffect> vfxs = new List<VisualEffect>();

	public VisualEffect currentVisibleVfx;

	public TextMeshPro tmpText;

	public int currentIndex;

	private void Start()
	{
		if (vfxs == null)
		{
			Object.DestroyImmediate(this);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			if (currentIndex + 1 >= vfxs.Count)
			{
				return;
			}
			vfxs[currentIndex].gameObject.SetActive(value: false);
			vfxs[currentIndex + 1].gameObject.SetActive(value: true);
			tmpText.text = vfxs[currentIndex + 1].GetInt("ObjectsCount").ToString();
			currentIndex++;
		}
		if (Input.GetKeyDown(KeyCode.F2) && currentIndex - 1 >= 0)
		{
			vfxs[currentIndex].gameObject.SetActive(value: false);
			vfxs[currentIndex - 1].gameObject.SetActive(value: true);
			tmpText.text = vfxs[currentIndex - 1].GetInt("ObjectsCount").ToString();
			currentIndex--;
		}
	}
}
