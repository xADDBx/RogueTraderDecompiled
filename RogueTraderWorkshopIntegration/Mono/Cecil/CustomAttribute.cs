using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[DebuggerDisplay("{AttributeType}")]
[ComVisible(false)]
public sealed class CustomAttribute : ICustomAttribute
{
	internal CustomAttributeValueProjection projection;

	internal readonly uint signature;

	internal bool resolved;

	private MethodReference constructor;

	private byte[] blob;

	internal Collection<CustomAttributeArgument> arguments;

	internal Collection<CustomAttributeNamedArgument> fields;

	internal Collection<CustomAttributeNamedArgument> properties;

	public MethodReference Constructor
	{
		get
		{
			return constructor;
		}
		set
		{
			constructor = value;
		}
	}

	public TypeReference AttributeType => constructor.DeclaringType;

	public bool IsResolved => resolved;

	public bool HasConstructorArguments
	{
		get
		{
			Resolve();
			return !arguments.IsNullOrEmpty();
		}
	}

	public Collection<CustomAttributeArgument> ConstructorArguments
	{
		get
		{
			Resolve();
			if (arguments == null)
			{
				Interlocked.CompareExchange(ref arguments, new Collection<CustomAttributeArgument>(), null);
			}
			return arguments;
		}
	}

	public bool HasFields
	{
		get
		{
			Resolve();
			return !fields.IsNullOrEmpty();
		}
	}

	public Collection<CustomAttributeNamedArgument> Fields
	{
		get
		{
			Resolve();
			if (fields == null)
			{
				Interlocked.CompareExchange(ref fields, new Collection<CustomAttributeNamedArgument>(), null);
			}
			return fields;
		}
	}

	public bool HasProperties
	{
		get
		{
			Resolve();
			return !properties.IsNullOrEmpty();
		}
	}

	public Collection<CustomAttributeNamedArgument> Properties
	{
		get
		{
			Resolve();
			if (properties == null)
			{
				Interlocked.CompareExchange(ref properties, new Collection<CustomAttributeNamedArgument>(), null);
			}
			return properties;
		}
	}

	internal bool HasImage
	{
		get
		{
			if (constructor != null)
			{
				return constructor.HasImage;
			}
			return false;
		}
	}

	internal ModuleDefinition Module => constructor.Module;

	internal CustomAttribute(uint signature, MethodReference constructor)
	{
		this.signature = signature;
		this.constructor = constructor;
		resolved = false;
	}

	public CustomAttribute(MethodReference constructor)
	{
		this.constructor = constructor;
		resolved = true;
	}

	public CustomAttribute(MethodReference constructor, byte[] blob)
	{
		this.constructor = constructor;
		resolved = false;
		this.blob = blob;
	}

	public byte[] GetBlob()
	{
		if (blob != null)
		{
			return blob;
		}
		if (!HasImage)
		{
			throw new NotSupportedException();
		}
		return Module.Read(ref blob, this, (CustomAttribute attribute, MetadataReader reader) => reader.ReadCustomAttributeBlob(attribute.signature));
	}

	private void Resolve()
	{
		if (resolved || !HasImage)
		{
			return;
		}
		lock (Module.SyncRoot)
		{
			if (resolved)
			{
				return;
			}
			Module.Read(this, delegate(CustomAttribute attribute, MetadataReader reader)
			{
				try
				{
					reader.ReadCustomAttributeSignature(attribute);
					resolved = true;
				}
				catch (ResolutionException)
				{
					if (arguments != null)
					{
						arguments.Clear();
					}
					if (fields != null)
					{
						fields.Clear();
					}
					if (properties != null)
					{
						properties.Clear();
					}
					resolved = false;
				}
			});
		}
	}
}
