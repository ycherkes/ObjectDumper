﻿using System;
using System.IO;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class FileSystemInfoMiddleware : IObjectDescriptorMiddleware
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
    {
        return @object switch
        {
            FileInfo fileInfo => new ObjectDescription
            {
                Type = objectType,
                ConstructorArguments = 
                [
                    new ConstructorArgumentDescription
                    { 
                        Value = fileInfo.FullName,
                        Type = typeof(string),
                        Name = "fileName"
                    }
                ]
            },
            DirectoryInfo directoryInfo => new ObjectDescription
            {
                Type = objectType,
                ConstructorArguments =
                [
                    new ConstructorArgumentDescription
                    {
                        Value = directoryInfo.FullName,
                        Type = typeof(string),
                        Name = "path"
                    }
                ]
            },
            DriveInfo driveInfo => new ObjectDescription
            {
                Type = objectType,
                ConstructorArguments =
                [
                    new ConstructorArgumentDescription
                    {
                        Value = driveInfo.Name,
                        Type = typeof(string),
                        Name = "driveName"
                    }
                ]
            },
            _ => prev()
        };
    }
}