package com.yellowflavor.objectdumper.settings

import com.intellij.openapi.components.*
import com.intellij.util.xmlb.XmlSerializerUtil

@State(
    name = "ObjectDumperSettings",
    storages = [Storage("ObjectDumperSettings.xml")]
)
class ObjectDumperSettings : PersistentStateComponent<ObjectDumperSettings> {

    // ── Common Settings ──────────────────────────────────────────────────
    var commonMaxDepth: Int = 25
    var commonOperationTimeoutSeconds: Int = 10
    var dumpDestination: DumpDestination = DumpDestination.NEW_TAB

    // ── C# Settings ──────────────────────────────────────────────────────
    var csharpEnabled: Boolean = true
    var csharpIgnoreNullValues: Boolean = true
    var csharpIgnoreDefaultValues: Boolean = true
    var csharpUseFullTypeName: Boolean = false
    var csharpDateTimeInstantiation: DateTimeInstantiation = DateTimeInstantiation.Parse
    var csharpDateKind: DateKind = DateKind.Original
    var csharpMaxCollectionSize: Int = Int.MAX_VALUE
    var csharpUseNamedArgumentsInConstructors: Boolean = false
    var csharpUsePredefinedConstants: Boolean = true
    var csharpUsePredefinedMethods: Boolean = true
    var csharpGetPropertiesBindingFlagsModifiers: BindingFlagsModifiers = BindingFlagsModifiers.Public
    var csharpGetPropertiesBindingFlagsInstanceOrStatic: BindingFlagsInstanceOrStatic = BindingFlagsInstanceOrStatic.Instance
    var csharpIgnoreReadonlyProperties: Boolean = true
    var csharpIndentString: String = "    "
    var csharpGetFieldsBindingFlagsModifiers: BindingFlagsModifiers? = null
    var csharpGetFieldsBindingFlagsInstanceOrStatic: BindingFlagsInstanceOrStatic? = null
    var csharpSortDirection: SortDirection? = null
    var csharpGenerateVariableInitializer: Boolean = true
    var csharpPrimitiveCollectionLayout: CollectionLayout = CollectionLayout.MultiLine
    var csharpIntegralNumericFormat: String = "D"

    // ── Visual Basic Settings ────────────────────────────────────────────
    var visualBasicEnabled: Boolean = true
    var vbIgnoreNullValues: Boolean = true
    var vbIgnoreDefaultValues: Boolean = true
    var vbUseFullTypeName: Boolean = false
    var vbDateTimeInstantiation: DateTimeInstantiation = DateTimeInstantiation.Parse
    var vbDateKind: DateKind = DateKind.Original
    var vbMaxCollectionSize: Int = Int.MAX_VALUE
    var vbUseNamedArgumentsInConstructors: Boolean = false
    var vbUsePredefinedConstants: Boolean = true
    var vbUsePredefinedMethods: Boolean = true
    var vbGetPropertiesBindingFlagsModifiers: BindingFlagsModifiers = BindingFlagsModifiers.Public
    var vbGetPropertiesBindingFlagsInstanceOrStatic: BindingFlagsInstanceOrStatic = BindingFlagsInstanceOrStatic.Instance
    var vbIgnoreReadonlyProperties: Boolean = true
    var vbIndentString: String = "    "
    var vbGetFieldsBindingFlagsModifiers: BindingFlagsModifiers? = null
    var vbGetFieldsBindingFlagsInstanceOrStatic: BindingFlagsInstanceOrStatic? = null
    var vbSortDirection: SortDirection? = null
    var vbGenerateVariableInitializer: Boolean = true
    var vbPrimitiveCollectionLayout: CollectionLayout = CollectionLayout.MultiLine
    var vbIntegralNumericFormat: String = "D"

    // ── JSON Settings ────────────────────────────────────────────────────
    var jsonEnabled: Boolean = true
    var jsonIgnoreNullValues: Boolean = true
    var jsonIgnoreDefaultValues: Boolean = true
    var jsonNamingStrategy: NamingStrategy = NamingStrategy.CamelCase
    var jsonSerializeEnumAsString: Boolean = true
    var jsonTypeNameHandling: TypeNameHandling = TypeNameHandling.None
    var jsonDateTimeZoneHandling: DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind

