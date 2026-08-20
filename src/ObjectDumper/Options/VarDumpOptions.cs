namespace ObjectDumper.Options;

public enum TypeNamingPolicy
{
    ShortName,
    NestedQualified,
    FullName
}

public enum NewLineStyle
{
    Auto,
    Unix,
    Windows
}

public enum StringLiteralStyle
{
    Auto,
    Escaped,
    Verbatim,
    Raw
}

public enum CollectionLiteralStyle
{
    Initializer,
    Expression
}
