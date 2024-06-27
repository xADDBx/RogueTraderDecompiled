using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class SecurityDeclarationRocks
{
	public static PermissionSet ToPermissionSet(this SecurityDeclaration self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (TryProcessPermissionSetAttribute(self, out var set))
		{
			return set;
		}
		return CreatePermissionSet(self);
	}

	private static bool TryProcessPermissionSetAttribute(SecurityDeclaration declaration, out PermissionSet set)
	{
		set = null;
		if (!declaration.HasSecurityAttributes && declaration.SecurityAttributes.Count != 1)
		{
			return false;
		}
		SecurityAttribute securityAttribute = declaration.SecurityAttributes[0];
		if (!securityAttribute.AttributeType.IsTypeOf("System.Security.Permissions", "PermissionSetAttribute"))
		{
			return false;
		}
		PermissionSetAttribute permissionSetAttribute = new PermissionSetAttribute((System.Security.Permissions.SecurityAction)declaration.Action);
		CustomAttributeNamedArgument customAttributeNamedArgument = securityAttribute.Properties[0];
		string text = (string)customAttributeNamedArgument.Argument.Value;
		string name = customAttributeNamedArgument.Name;
		if (!(name == "XML"))
		{
			if (!(name == "Name"))
			{
				throw new NotImplementedException(customAttributeNamedArgument.Name);
			}
			permissionSetAttribute.Name = text;
		}
		else
		{
			permissionSetAttribute.XML = text;
		}
		set = permissionSetAttribute.CreatePermissionSet();
		return true;
	}

	private static PermissionSet CreatePermissionSet(SecurityDeclaration declaration)
	{
		PermissionSet permissionSet = new PermissionSet(PermissionState.None);
		foreach (SecurityAttribute securityAttribute in declaration.SecurityAttributes)
		{
			IPermission perm = CreatePermission(declaration, securityAttribute);
			permissionSet.AddPermission(perm);
		}
		return permissionSet;
	}

	private static IPermission CreatePermission(SecurityDeclaration declaration, SecurityAttribute attribute)
	{
		Type type = Type.GetType(attribute.AttributeType.FullName);
		if (type == null)
		{
			throw new ArgumentException("attribute");
		}
		System.Security.Permissions.SecurityAttribute obj = CreateSecurityAttribute(type, declaration) ?? throw new InvalidOperationException();
		CompleteSecurityAttribute(obj, attribute);
		return obj.CreatePermission();
	}

	private static void CompleteSecurityAttribute(System.Security.Permissions.SecurityAttribute security_attribute, SecurityAttribute attribute)
	{
		if (attribute.HasFields)
		{
			CompleteSecurityAttributeFields(security_attribute, attribute);
		}
		if (attribute.HasProperties)
		{
			CompleteSecurityAttributeProperties(security_attribute, attribute);
		}
	}

	private static void CompleteSecurityAttributeFields(System.Security.Permissions.SecurityAttribute security_attribute, SecurityAttribute attribute)
	{
		Type type = security_attribute.GetType();
		foreach (CustomAttributeNamedArgument field in attribute.Fields)
		{
			type.GetField(field.Name).SetValue(security_attribute, field.Argument.Value);
		}
	}

	private static void CompleteSecurityAttributeProperties(System.Security.Permissions.SecurityAttribute security_attribute, SecurityAttribute attribute)
	{
		Type type = security_attribute.GetType();
		foreach (CustomAttributeNamedArgument property in attribute.Properties)
		{
			type.GetProperty(property.Name).SetValue(security_attribute, property.Argument.Value, null);
		}
	}

	private static System.Security.Permissions.SecurityAttribute CreateSecurityAttribute(Type attribute_type, SecurityDeclaration declaration)
	{
		try
		{
			return (System.Security.Permissions.SecurityAttribute)Activator.CreateInstance(attribute_type, (System.Security.Permissions.SecurityAction)declaration.Action);
		}
		catch (MissingMethodException)
		{
			return (System.Security.Permissions.SecurityAttribute)Activator.CreateInstance(attribute_type, new object[0]);
		}
	}

	public static SecurityDeclaration ToSecurityDeclaration(this PermissionSet self, SecurityAction action, ModuleDefinition module)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		SecurityDeclaration securityDeclaration = new SecurityDeclaration(action);
		SecurityAttribute item = new SecurityAttribute(module.TypeSystem.LookupType("System.Security.Permissions", "PermissionSetAttribute"))
		{
			Properties = 
			{
				new CustomAttributeNamedArgument("XML", new CustomAttributeArgument(module.TypeSystem.String, self.ToXml().ToString()))
			}
		};
		securityDeclaration.SecurityAttributes.Add(item);
		return securityDeclaration;
	}
}
