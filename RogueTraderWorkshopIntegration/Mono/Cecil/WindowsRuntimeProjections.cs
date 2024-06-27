using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil;

internal sealed class WindowsRuntimeProjections
{
	private struct ProjectionInfo
	{
		public readonly string WinRTNamespace;

		public readonly string ClrNamespace;

		public readonly string ClrName;

		public readonly string ClrAssembly;

		public readonly bool Attribute;

		public ProjectionInfo(string winrt_namespace, string clr_namespace, string clr_name, string clr_assembly, bool attribute = false)
		{
			WinRTNamespace = winrt_namespace;
			ClrNamespace = clr_namespace;
			ClrName = clr_name;
			ClrAssembly = clr_assembly;
			Attribute = attribute;
		}
	}

	private static readonly Version version = new Version(4, 0, 0, 0);

	private static readonly byte[] contract_pk_token = new byte[8] { 176, 63, 95, 127, 17, 213, 10, 58 };

	private static readonly byte[] contract_pk = new byte[160]
	{
		0, 36, 0, 0, 4, 128, 0, 0, 148, 0,
		0, 0, 6, 2, 0, 0, 0, 36, 0, 0,
		82, 83, 65, 49, 0, 4, 0, 0, 1, 0,
		1, 0, 7, 209, 250, 87, 196, 174, 217, 240,
		163, 46, 132, 170, 15, 174, 253, 13, 233, 232,
		253, 106, 236, 143, 135, 251, 3, 118, 108, 131,
		76, 153, 146, 30, 178, 59, 231, 154, 217, 213,
		220, 193, 221, 154, 210, 54, 19, 33, 2, 144,
		11, 114, 60, 249, 128, 149, 127, 196, 225, 119,
		16, 143, 198, 7, 119, 79, 41, 232, 50, 14,
		146, 234, 5, 236, 228, 232, 33, 192, 165, 239,
		232, 241, 100, 92, 76, 12, 147, 193, 171, 153,
		40, 93, 98, 44, 170, 101, 44, 29, 250, 214,
		61, 116, 93, 111, 45, 229, 241, 126, 94, 175,
		15, 196, 150, 61, 38, 28, 138, 18, 67, 101,
		24, 32, 109, 192, 147, 52, 77, 90, 210, 147
	};

	private static Dictionary<string, ProjectionInfo> projections;

	private readonly ModuleDefinition module;

	private Version corlib_version = new Version(255, 255, 255, 255);

	private AssemblyNameReference[] virtual_references;

