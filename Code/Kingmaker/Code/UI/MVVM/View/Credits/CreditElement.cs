using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditElement : MonoBehaviour
{
	public T GetInstance<T>() where T : CreditElement
	{
		return Object.Instantiate(base.gameObject).GetComponent<T>();
	}

	public virtual void Prepare()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Destroy()
	{
		Object.Destroy(base.gameObject);
	}
}