    // ── XML Settings ─────────────────────────────────────────────────────
    var xmlEnabled: Boolean = true
    var xmlIgnoreNullValues: Boolean = true
    var xmlIgnoreDefaultValues: Boolean = true
    var xmlNamingStrategy: NamingStrategy = NamingStrategy.Default
    var xmlSerializeEnumAsString: Boolean = true
    var xmlUseFullTypeName: Boolean = false
    var xmlDateTimeZoneHandling: DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind

    // ── YAML Settings ────────────────────────────────────────────────────
    var yamlEnabled: Boolean = true
    var yamlNamingConvention: NamingConvention = NamingConvention.Null

    override fun getState(): ObjectDumperSettings = this

    override fun loadState(state: ObjectDumperSettings) {
        XmlSerializerUtil.copyBean(state, this)
    }

    companion object {
        fun getInstance(): ObjectDumperSettings = service()
    }

    /**
     * Compute .NET BindingFlags integer bitmask from the decomposed modifiers + instance/static dropdowns.
     * .NET values: Public=16, NonPublic=32, Instance=4, Static=8
     * Returns null if both parameters are null (meaning "not set" / disabled).
     */
    fun computeBindingFlags(modifiers: BindingFlagsModifiers?, instanceOrStatic: BindingFlagsInstanceOrStatic?): Int? {
        if (modifiers == null && instanceOrStatic == null) return null
        var flags = 0
        when (modifiers) {
            BindingFlagsModifiers.Public -> flags = flags or 16
            BindingFlagsModifiers.NonPublic -> flags = flags or 32
            BindingFlagsModifiers.All -> flags = flags or 16 or 32
            null -> {}
        }
        when (instanceOrStatic) {
            BindingFlagsInstanceOrStatic.Instance -> flags = flags or 4
            BindingFlagsInstanceOrStatic.Static -> flags = flags or 8
            BindingFlagsInstanceOrStatic.All -> flags = flags or 4 or 8
            null -> {}
        }
        return if (flags == 0) null else flags
    }
}

// ── Enums ────────────────────────────────────────────────────────────────────

enum class DumpDestination {
    NEW_TAB,
    CLIPBOARD,
    DEBUG_CONSOLE
}

/** Matches C# DateTimeInstantiation enum. JSON value: PascalCase string. */
enum class DateTimeInstantiation(val jsonValue: String) {
    New("New"),
    Parse("Parse");
}

/** Matches C# DateKind enum. JSON value: PascalCase string. */
enum class DateKind(val jsonValue: String) {
    ConvertToUtc("ConvertToUtc"),
    Original("Original");
}

/** Matches C# CollectionLayout enum. JSON value: PascalCase string. */
enum class CollectionLayout(val jsonValue: String) {
    MultiLine("MultiLine"),
    SingleLine("SingleLine");
}

/** Matches C# NamingStrategy enum. JSON value: PascalCase string. */
enum class NamingStrategy(val jsonValue: String) {
    Default("Default"),
    CamelCase("CamelCase"),
    KebabCase("KebabCase"),
    SnakeCase("SnakeCase");
}

/** Matches C# NamingConvention enum (YAML). JSON value: PascalCase string. */
enum class NamingConvention(val jsonValue: String) {
    CamelCase("CamelCase"),
    Hyphenated("Hyphenated"),
    LowerCase("LowerCase"),
    Null("Null"),
    PascalCase("PascalCase"),
    Underscored("Underscored");
}

/** Matches Newtonsoft.Json TypeNameHandling enum. JSON value: PascalCase string. */
enum class TypeNameHandling(val jsonValue: String) {
    None("None"),
    Objects("Objects"),
    Arrays("Arrays"),
    All("All"),
    Auto("Auto");
}

/** Matches Newtonsoft.Json DateTimeZoneHandling enum. JSON value: PascalCase string. */
enum class DateTimeZoneHandling(val jsonValue: String) {
    Local("Local"),
    Utc("Utc"),
    Unspecified("Unspecified"),
    RoundtripKind("RoundtripKind");
}

/** Matches C# ListSortDirection. JSON value: PascalCase string. */
enum class SortDirection(val jsonValue: String) {
    Ascending("Ascending"),
    Descending("Descending");
}

/**
 * Decomposed BindingFlags for UI dropdowns — Modifiers part.
 * Combines with BindingFlagsInstanceOrStatic to produce the integer bitmask.
 */
enum class BindingFlagsModifiers {
    Public,
    NonPublic,
    All
}

/**
 * Decomposed BindingFlags for UI dropdowns — Instance/Static part.
 */
enum class BindingFlagsInstanceOrStatic {
    Instance,
    Static,
    All
}
