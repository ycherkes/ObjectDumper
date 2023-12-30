using System;
using System.Collections.Generic;
using System.IO;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class FileSystemInfoMiddleware : IObjectDescriptorMiddleware
{
    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
    {
        return @object switch
        {
            FileInfo fileInfo => new[]
            {
                new ReflectionDescriptor(fileInfo.FullName)
                {
                    ReflectionType = ReflectionType.ConstructorParameter
                }
            },
            DirectoryInfo directoryInfo => new[]
            {
                new ReflectionDescriptor(directoryInfo.FullName)
                {
                    ReflectionType = ReflectionType.ConstructorParameter
                }
            },
            DriveInfo driveInfo => new[]
            {
                new ReflectionDescriptor(driveInfo.Name)
                {
                    ReflectionType = ReflectionType.ConstructorParameter
                }
            },
            _ => prev()
        };
    }
}