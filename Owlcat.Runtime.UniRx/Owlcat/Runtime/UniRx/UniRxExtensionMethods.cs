using System;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Owlcat.Runtime.UniRx;

public static class UniRxExtensionMethods
{
	public static IDisposable Subscribe(this IObservable<Unit> source, Action action)
	{
		return ObservableExtensions.Subscribe(source, delegate
		{
			action();
		});
	}

	public static IObservable<bool> And(this IObservable<bool> property1, IObservable<bool> property2)
	{
		if (property1 == null)
		{
			return property2;
		}
		if (property2 == null)
		{
			return property1;
		}
		return property1.CombineLatest(property2, (bool p1, bool p2) => p1 && p2);
	}

	public static IObservable<bool> Or(this IObservable<bool> property1, IObservable<bool> property2)
	{
		if (property1 == null)
		{
			return property2;
		}
		if (property2 == null)
		{
			return property1;
		}
		return property1.CombineLatest(property2, (bool p1, bool p2) => p1 || p2);
	}

	public static IObservable<bool> Xor(this IObservable<bool> property1, IObservable<bool> property2)
	{
		if (property1 == null)
		{
			return property2;
		}
		if (property2 == null)
		{
			return property1;
		}
		return property1.CombineLatest(property2, (bool p1, bool p2) => p1 ^ p2);
	}

	public static IObservable<bool> Not(this IObservable<bool> property)
	{
		return property.Select((bool p) => !p);
	}

	public static IObservable<T> NotNull<T>(this IObservable<T> source)
	{
		return source.Where((T value) => value != null);
	}

	public static IObservable<TResult> SelectManyOrDefault<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> collectionSelector)
	{
		return source.SelectMany((TSource sourceElement) => (sourceElement != null) ? collectionSelector(sourceElement) : Observable.Return(default(TResult)));
	}

	public static IObservable<T> DefaultIfNull<T>(this IObservable<T> source, T defaultValue) where T : class
	{
		return new DefaultIfNullObservable<T>(source, defaultValue);
	}

	public static IObservable<Unit> ObserveAnyCollectionChange<T>(this IReadOnlyReactiveCollection<T> collection)
	{
		return (from _ in collection.ObserveCountChanged()
			select Unit.Default).Merge(from _ in collection.ObserveMove()
			select Unit.Default, from _ in collection.ObserveReplace()
			select Unit.Default, Observable.Return(Unit.Default));
	}

	public static IReadOnlyReactiveCollection<TResult> SelectCollection<TSource, TResult>(this IReadOnlyReactiveCollection<TSource> collection, Func<TSource, TResult> selector)
	{
		return new SelectReactiveCollection<TSource, TResult>(collection, selector);
	}

	public static IObservable<bool> ObserveContains<T>(this ReactiveCollection<T> collection, IObservable<T> item)
	{
		return collection.ObserveAnyCollectionChange().CombineLatest(item, (Unit _, T i) => collection.Contains(i));
	}

	public static ReadOnlyReactiveProperty<TProperty> GetReactiveProperty<TUpdatable, TProperty>(this TUpdatable updatable, Func<TUpdatable, TProperty> propertyGetter, bool notifyOnlyNewValue = true) where TUpdatable : ICanConvertPropertiesToReactive
	{
		return new ReadOnlyReactiveProperty<TProperty>(updatable.UpdateCommand.Select((Unit _) => propertyGetter(updatable)), propertyGetter(updatable), notifyOnlyNewValue);
	}

	public static IObservable<TSource> ObserveLastValueOnLateUpdate<TSource>(this IObservable<TSource> source)
	{
		return LastValueOnLateUpdateObservableFabric.CreateObservable(source);
	}

	public static IObservable<TSource> ObserveInProfilerSample<TSource>(this IObservable<TSource> source, string sampleName)
	{
		return new ObserveInProfilerSampleObservable<TSource>(source, sampleName);
	}

	public static IObservable<TSource> ObserveInTryCatchFinally<TSource>(this IObservable<TSource> source, Action<Exception> catchAction = null, Action finallyAction = null)
	{
		return source.ObserveInTryCatchFinally<TSource, Exception>(catchAction, finallyAction);
	}

	public static IObservable<TSource> ObserveInTryCatchFinally<TSource, TException>(this IObservable<TSource> source, Action<TException> catchAction = null, Action finallyAction = null) where TException : Exception
	{
		return new TryCatchFinallyObservable<TSource, TException>(source, catchAction, finallyAction);
	}

	public static IObservable<PointerEventData> OnPointerEnterAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservablePointerEnterTrigger>(component.gameObject).OnPointerEnterAsObservable();
	}

	public static IObservable<PointerEventData> OnPointerExitAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservablePointerExitTrigger>(component.gameObject).OnPointerExitAsObservable();
	}

	public static IObservable<int> OnValueChangedAsObservable(this TMP_Dropdown dropdown)
	{
		return Observable.CreateWithState(dropdown, delegate(TMP_Dropdown d, IObserver<int> observer)
		{
			observer.OnNext(d.value);
			return d.onValueChanged.AsObservable().Subscribe(observer);
		});
	}

	public static IObservable<string> OnEndEditAsObservable(this TMP_InputField inputField)
	{
		return inputField.onEndEdit.AsObservable();
	}

	public static IObservable<PointerEventData> OnPointerClickAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservablePointerClickTrigger>(component.gameObject).OnPointerClickAsObservable();
	}

	public static IObservable<PointerEventData> OnDragAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservableDragTrigger>(component.gameObject).OnDragAsObservable();
	}

	public static IObservable<PointerEventData> OnBeginDragAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservableBeginDragTrigger>(component.gameObject).OnBeginDragAsObservable();
	}

	public static IObservable<PointerEventData> OnEndDragAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservableEndDragTrigger>(component.gameObject).OnEndDragAsObservable();
	}

	public static IObservable<PointerEventData> OnDropAsObservable(this MonoBehaviour component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservableDropTrigger>(component.gameObject).OnDropAsObservable();
	}

	private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
	{
		T val = gameObject.GetComponent<T>();
		if (val == null)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}
}