	private static Dictionary<string, ProjectionInfo> Projections
	{
		get
		{
			if (projections != null)
			{
				return projections;
			}
			Dictionary<string, ProjectionInfo> value = new Dictionary<string, ProjectionInfo>
			{
				{
					"AttributeTargets",
					new ProjectionInfo("Windows.Foundation.Metadata", "System", "AttributeTargets", "System.Runtime")
				},
				{
					"AttributeUsageAttribute",
					new ProjectionInfo("Windows.Foundation.Metadata", "System", "AttributeUsageAttribute", "System.Runtime", attribute: true)
				},
				{
					"Color",
					new ProjectionInfo("Windows.UI", "Windows.UI", "Color", "System.Runtime.WindowsRuntime")
				},
				{
					"CornerRadius",
					new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "CornerRadius", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"DateTime",
					new ProjectionInfo("Windows.Foundation", "System", "DateTimeOffset", "System.Runtime")
				},
				{
					"Duration",
					new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "Duration", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"DurationType",
					new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "DurationType", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"EventHandler`1",
					new ProjectionInfo("Windows.Foundation", "System", "EventHandler`1", "System.Runtime")
				},
				{
					"EventRegistrationToken",
					new ProjectionInfo("Windows.Foundation", "System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken", "System.Runtime.InteropServices.WindowsRuntime")
				},
				{
					"GeneratorPosition",
					new ProjectionInfo("Windows.UI.Xaml.Controls.Primitives", "Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"GridLength",
					new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "GridLength", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"GridUnitType",
					new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "GridUnitType", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"HResult",
					new ProjectionInfo("Windows.Foundation", "System", "Exception", "System.Runtime")
				},
				{
					"IBindableIterable",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections", "IEnumerable", "System.Runtime")
				},
				{
					"IBindableVector",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections", "IList", "System.Runtime")
				},
				{
					"IClosable",
					new ProjectionInfo("Windows.Foundation", "System", "IDisposable", "System.Runtime")
				},
				{
					"ICommand",
					new ProjectionInfo("Windows.UI.Xaml.Input", "System.Windows.Input", "ICommand", "System.ObjectModel")
				},
				{
					"IIterable`1",
					new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IEnumerable`1", "System.Runtime")
				},
				{
					"IKeyValuePair`2",
					new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "KeyValuePair`2", "System.Runtime")
				},
				{
					"IMapView`2",
					new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IReadOnlyDictionary`2", "System.Runtime")
				},
				{
					"IMap`2",
					new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IDictionary`2", "System.Runtime")
				},
				{
					"INotifyCollectionChanged",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "INotifyCollectionChanged", "System.ObjectModel")
				},
				{
					"INotifyPropertyChanged",
					new ProjectionInfo("Windows.UI.Xaml.Data", "System.ComponentModel", "INotifyPropertyChanged", "System.ObjectModel")
				},
				{
					"IReference`1",
					new ProjectionInfo("Windows.Foundation", "System", "Nullable`1", "System.Runtime")
				},
				{
					"IVectorView`1",
					new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IReadOnlyList`1", "System.Runtime")
				},
				{
					"IVector`1",
					new ProjectionInfo("Windows.Foundation.Collections", "System.Collections.Generic", "IList`1", "System.Runtime")
				},
				{
					"KeyTime",
					new ProjectionInfo("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "KeyTime", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"Matrix",
					new ProjectionInfo("Windows.UI.Xaml.Media", "Windows.UI.Xaml.Media", "Matrix", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"Matrix3D",
					new ProjectionInfo("Windows.UI.Xaml.Media.Media3D", "Windows.UI.Xaml.Media.Media3D", "Matrix3D", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"Matrix3x2",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Matrix3x2", "System.Numerics.Vectors")
				},
				{
					"Matrix4x4",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Matrix4x4", "System.Numerics.Vectors")
				},
				{
					"NotifyCollectionChangedAction",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedAction", "System.ObjectModel")
				},
				{
					"NotifyCollectionChangedEventArgs",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedEventArgs", "System.ObjectModel")
				},
				{
					"NotifyCollectionChangedEventHandler",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System.Collections.Specialized", "NotifyCollectionChangedEventHandler", "System.ObjectModel")
				},
				{
					"Plane",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Plane", "System.Numerics.Vectors")
				},
				{
					"Point",
					new ProjectionInfo("Windows.Foundation", "Windows.Foundation", "Point", "System.Runtime.WindowsRuntime")
				},
				{
					"PropertyChangedEventArgs",
					new ProjectionInfo("Windows.UI.Xaml.Data", "System.ComponentModel", "PropertyChangedEventArgs", "System.ObjectModel")
				},
				{
					"PropertyChangedEventHandler",
					new ProjectionInfo("Windows.UI.Xaml.Data", "System.ComponentModel", "PropertyChangedEventHandler", "System.ObjectModel")
				},
				{
					"Quaternion",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Quaternion", "System.Numerics.Vectors")
				},
				{
					"Rect",
					new ProjectionInfo("Windows.Foundation", "Windows.Foundation", "Rect", "System.Runtime.WindowsRuntime")
				},
				{
					"RepeatBehavior",
					new ProjectionInfo("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "RepeatBehavior", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"RepeatBehaviorType",
					new ProjectionInfo("Windows.UI.Xaml.Media.Animation", "Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"Size",
					new ProjectionInfo("Windows.Foundation", "Windows.Foundation", "Size", "System.Runtime.WindowsRuntime")
				},
				{
					"Thickness",
					new ProjectionInfo("Windows.UI.Xaml", "Windows.UI.Xaml", "Thickness", "System.Runtime.WindowsRuntime.UI.Xaml")
				},
				{
					"TimeSpan",
					new ProjectionInfo("Windows.Foundation", "System", "TimeSpan", "System.Runtime")
				},
				{
					"TypeName",
					new ProjectionInfo("Windows.UI.Xaml.Interop", "System", "Type", "System.Runtime")
				},
				{
					"Uri",
					new ProjectionInfo("Windows.Foundation", "System", "Uri", "System.Runtime")
				},
				{
					"Vector2",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Vector2", "System.Numerics.Vectors")
				},
				{
					"Vector3",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Vector3", "System.Numerics.Vectors")
				},
				{
					"Vector4",
					new ProjectionInfo("Windows.Foundation.Numerics", "System.Numerics", "Vector4", "System.Numerics.Vectors")
				}
			};
			Interlocked.CompareExchange(ref projections, value, null);
			return projections;
		}
	}

	private AssemblyNameReference[] VirtualReferences
	{
		get
		{
			if (virtual_references == null)
			{
				Mixin.Read(module.AssemblyReferences);
			}
			return virtual_references;
		}
	}

	public WindowsRuntimeProjections(ModuleDefinition module)
	{
		this.module = module;
	}

	public static void Project(TypeDefinition type)
	{
		TypeDefinitionTreatment typeDefinitionTreatment = TypeDefinitionTreatment.None;
		MetadataKind metadataKind = type.Module.MetadataKind;
		Collection<MethodDefinition> redirectedMethods = null;
		Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> redirectedInterfaces = null;
		if (type.IsWindowsRuntime)
		{
			switch (metadataKind)
			{
			case MetadataKind.WindowsMetadata:
			{
				typeDefinitionTreatment = GetWellKnownTypeDefinitionTreatment(type);
				if (typeDefinitionTreatment != 0)
				{
					ApplyProjection(type, new TypeDefinitionProjection(type, typeDefinitionTreatment, redirectedMethods, redirectedInterfaces));
					return;
				}
				TypeReference baseType = type.BaseType;
				typeDefinitionTreatment = ((baseType == null || !IsAttribute(baseType)) ? GenerateRedirectionInformation(type, out redirectedMethods, out redirectedInterfaces) : TypeDefinitionTreatment.NormalAttribute);
				break;
			}
			case MetadataKind.ManagedWindowsMetadata:
				if (NeedsWindowsRuntimePrefix(type))
				{
					typeDefinitionTreatment = TypeDefinitionTreatment.PrefixWindowsRuntimeName;
				}
				break;
			}
			if ((typeDefinitionTreatment == TypeDefinitionTreatment.PrefixWindowsRuntimeName || typeDefinitionTreatment == TypeDefinitionTreatment.NormalType) && !type.IsInterface && HasAttribute(type.CustomAttributes, "Windows.UI.Xaml", "TreatAsAbstractComposableClassAttribute"))
			{
				typeDefinitionTreatment |= TypeDefinitionTreatment.Abstract;
			}
		}
		else if (metadataKind == MetadataKind.ManagedWindowsMetadata && IsClrImplementationType(type))
		{
			typeDefinitionTreatment = TypeDefinitionTreatment.UnmangleWindowsRuntimeName;
		}
		if (typeDefinitionTreatment != 0)
		{
			ApplyProjection(type, new TypeDefinitionProjection(type, typeDefinitionTreatment, redirectedMethods, redirectedInterfaces));
		}
	}

	private static TypeDefinitionTreatment GetWellKnownTypeDefinitionTreatment(TypeDefinition type)
	{
		if (!Projections.TryGetValue(type.Name, out var value))
		{
			return TypeDefinitionTreatment.None;
		}
		TypeDefinitionTreatment typeDefinitionTreatment = (value.Attribute ? TypeDefinitionTreatment.RedirectToClrAttribute : TypeDefinitionTreatment.RedirectToClrType);
		if (type.Namespace == value.ClrNamespace)
		{
			return typeDefinitionTreatment;
		}
		if (type.Namespace == value.WinRTNamespace)
		{
			return typeDefinitionTreatment | TypeDefinitionTreatment.Internal;
		}
		return TypeDefinitionTreatment.None;
	}

	private static TypeDefinitionTreatment GenerateRedirectionInformation(TypeDefinition type, out Collection<MethodDefinition> redirectedMethods, out Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> redirectedInterfaces)
	{
		bool flag = false;
		redirectedMethods = null;
		redirectedInterfaces = null;
		foreach (InterfaceImplementation @interface in type.Interfaces)
		{
			if (IsRedirectedType(@interface.InterfaceType))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return TypeDefinitionTreatment.NormalType;
		}
		HashSet<TypeReference> hashSet = new HashSet<TypeReference>(new TypeReferenceEqualityComparer());
		redirectedMethods = new Collection<MethodDefinition>();
		redirectedInterfaces = new Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>>();
		foreach (InterfaceImplementation interface2 in type.Interfaces)
		{
			TypeReference interfaceType = interface2.InterfaceType;
			if (IsRedirectedType(interfaceType))
			{
				hashSet.Add(interfaceType);
				CollectImplementedInterfaces(interfaceType, hashSet);
			}
		}
		foreach (InterfaceImplementation interface3 in type.Interfaces)
		{
			TypeReference interfaceType2 = interface3.InterfaceType;
			if (!IsRedirectedType(interface3.InterfaceType))
			{
				continue;
			}
			TypeReference elementType = interfaceType2.GetElementType();
			TypeReference typeReference = new TypeReference(elementType.Namespace, elementType.Name, elementType.Module, elementType.Scope)
			{
				DeclaringType = elementType.DeclaringType,
				projection = elementType.projection
			};
			RemoveProjection(typeReference);
			if (interfaceType2 is GenericInstanceType genericInstanceType)
			{
				GenericInstanceType genericInstanceType2 = new GenericInstanceType(typeReference);
				foreach (TypeReference genericArgument in genericInstanceType.GenericArguments)
				{
					genericInstanceType2.GenericArguments.Add(genericArgument);
				}
				typeReference = genericInstanceType2;
			}
			InterfaceImplementation value = new InterfaceImplementation(typeReference);
			redirectedInterfaces.Add(new KeyValuePair<InterfaceImplementation, InterfaceImplementation>(interface3, value));
		}
		if (!type.IsInterface)
		{
			foreach (TypeReference item in hashSet)
			{
				RedirectInterfaceMethods(item, redirectedMethods);
			}
		}
		return TypeDefinitionTreatment.RedirectImplementedMethods;
	}

	private static void CollectImplementedInterfaces(TypeReference type, HashSet<TypeReference> results)
	{
		TypeResolver typeResolver = TypeResolver.For(type);
		foreach (InterfaceImplementation @interface in type.Resolve().Interfaces)
		{
			TypeReference typeReference = typeResolver.Resolve(@interface.InterfaceType);
			results.Add(typeReference);
			CollectImplementedInterfaces(typeReference, results);
		}
	}

	private static void RedirectInterfaceMethods(TypeReference interfaceType, Collection<MethodDefinition> redirectedMethods)
	{
		TypeResolver typeResolver = TypeResolver.For(interfaceType);
		foreach (MethodDefinition method in interfaceType.Resolve().Methods)
		{
			MethodDefinition methodDefinition = new MethodDefinition(method.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.VtableLayoutMask, typeResolver.Resolve(method.ReturnType));
			methodDefinition.ImplAttributes = MethodImplAttributes.CodeTypeMask;
			foreach (ParameterDefinition parameter in method.Parameters)
			{
				methodDefinition.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, typeResolver.Resolve(parameter.ParameterType)));
			}
			methodDefinition.Overrides.Add(typeResolver.Resolve(method));
			redirectedMethods.Add(methodDefinition);
		}
	}

	private static bool IsRedirectedType(TypeReference type)
	{
		if (type.GetElementType().projection is TypeReferenceProjection typeReferenceProjection)
		{
			return typeReferenceProjection.Treatment == TypeReferenceTreatment.UseProjectionInfo;
		}
		return false;
	}

	private static bool NeedsWindowsRuntimePrefix(TypeDefinition type)
	{
		if ((type.Attributes & (TypeAttributes.VisibilityMask | TypeAttributes.ClassSemanticMask)) != TypeAttributes.Public)
		{
			return false;
		}
		TypeReference baseType = type.BaseType;
		if (baseType == null || baseType.MetadataToken.TokenType != TokenType.TypeRef)
		{
			return false;
		}
		if (baseType.Namespace == "System")
		{
			switch (baseType.Name)
			{
			case "Attribute":
			case "MulticastDelegate":
			case "ValueType":
				return false;
			}
		}
		return true;
	}

	public static bool IsClrImplementationType(TypeDefinition type)
	{
		if ((type.Attributes & (TypeAttributes.VisibilityMask | TypeAttributes.SpecialName)) != TypeAttributes.SpecialName)
		{
			return false;
		}
		return type.Name.StartsWith("<CLR>");
	}

	public static void ApplyProjection(TypeDefinition type, TypeDefinitionProjection projection)
	{
		if (projection == null)
		{
			return;
		}
		TypeDefinitionTreatment treatment = projection.Treatment;
		switch (treatment & TypeDefinitionTreatment.KindMask)
		{
		case TypeDefinitionTreatment.NormalType:
			type.Attributes |= TypeAttributes.Import | TypeAttributes.WindowsRuntime;
			break;
		case TypeDefinitionTreatment.NormalAttribute:
			type.Attributes |= TypeAttributes.Sealed | TypeAttributes.WindowsRuntime;
			break;
		case TypeDefinitionTreatment.UnmangleWindowsRuntimeName:
			type.Attributes = (type.Attributes & ~TypeAttributes.SpecialName) | TypeAttributes.Public;
			type.Name = type.Name.Substring("<CLR>".Length);
			break;
		case TypeDefinitionTreatment.PrefixWindowsRuntimeName:
			type.Attributes = (type.Attributes & ~TypeAttributes.Public) | TypeAttributes.Import;
			type.Name = "<WinRT>" + type.Name;
			break;
		case TypeDefinitionTreatment.RedirectToClrType:
			type.Attributes = (type.Attributes & ~TypeAttributes.Public) | TypeAttributes.Import;
			break;
		case TypeDefinitionTreatment.RedirectToClrAttribute:
			type.Attributes &= ~TypeAttributes.Public;
			break;
		case TypeDefinitionTreatment.RedirectImplementedMethods:
			type.Attributes |= TypeAttributes.Import | TypeAttributes.WindowsRuntime;
			foreach (KeyValuePair<InterfaceImplementation, InterfaceImplementation> redirectedInterface in projection.RedirectedInterfaces)
			{
				type.Interfaces.Add(redirectedInterface.Value);
				foreach (CustomAttribute customAttribute in redirectedInterface.Key.CustomAttributes)
				{
					redirectedInterface.Value.CustomAttributes.Add(customAttribute);
				}
				redirectedInterface.Key.CustomAttributes.Clear();
				foreach (MethodDefinition method in type.Methods)
				{
					foreach (MethodReference @override in method.Overrides)
					{
						if (TypeReferenceEqualityComparer.AreEqual(@override.DeclaringType, redirectedInterface.Key.InterfaceType))
						{
							@override.DeclaringType = redirectedInterface.Value.InterfaceType;
						}
					}
				}
			}
			foreach (MethodDefinition redirectedMethod in projection.RedirectedMethods)
			{
				type.Methods.Add(redirectedMethod);
			}
			break;
		}
		if ((treatment & TypeDefinitionTreatment.Abstract) != 0)
		{
			type.Attributes |= TypeAttributes.Abstract;
		}
		if ((treatment & TypeDefinitionTreatment.Internal) != 0)
		{
			type.Attributes &= ~TypeAttributes.Public;
		}
		type.WindowsRuntimeProjection = projection;
	}

	public static TypeDefinitionProjection RemoveProjection(TypeDefinition type)
	{
		if (!type.IsWindowsRuntimeProjection)
		{
			return null;
		}
		TypeDefinitionProjection windowsRuntimeProjection = type.WindowsRuntimeProjection;
		type.WindowsRuntimeProjection = null;
		type.Attributes = windowsRuntimeProjection.Attributes;
		type.Name = windowsRuntimeProjection.Name;
		if (windowsRuntimeProjection.Treatment == TypeDefinitionTreatment.RedirectImplementedMethods)
		{
			foreach (MethodDefinition redirectedMethod in windowsRuntimeProjection.RedirectedMethods)
			{
				type.Methods.Remove(redirectedMethod);
			}
			foreach (KeyValuePair<InterfaceImplementation, InterfaceImplementation> redirectedInterface in windowsRuntimeProjection.RedirectedInterfaces)
			{
				foreach (MethodDefinition method in type.Methods)
				{
					foreach (MethodReference @override in method.Overrides)
					{
						if (TypeReferenceEqualityComparer.AreEqual(@override.DeclaringType, redirectedInterface.Value.InterfaceType))
						{
							@override.DeclaringType = redirectedInterface.Key.InterfaceType;
						}
					}
				}
				foreach (CustomAttribute customAttribute in redirectedInterface.Value.CustomAttributes)
				{
					redirectedInterface.Key.CustomAttributes.Add(customAttribute);
				}
				redirectedInterface.Value.CustomAttributes.Clear();
				type.Interfaces.Remove(redirectedInterface.Value);
			}
		}
		return windowsRuntimeProjection;
	}

	public static void Project(TypeReference type)
	{
		ProjectionInfo value;
		TypeReferenceTreatment typeReferenceTreatment = ((!Projections.TryGetValue(type.Name, out value) || !(value.WinRTNamespace == type.Namespace)) ? GetSpecialTypeReferenceTreatment(type) : TypeReferenceTreatment.UseProjectionInfo);
		if (typeReferenceTreatment != 0)
		{
			ApplyProjection(type, new TypeReferenceProjection(type, typeReferenceTreatment));
		}
	}

	private static TypeReferenceTreatment GetSpecialTypeReferenceTreatment(TypeReference type)
	{
		if (type.Namespace == "System")
		{
			if (type.Name == "MulticastDelegate")
			{
				return TypeReferenceTreatment.SystemDelegate;
			}
			if (type.Name == "Attribute")
			{
				return TypeReferenceTreatment.SystemAttribute;
			}
		}
		return TypeReferenceTreatment.None;
	}

	private static bool IsAttribute(TypeReference type)
	{
		if (type.MetadataToken.TokenType != TokenType.TypeRef)
		{
			return false;
		}
		if (type.Name == "Attribute")
		{
			return type.Namespace == "System";
		}
		return false;
	}

	private static bool IsEnum(TypeReference type)
	{
		if (type.MetadataToken.TokenType != TokenType.TypeRef)
		{
			return false;
		}
		if (type.Name == "Enum")
		{
			return type.Namespace == "System";
		}
		return false;
	}

	public static void ApplyProjection(TypeReference type, TypeReferenceProjection projection)
	{
		if (projection != null)
		{
			switch (projection.Treatment)
			{
			case TypeReferenceTreatment.SystemDelegate:
			case TypeReferenceTreatment.SystemAttribute:
				type.Scope = type.Module.Projections.GetAssemblyReference("System.Runtime");
				break;
			case TypeReferenceTreatment.UseProjectionInfo:
			{
				ProjectionInfo projectionInfo = Projections[type.Name];
				type.Name = projectionInfo.ClrName;
				type.Namespace = projectionInfo.ClrNamespace;
				type.Scope = type.Module.Projections.GetAssemblyReference(projectionInfo.ClrAssembly);
				break;
			}
			}
			type.WindowsRuntimeProjection = projection;
		}
	}

	public static TypeReferenceProjection RemoveProjection(TypeReference type)
	{
		if (!type.IsWindowsRuntimeProjection)
		{
			return null;
		}
		TypeReferenceProjection windowsRuntimeProjection = type.WindowsRuntimeProjection;
		type.WindowsRuntimeProjection = null;
		type.Name = windowsRuntimeProjection.Name;
		type.Namespace = windowsRuntimeProjection.Namespace;
		type.Scope = windowsRuntimeProjection.Scope;
		return windowsRuntimeProjection;
	}

	public static void Project(MethodDefinition method)
	{
		MethodDefinitionTreatment methodDefinitionTreatment = MethodDefinitionTreatment.None;
		bool flag = false;
		TypeDefinition declaringType = method.DeclaringType;
		if (declaringType.IsWindowsRuntime)
		{
			if (IsClrImplementationType(declaringType))
			{
				methodDefinitionTreatment = MethodDefinitionTreatment.None;
			}
			else if (declaringType.IsNested)
			{
				methodDefinitionTreatment = MethodDefinitionTreatment.None;
			}
			else if (declaringType.IsInterface)
			{
				methodDefinitionTreatment = MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall;
			}
			else if (declaringType.Module.MetadataKind == MetadataKind.ManagedWindowsMetadata && !method.IsPublic)
			{
				methodDefinitionTreatment = MethodDefinitionTreatment.None;
			}
			else
			{
				flag = true;
				TypeReference baseType = declaringType.BaseType;
				if (baseType != null && baseType.MetadataToken.TokenType == TokenType.TypeRef)
				{
					switch (GetSpecialTypeReferenceTreatment(baseType))
					{
					case TypeReferenceTreatment.SystemDelegate:
						methodDefinitionTreatment = MethodDefinitionTreatment.Public | MethodDefinitionTreatment.Runtime;
						flag = false;
						break;
					case TypeReferenceTreatment.SystemAttribute:
						methodDefinitionTreatment = MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall;
						flag = false;
						break;
					}
				}
			}
		}
		if (flag)
		{
			bool flag2 = false;
			bool flag3 = false;
			foreach (MethodReference @override in method.Overrides)
			{
				if (@override.MetadataToken.TokenType == TokenType.MemberRef && ImplementsRedirectedInterface(@override))
				{
					flag2 = true;
				}
				else
				{
					flag3 = true;
				}
			}
			if (flag2 && !flag3)
			{
				methodDefinitionTreatment = MethodDefinitionTreatment.Private | MethodDefinitionTreatment.Runtime | MethodDefinitionTreatment.InternalCall;
				flag = false;
			}
		}
		if (flag)
		{
			methodDefinitionTreatment |= GetMethodDefinitionTreatmentFromCustomAttributes(method);
		}
		if (methodDefinitionTreatment != 0)
		{
			ApplyProjection(method, new MethodDefinitionProjection(method, methodDefinitionTreatment));
		}
	}

	private static MethodDefinitionTreatment GetMethodDefinitionTreatmentFromCustomAttributes(MethodDefinition method)
	{
		MethodDefinitionTreatment methodDefinitionTreatment = MethodDefinitionTreatment.None;
		foreach (CustomAttribute customAttribute in method.CustomAttributes)
		{
			TypeReference attributeType = customAttribute.AttributeType;
			if (!(attributeType.Namespace != "Windows.UI.Xaml"))
			{
				if (attributeType.Name == "TreatAsPublicMethodAttribute")
				{
					methodDefinitionTreatment |= MethodDefinitionTreatment.Public;
				}
				else if (attributeType.Name == "TreatAsAbstractMethodAttribute")
				{
					methodDefinitionTreatment |= MethodDefinitionTreatment.Abstract;
				}
			}
		}
		return methodDefinitionTreatment;
	}

	public static void ApplyProjection(MethodDefinition method, MethodDefinitionProjection projection)
	{
		if (projection != null)
		{
			MethodDefinitionTreatment treatment = projection.Treatment;
			if ((treatment & MethodDefinitionTreatment.Abstract) != 0)
			{
				method.Attributes |= MethodAttributes.Abstract;
			}
			if ((treatment & MethodDefinitionTreatment.Private) != 0)
			{
				method.Attributes = (method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Private;
			}
			if ((treatment & MethodDefinitionTreatment.Public) != 0)
			{
				method.Attributes = (method.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public;
			}
			if ((treatment & MethodDefinitionTreatment.Runtime) != 0)
			{
				method.ImplAttributes |= MethodImplAttributes.CodeTypeMask;
			}
			if ((treatment & MethodDefinitionTreatment.InternalCall) != 0)
			{
				method.ImplAttributes |= MethodImplAttributes.InternalCall;
			}
			method.WindowsRuntimeProjection = projection;
		}
	}

	public static MethodDefinitionProjection RemoveProjection(MethodDefinition method)
	{
		if (!method.IsWindowsRuntimeProjection)
		{
			return null;
		}
		MethodDefinitionProjection windowsRuntimeProjection = method.WindowsRuntimeProjection;
		method.WindowsRuntimeProjection = null;
		method.Attributes = windowsRuntimeProjection.Attributes;
		method.ImplAttributes = windowsRuntimeProjection.ImplAttributes;
		method.Name = windowsRuntimeProjection.Name;
		return windowsRuntimeProjection;
	}

	public static void Project(FieldDefinition field)
	{
		FieldDefinitionTreatment fieldDefinitionTreatment = FieldDefinitionTreatment.None;
		TypeDefinition declaringType = field.DeclaringType;
		if (declaringType.Module.MetadataKind == MetadataKind.WindowsMetadata && field.IsRuntimeSpecialName && field.Name == "value__")
		{
			TypeReference baseType = declaringType.BaseType;
			if (baseType != null && IsEnum(baseType))
			{
				fieldDefinitionTreatment = FieldDefinitionTreatment.Public;
			}
		}
		if (fieldDefinitionTreatment != 0)
		{
			ApplyProjection(field, new FieldDefinitionProjection(field, fieldDefinitionTreatment));
		}
	}

	public static void ApplyProjection(FieldDefinition field, FieldDefinitionProjection projection)
	{
		if (projection != null)
		{
			if (projection.Treatment == FieldDefinitionTreatment.Public)
			{
				field.Attributes = (field.Attributes & ~FieldAttributes.FieldAccessMask) | FieldAttributes.Public;
			}
			field.WindowsRuntimeProjection = projection;
		}
	}

	public static FieldDefinitionProjection RemoveProjection(FieldDefinition field)
	{
		if (!field.IsWindowsRuntimeProjection)
		{
			return null;
		}
		FieldDefinitionProjection windowsRuntimeProjection = field.WindowsRuntimeProjection;
		field.WindowsRuntimeProjection = null;
		field.Attributes = windowsRuntimeProjection.Attributes;
		return windowsRuntimeProjection;
	}

	private static bool ImplementsRedirectedInterface(MemberReference member)
	{
		TypeReference declaringType = member.DeclaringType;
		TypeReference typeReference;
		switch (declaringType.MetadataToken.TokenType)
		{
		case TokenType.TypeRef:
			typeReference = declaringType;
			break;
		case TokenType.TypeSpec:
			if (!declaringType.IsGenericInstance)
			{
				return false;
			}
			typeReference = ((TypeSpecification)declaringType).ElementType;
			if (typeReference.MetadataType != MetadataType.Class || typeReference.MetadataToken.TokenType != TokenType.TypeRef)
			{
				return false;
			}
			break;
		default:
			return false;
		}
		TypeReferenceProjection projection = RemoveProjection(typeReference);
		bool result = false;
		if (Projections.TryGetValue(typeReference.Name, out var value) && typeReference.Namespace == value.WinRTNamespace)
		{
			result = true;
		}
		ApplyProjection(typeReference, projection);
		return result;
	}

	public void AddVirtualReferences(Collection<AssemblyNameReference> references)
	{
		AssemblyNameReference coreLibrary = GetCoreLibrary(references);
		corlib_version = coreLibrary.Version;
		coreLibrary.Version = version;
		if (virtual_references == null)
		{
			AssemblyNameReference[] assemblyReferences = GetAssemblyReferences(coreLibrary);
			Interlocked.CompareExchange(ref virtual_references, assemblyReferences, null);
		}
		AssemblyNameReference[] array = virtual_references;
		foreach (AssemblyNameReference item in array)
		{
			references.Add(item);
		}
	}

	public void RemoveVirtualReferences(Collection<AssemblyNameReference> references)
	{
		GetCoreLibrary(references).Version = corlib_version;
		AssemblyNameReference[] virtualReferences = VirtualReferences;
		foreach (AssemblyNameReference item in virtualReferences)
		{
			references.Remove(item);
		}
	}

	private static AssemblyNameReference[] GetAssemblyReferences(AssemblyNameReference corlib)
	{
		AssemblyNameReference assemblyNameReference = new AssemblyNameReference("System.Runtime", version);
		AssemblyNameReference assemblyNameReference2 = new AssemblyNameReference("System.Runtime.InteropServices.WindowsRuntime", version);
		AssemblyNameReference assemblyNameReference3 = new AssemblyNameReference("System.ObjectModel", version);
		AssemblyNameReference assemblyNameReference4 = new AssemblyNameReference("System.Runtime.WindowsRuntime", version);
		AssemblyNameReference assemblyNameReference5 = new AssemblyNameReference("System.Runtime.WindowsRuntime.UI.Xaml", version);
		AssemblyNameReference assemblyNameReference6 = new AssemblyNameReference("System.Numerics.Vectors", version);
		if (corlib.HasPublicKey)
		{
			byte[] publicKey2 = (assemblyNameReference5.PublicKey = corlib.PublicKey);
			assemblyNameReference4.PublicKey = publicKey2;
			byte[] array2 = (assemblyNameReference6.PublicKey = contract_pk);
			byte[] array4 = (assemblyNameReference3.PublicKey = array2);
			publicKey2 = (assemblyNameReference2.PublicKey = array4);
			assemblyNameReference.PublicKey = publicKey2;
		}
		else
		{
			byte[] publicKey2 = (assemblyNameReference5.PublicKeyToken = corlib.PublicKeyToken);
			assemblyNameReference4.PublicKeyToken = publicKey2;
			byte[] array2 = (assemblyNameReference6.PublicKeyToken = contract_pk_token);
			byte[] array4 = (assemblyNameReference3.PublicKeyToken = array2);
			publicKey2 = (assemblyNameReference2.PublicKeyToken = array4);
			assemblyNameReference.PublicKeyToken = publicKey2;
		}
		return new AssemblyNameReference[6] { assemblyNameReference, assemblyNameReference2, assemblyNameReference3, assemblyNameReference4, assemblyNameReference5, assemblyNameReference6 };
	}

	private static AssemblyNameReference GetCoreLibrary(Collection<AssemblyNameReference> references)
	{
		foreach (AssemblyNameReference reference in references)
		{
			if (reference.Name == "mscorlib")
			{
				return reference;
			}
		}
		throw new BadImageFormatException("Missing mscorlib reference in AssemblyRef table.");
	}

	private AssemblyNameReference GetAssemblyReference(string name)
	{
		AssemblyNameReference[] virtualReferences = VirtualReferences;
		foreach (AssemblyNameReference assemblyNameReference in virtualReferences)
		{
			if (assemblyNameReference.Name == name)
			{
				return assemblyNameReference;
			}
		}
		throw new Exception();
	}

	public static void Project(ICustomAttributeProvider owner, Collection<CustomAttribute> owner_attributes, CustomAttribute attribute)
	{
		if (!IsWindowsAttributeUsageAttribute(owner, attribute))
		{
			return;
		}
		CustomAttributeValueTreatment customAttributeValueTreatment = CustomAttributeValueTreatment.None;
		TypeDefinition typeDefinition = (TypeDefinition)owner;
		if (typeDefinition.Namespace == "Windows.Foundation.Metadata")
		{
			if (typeDefinition.Name == "VersionAttribute")
			{
				customAttributeValueTreatment = CustomAttributeValueTreatment.VersionAttribute;
			}
			else if (typeDefinition.Name == "DeprecatedAttribute")
			{
				customAttributeValueTreatment = CustomAttributeValueTreatment.DeprecatedAttribute;
			}
		}
		if (customAttributeValueTreatment == CustomAttributeValueTreatment.None)
		{
			customAttributeValueTreatment = ((!HasAttribute(owner_attributes, "Windows.Foundation.Metadata", "AllowMultipleAttribute")) ? CustomAttributeValueTreatment.AllowSingle : CustomAttributeValueTreatment.AllowMultiple);
		}
		if (customAttributeValueTreatment != 0)
		{
			AttributeTargets targets = (AttributeTargets)attribute.ConstructorArguments[0].Value;
			ApplyProjection(attribute, new CustomAttributeValueProjection(targets, customAttributeValueTreatment));
		}
	}

	private static bool IsWindowsAttributeUsageAttribute(ICustomAttributeProvider owner, CustomAttribute attribute)
	{
		if (owner.MetadataToken.TokenType != TokenType.TypeDef)
		{
			return false;
		}
		MethodReference constructor = attribute.Constructor;
		if (constructor.MetadataToken.TokenType != TokenType.MemberRef)
		{
			return false;
		}
		TypeReference declaringType = constructor.DeclaringType;
		if (declaringType.MetadataToken.TokenType != TokenType.TypeRef)
		{
			return false;
		}
		if (declaringType.Name == "AttributeUsageAttribute")
		{
			return declaringType.Namespace == "System";
		}
		return false;
	}

	private static bool HasAttribute(Collection<CustomAttribute> attributes, string @namespace, string name)
	{
		foreach (CustomAttribute attribute in attributes)
		{
			TypeReference attributeType = attribute.AttributeType;
			if (attributeType.Name == name && attributeType.Namespace == @namespace)
			{
				return true;
			}
		}
		return false;
	}

	public static void ApplyProjection(CustomAttribute attribute, CustomAttributeValueProjection projection)
	{
		if (projection != null)
		{
			bool flag;
			bool flag2;
			switch (projection.Treatment)
			{
			case CustomAttributeValueTreatment.AllowSingle:
				flag = false;
				flag2 = false;
				break;
			case CustomAttributeValueTreatment.AllowMultiple:
				flag = false;
				flag2 = true;
				break;
			case CustomAttributeValueTreatment.VersionAttribute:
			case CustomAttributeValueTreatment.DeprecatedAttribute:
				flag = true;
				flag2 = true;
				break;
			default:
				throw new ArgumentException();
			}
			AttributeTargets attributeTargets = (AttributeTargets)attribute.ConstructorArguments[0].Value;
			if (flag)
			{
				attributeTargets |= AttributeTargets.Constructor | AttributeTargets.Property;
			}
			attribute.ConstructorArguments[0] = new CustomAttributeArgument(attribute.ConstructorArguments[0].Type, attributeTargets);
			attribute.Properties.Add(new CustomAttributeNamedArgument("AllowMultiple", new CustomAttributeArgument(attribute.Module.TypeSystem.Boolean, flag2)));
			attribute.projection = projection;
		}
	}

	public static CustomAttributeValueProjection RemoveProjection(CustomAttribute attribute)
	{
		if (attribute.projection == null)
		{
			return null;
		}
		CustomAttributeValueProjection projection = attribute.projection;
		attribute.projection = null;
		attribute.ConstructorArguments[0] = new CustomAttributeArgument(attribute.ConstructorArguments[0].Type, projection.Targets);
		attribute.Properties.Clear();
		return projection;
	}
}
