﻿using System.ComponentModel;
using System.Reflection;
using YellowFlavor.Serialization.Embedded.CodeDom;

namespace YellowFlavor.Serialization.Implementation.Settings;

internal class VbSettings
{
    public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.New;
    public DateKind DateKind { get; set; } = DateKind.ConvertToUtc;
    public bool IgnoreNullValues { get; set; } = true;
    public bool IgnoreDefaultValues { get; set; } = true;
    public int MaxDepth { get; set; } = 25;
    public bool UseFullTypeName { get; set; } = false;
    public bool UseNamedArgumentsForReferenceRecordTypes { get; set; } = false;
    public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public bool WritablePropertiesOnly { get; set; } = true;
    public BindingFlags? GetFieldsBindingFlags { get; set; }
    public ListSortDirection? SortDirection { get; set; }
}