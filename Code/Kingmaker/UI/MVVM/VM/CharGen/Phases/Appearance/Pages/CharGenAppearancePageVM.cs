using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;

public class CharGenAppearancePageVM : SelectionGroupEntityVM
{
	public readonly CharGenAppearancePageType PageType;

	public readonly string PageLabel;

	public readonly IReadOnlyReactiveProperty<bool> IsInDetailedView;

	private readonly Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM> m_ComponentsByType = new Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM>();

	private CompositeDisposable m_ComponentSubscriptions;

	private readonly CharGenContext m_CharGenContext;

	public AutoDisposingList<BaseCharGenAppearancePageComponentVM> Components { get; } = new AutoDisposingList<BaseCharGenAppearancePageComponentVM>();


	public CharGenAppearancePageVM(CharGenContext ctx, CharGenAppearancePageType pageType, IReadOnlyReactiveProperty<bool> isInDetailedView)
		: base(allowSwitchOff: false)
	{
		PageType = pageType;
		PageLabel = UIStrings.Instance.CharGen.GetPageLabelByType(pageType);
		m_CharGenContext = ctx;
		IsInDetailedView = isInDetailedView;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
	}

	public void BeginPageView()
	{
		CreateComponentsIfNeeded();
		foreach (BaseCharGenAppearancePageComponentVM component in Components)
		{
			component.OnBeginView();
		}
	}

	private void Clear()
	{
		m_ComponentsByType.Clear();
		Components.Clear();
		m_ComponentSubscriptions?.Clear();
		m_ComponentSubscriptions = null;
	}

	public void CreateComponentsIfNeeded()
	{
		if (!Components.Any())
		{
			CreateComponents();
		}
	}

	private void CreateComponents()
	{
		Clear();
		m_ComponentSubscriptions = new CompositeDisposable();
		foreach (CharGenAppearancePageComponent components in CharGenAppearancePages.GetComponentsList(PageType))
		{
			BaseCharGenAppearancePageComponentVM component = CharGenAppearanceComponentFactory.GetComponent(components, m_CharGenContext);
			if (component == null)
			{
				continue;
			}
			Components.Add(component);
			m_ComponentsByType[components] = component;
			m_ComponentSubscriptions.Add(component.OnChanged.ObserveLastValueOnLateUpdate().Subscribe(delegate(CharGenAppearancePageComponent value)
			{
				DelayedInvoker.InvokeInFrames(delegate
				{
					OnComponentChanged(value);
				}, 1);
			}));
		}
	}

	private void OnComponentChanged(CharGenAppearancePageComponent changedComponent)
	{
		List<CharGenAppearancePageComponent> list = new List<CharGenAppearancePageComponent>();
		switch (changedComponent)
		{
		case CharGenAppearancePageComponent.BodyType:
			list.Add(CharGenAppearancePageComponent.SkinColour);
			break;
		case CharGenAppearancePageComponent.HairType:
			list.Add(CharGenAppearancePageComponent.HairColour);
			break;
		case CharGenAppearancePageComponent.EyebrowType:
			list.Add(CharGenAppearancePageComponent.EyebrowColour);
			break;
		case CharGenAppearancePageComponent.BeardType:
			list.Add(CharGenAppearancePageComponent.BeardColour);
			break;
		case CharGenAppearancePageComponent.Tattoo:
			list.Add(CharGenAppearancePageComponent.TattooColor);
			break;
		}
		UpdateComponents(list);
	}

	public void UpdateComponents()
	{
		UpdateComponents(m_ComponentsByType.Keys);
	}

	public void UpdateComponent(CharGenAppearancePageComponent changedComponent)
	{
		OnComponentChanged(changedComponent);
	}

	private void UpdateComponents(IEnumerable<CharGenAppearancePageComponent> componentTypes)
	{
		foreach (CharGenAppearancePageComponent componentType in componentTypes)
		{
			if (m_ComponentsByType.TryGetValue(componentType, out var value))
			{
				CharGenAppearanceComponentFactory.UpdateComponent(componentType, value, m_CharGenContext);
			}
		}
	}

	protected override void DoSelectMe()
	{
	}
}
