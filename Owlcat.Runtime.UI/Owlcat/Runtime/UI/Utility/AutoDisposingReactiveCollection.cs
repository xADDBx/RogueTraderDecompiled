using System;
using System.Collections.Generic;
using UniRx;

namespace Owlcat.Runtime.UI.Utility;

public class AutoDisposingReactiveCollection<T> : ReactiveCollection<T> where T : IDisposable
{
	protected override void RemoveItem(int index)
	{
		T val = base[index];
		if (val != null)
		{
			val.Dispose();
		}
		base.RemoveItem(index);
	}

	protected override void ClearItems()
	{
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current?.Dispose();
			}
		}
		base.ClearItems();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ClearItems();
		}
		base.Dispose(disposing);
	}
}
