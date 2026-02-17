package com.yellowflavor.objectdumper.settings

import com.intellij.openapi.options.Configurable
import com.intellij.ui.components.JBCheckBox
import com.intellij.ui.components.JBTextField
import com.intellij.util.ui.FormBuilder
import java.awt.BorderLayout
import javax.swing.*

class ObjectDumperConfigurable : Configurable {

    private var mainPanel: JPanel? = null

    // ── Common ───────────────────────────────────────────────────────────
    private val commonMaxDepth = JBTextField()
    private val commonOperationTimeout = JBTextField()
    private val commonDumpDestination = JComboBox(DumpDestination.values())

    // ── C# ───────────────────────────────────────────────────────────────
    private val csharpEnabled = JBCheckBox("Enabled")
    private val csharpIgnoreNullValues = JBCheckBox("Ignore Null Values")
    private val csharpIgnoreDefaultValues = JBCheckBox("Ignore Default Values")
    private val csharpUseFullTypeName = JBCheckBox("Use Full Type Name")
    private val csharpDateTimeInstantiation = JComboBox(DateTimeInstantiation.values())
    private val csharpDateKind = JComboBox(DateKind.values())
    private val csharpMaxCollectionSize = JBTextField()
    private val csharpUseNamedArgumentsInConstructors = JBCheckBox("Use Named Arguments In Constructors")
    private val csharpUsePredefinedConstants = JBCheckBox("Use Predefined Constants")
    private val csharpUsePredefinedMethods = JBCheckBox("Use Predefined Methods")
    private val csharpGetPropertiesModifiers = JComboBox(BindingFlagsModifiers.values())
    private val csharpGetPropertiesInstanceOrStatic = JComboBox(BindingFlagsInstanceOrStatic.values())
    private val csharpIgnoreReadonlyProperties = JBCheckBox("Ignore Readonly Properties")
    private val csharpIndentString = JBTextField()
    private val csharpGetFieldsEnabled = JBCheckBox("Enable Get Fields")
    private val csharpGetFieldsModifiers = JComboBox(BindingFlagsModifiers.values())
    private val csharpGetFieldsInstanceOrStatic = JComboBox(BindingFlagsInstanceOrStatic.values())
    private val csharpSortDirection = JComboBox(arrayOf<SortDirection?>(null, SortDirection.Ascending, SortDirection.Descending))
    private val csharpGenerateVariableInitializer = JBCheckBox("Generate Variable Initializer")
    private val csharpPrimitiveCollectionLayout = JComboBox(CollectionLayout.values())
    private val csharpIntegralNumericFormat = JBTextField()

    // ── VB ───────────────────────────────────────────────────────────────
    private val vbEnabled = JBCheckBox("Enabled")
    private val vbIgnoreNullValues = JBCheckBox("Ignore Null Values")
    private val vbIgnoreDefaultValues = JBCheckBox("Ignore Default Values")
    private val vbUseFullTypeName = JBCheckBox("Use Full Type Name")
    private val vbDateTimeInstantiation = JComboBox(DateTimeInstantiation.values())
    private val vbDateKind = JComboBox(DateKind.values())
    private val vbMaxCollectionSize = JBTextField()
    private val vbUseNamedArgumentsInConstructors = JBCheckBox("Use Named Arguments In Constructors")
    private val vbUsePredefinedConstants = JBCheckBox("Use Predefined Constants")
    private val vbUsePredefinedMethods = JBCheckBox("Use Predefined Methods")
    private val vbGetPropertiesModifiers = JComboBox(BindingFlagsModifiers.values())
    private val vbGetPropertiesInstanceOrStatic = JComboBox(BindingFlagsInstanceOrStatic.values())
    private val vbIgnoreReadonlyProperties = JBCheckBox("Ignore Readonly Properties")
    private val vbIndentString = JBTextField()
    private val vbGetFieldsEnabled = JBCheckBox("Enable Get Fields")
    private val vbGetFieldsModifiers = JComboBox(BindingFlagsModifiers.values())
    private val vbGetFieldsInstanceOrStatic = JComboBox(BindingFlagsInstanceOrStatic.values())
    private val vbSortDirection = JComboBox(arrayOf<SortDirection?>(null, SortDirection.Ascending, SortDirection.Descending))
    private val vbGenerateVariableInitializer = JBCheckBox("Generate Variable Initializer")
    private val vbPrimitiveCollectionLayout = JComboBox(CollectionLayout.values())
    private val vbIntegralNumericFormat = JBTextField()

    // ── JSON ─────────────────────────────────────────────────────────────
    private val jsonEnabled = JBCheckBox("Enabled")
    private val jsonIgnoreNullValues = JBCheckBox("Ignore Null Values")
    private val jsonIgnoreDefaultValues = JBCheckBox("Ignore Default Values")
    private val jsonNamingStrategy = JComboBox(NamingStrategy.values())
    private val jsonSerializeEnumAsString = JBCheckBox("Serialize Enums As Strings")
    private val jsonTypeNameHandling = JComboBox(TypeNameHandling.values())
    private val jsonDateTimeZoneHandling = JComboBox(DateTimeZoneHandling.values())

    // ── XML ──────────────────────────────────────────────────────────────
    private val xmlEnabled = JBCheckBox("Enabled")
    private val xmlIgnoreNullValues = JBCheckBox("Ignore Null Values")
    private val xmlIgnoreDefaultValues = JBCheckBox("Ignore Default Values")
    private val xmlNamingStrategy = JComboBox(NamingStrategy.values())
    private val xmlSerializeEnumAsString = JBCheckBox("Serialize Enums As Strings")
    private val xmlUseFullTypeName = JBCheckBox("Use Full Type Name")
    private val xmlDateTimeZoneHandling = JComboBox(DateTimeZoneHandling.values())

    // ── YAML ─────────────────────────────────────────────────────────────
    private val yamlEnabled = JBCheckBox("Enabled")
    private val yamlNamingConvention = JComboBox(NamingConvention.values())

    override fun getDisplayName(): String = "Object Dumper"

    override fun createComponent(): JComponent {
        // Set up Get Fields enabled/disabled toggle behavior
        setupGetFieldsToggle(csharpGetFieldsEnabled, csharpGetFieldsModifiers, csharpGetFieldsInstanceOrStatic)
        setupGetFieldsToggle(vbGetFieldsEnabled, vbGetFieldsModifiers, vbGetFieldsInstanceOrStatic)

        // Custom renderer for nullable SortDirection comboboxes
        val sortDirectionRenderer = DefaultListCellRenderer()
        csharpSortDirection.renderer = object : DefaultListCellRenderer() {
            override fun getListCellRendererComponent(list: JList<*>?, value: Any?, index: Int, isSelected: Boolean, cellHasFocus: Boolean) =
                super.getListCellRendererComponent(list, value?.toString() ?: "(None)", index, isSelected, cellHasFocus)
        }
        vbSortDirection.renderer = object : DefaultListCellRenderer() {
            override fun getListCellRendererComponent(list: JList<*>?, value: Any?, index: Int, isSelected: Boolean, cellHasFocus: Boolean) =
                super.getListCellRendererComponent(list, value?.toString() ?: "(None)", index, isSelected, cellHasFocus)
        }

        val tabbedPane = JTabbedPane()
        tabbedPane.addTab("Common", buildCommonPanel())
        tabbedPane.addTab("C#", buildCSharpPanel())
        tabbedPane.addTab("Visual Basic", buildVisualBasicPanel())
        tabbedPane.addTab("JSON", buildJsonPanel())
        tabbedPane.addTab("XML", buildXmlPanel())
        tabbedPane.addTab("YAML", buildYamlPanel())

        mainPanel = JPanel(BorderLayout()).apply {
            add(tabbedPane, BorderLayout.CENTER)
        }
        reset()
        return mainPanel!!
    }

    private fun setupGetFieldsToggle(
        enabledCheckbox: JBCheckBox,
        modifiersCombo: JComboBox<BindingFlagsModifiers>,
        instanceOrStaticCombo: JComboBox<BindingFlagsInstanceOrStatic>
    ) {
        enabledCheckbox.addActionListener {
            modifiersCombo.isEnabled = enabledCheckbox.isSelected
            instanceOrStaticCombo.isEnabled = enabledCheckbox.isSelected
        }
    }

    // ── Panel builders ───────────────────────────────────────────────────

    private fun buildCommonPanel(): JPanel {
        return FormBuilder.createFormBuilder()
            .addLabeledComponent("Max Depth:", commonMaxDepth)
            .addLabeledComponent("Operation Timeout (seconds):", commonOperationTimeout)
            .addLabeledComponent("Dump To:", commonDumpDestination)
            .addComponentFillVertically(JPanel(), 0)
            .panel
    }

    private fun buildCSharpPanel(): JPanel {
        return FormBuilder.createFormBuilder()
            .addComponent(csharpEnabled)
            .addSeparator()
            .addComponent(csharpIgnoreNullValues)
            .addComponent(csharpIgnoreDefaultValues)
            .addComponent(csharpUseFullTypeName)
            .addLabeledComponent("DateTime Instantiation:", csharpDateTimeInstantiation)
            .addLabeledComponent("DateTime Kind:", csharpDateKind)
            .addLabeledComponent("Max Collection Size:", csharpMaxCollectionSize)
            .addComponent(csharpUseNamedArgumentsInConstructors)
            .addComponent(csharpUsePredefinedConstants)
            .addComponent(csharpUsePredefinedMethods)
            .addSeparator()
            .addLabeledComponent("Get Properties — Modifiers:", csharpGetPropertiesModifiers)
            .addLabeledComponent("Get Properties — Instance/Static:", csharpGetPropertiesInstanceOrStatic)
            .addComponent(csharpIgnoreReadonlyProperties)
            .addLabeledComponent("Indent String:", csharpIndentString)
            .addSeparator()
            .addComponent(csharpGetFieldsEnabled)
            .addLabeledComponent("Get Fields — Modifiers:", csharpGetFieldsModifiers)
            .addLabeledComponent("Get Fields — Instance/Static:", csharpGetFieldsInstanceOrStatic)
            .addSeparator()
            .addLabeledComponent("Sort Direction:", csharpSortDirection)
            .addComponent(csharpGenerateVariableInitializer)
            .addLabeledComponent("Primitive Collection Layout:", csharpPrimitiveCollectionLayout)
            .addLabeledComponent("Integral Numeric Format:", csharpIntegralNumericFormat)
            .addComponentFillVertically(JPanel(), 0)
            .panel
    }

    private fun buildVisualBasicPanel(): JPanel {
        return FormBuilder.createFormBuilder()
            .addComponent(vbEnabled)
            .addSeparator()
            .addComponent(vbIgnoreNullValues)
            .addComponent(vbIgnoreDefaultValues)
            .addComponent(vbUseFullTypeName)
            .addLabeledComponent("DateTime Instantiation:", vbDateTimeInstantiation)
            .addLabeledComponent("DateTime Kind:", vbDateKind)
            .addLabeledComponent("Max Collection Size:", vbMaxCollectionSize)
            .addComponent(vbUseNamedArgumentsInConstructors)
            .addComponent(vbUsePredefinedConstants)
            .addComponent(vbUsePredefinedMethods)
            .addSeparator()
            .addLabeledComponent("Get Properties — Modifiers:", vbGetPropertiesModifiers)
            .addLabeledComponent("Get Properties — Instance/Static:", vbGetPropertiesInstanceOrStatic)
            .addComponent(vbIgnoreReadonlyProperties)
            .addLabeledComponent("Indent String:", vbIndentString)
            .addSeparator()
            .addComponent(vbGetFieldsEnabled)
            .addLabeledComponent("Get Fields — Modifiers:", vbGetFieldsModifiers)
            .addLabeledComponent("Get Fields — Instance/Static:", vbGetFieldsInstanceOrStatic)
            .addSeparator()
            .addLabeledComponent("Sort Direction:", vbSortDirection)
            .addComponent(vbGenerateVariableInitializer)
            .addLabeledComponent("Primitive Collection Layout:", vbPrimitiveCollectionLayout)
            .addLabeledComponent("Integral Numeric Format:", vbIntegralNumericFormat)
            .addComponentFillVertically(JPanel(), 0)
            .panel
    }

    private fun buildJsonPanel(): JPanel {
        return FormBuilder.createFormBuilder()
            .addComponent(jsonEnabled)
            .addSeparator()
            .addComponent(jsonIgnoreNullValues)
            .addComponent(jsonIgnoreDefaultValues)
            .addLabeledComponent("Naming Strategy:", jsonNamingStrategy)
            .addComponent(jsonSerializeEnumAsString)
            .addLabeledComponent("Type Name Handling:", jsonTypeNameHandling)
            .addLabeledComponent("DateTime Zone Handling:", jsonDateTimeZoneHandling)
            .addComponentFillVertically(JPanel(), 0)
            .panel
    }

    private fun buildXmlPanel(): JPanel {
        return FormBuilder.createFormBuilder()
            .addComponent(xmlEnabled)
            .addSeparator()
            .addComponent(xmlIgnoreNullValues)
            .addComponent(xmlIgnoreDefaultValues)
            .addLabeledComponent("Naming Strategy:", xmlNamingStrategy)
            .addComponent(xmlSerializeEnumAsString)
            .addComponent(xmlUseFullTypeName)
            .addLabeledComponent("DateTime Zone Handling:", xmlDateTimeZoneHandling)
            .addComponentFillVertically(JPanel(), 0)
            .panel
    }

    private fun buildYamlPanel(): JPanel {
        return FormBuilder.createFormBuilder()
            .addComponent(yamlEnabled)
            .addSeparator()
            .addLabeledComponent("Naming Convention:", yamlNamingConvention)
            .addComponentFillVertically(JPanel(), 0)
            .panel
    }

    // ── isModified ───────────────────────────────────────────────────────

    override fun isModified(): Boolean {
        val s = ObjectDumperSettings.getInstance()

        // Common
        if (commonMaxDepth.text.toIntOrNull() != s.commonMaxDepth) return true
        if (commonOperationTimeout.text.toIntOrNull() != s.commonOperationTimeoutSeconds) return true
        if (commonDumpDestination.selectedItem != s.dumpDestination) return true

        // C#
        if (csharpEnabled.isSelected != s.csharpEnabled) return true
        if (csharpIgnoreNullValues.isSelected != s.csharpIgnoreNullValues) return true
        if (csharpIgnoreDefaultValues.isSelected != s.csharpIgnoreDefaultValues) return true
        if (csharpUseFullTypeName.isSelected != s.csharpUseFullTypeName) return true
        if (csharpDateTimeInstantiation.selectedItem != s.csharpDateTimeInstantiation) return true
        if (csharpDateKind.selectedItem != s.csharpDateKind) return true
        if (csharpMaxCollectionSize.text.toIntOrNull() != s.csharpMaxCollectionSize) return true
        if (csharpUseNamedArgumentsInConstructors.isSelected != s.csharpUseNamedArgumentsInConstructors) return true
        if (csharpUsePredefinedConstants.isSelected != s.csharpUsePredefinedConstants) return true
        if (csharpUsePredefinedMethods.isSelected != s.csharpUsePredefinedMethods) return true
        if (csharpGetPropertiesModifiers.selectedItem != s.csharpGetPropertiesBindingFlagsModifiers) return true
        if (csharpGetPropertiesInstanceOrStatic.selectedItem != s.csharpGetPropertiesBindingFlagsInstanceOrStatic) return true
        if (csharpIgnoreReadonlyProperties.isSelected != s.csharpIgnoreReadonlyProperties) return true
        if (csharpIndentString.text != s.csharpIndentString) return true
        if (isGetFieldsModified(csharpGetFieldsEnabled, csharpGetFieldsModifiers, csharpGetFieldsInstanceOrStatic,
                s.csharpGetFieldsBindingFlagsModifiers, s.csharpGetFieldsBindingFlagsInstanceOrStatic)) return true
        if (csharpSortDirection.selectedItem != s.csharpSortDirection) return true
        if (csharpGenerateVariableInitializer.isSelected != s.csharpGenerateVariableInitializer) return true
        if (csharpPrimitiveCollectionLayout.selectedItem != s.csharpPrimitiveCollectionLayout) return true
        if (csharpIntegralNumericFormat.text != s.csharpIntegralNumericFormat) return true

        // VB
        if (vbEnabled.isSelected != s.visualBasicEnabled) return true
        if (vbIgnoreNullValues.isSelected != s.vbIgnoreNullValues) return true
        if (vbIgnoreDefaultValues.isSelected != s.vbIgnoreDefaultValues) return true
        if (vbUseFullTypeName.isSelected != s.vbUseFullTypeName) return true
        if (vbDateTimeInstantiation.selectedItem != s.vbDateTimeInstantiation) return true
        if (vbDateKind.selectedItem != s.vbDateKind) return true
        if (vbMaxCollectionSize.text.toIntOrNull() != s.vbMaxCollectionSize) return true
        if (vbUseNamedArgumentsInConstructors.isSelected != s.vbUseNamedArgumentsInConstructors) return true
        if (vbUsePredefinedConstants.isSelected != s.vbUsePredefinedConstants) return true
        if (vbUsePredefinedMethods.isSelected != s.vbUsePredefinedMethods) return true
        if (vbGetPropertiesModifiers.selectedItem != s.vbGetPropertiesBindingFlagsModifiers) return true
        if (vbGetPropertiesInstanceOrStatic.selectedItem != s.vbGetPropertiesBindingFlagsInstanceOrStatic) return true
        if (vbIgnoreReadonlyProperties.isSelected != s.vbIgnoreReadonlyProperties) return true
        if (vbIndentString.text != s.vbIndentString) return true
        if (isGetFieldsModified(vbGetFieldsEnabled, vbGetFieldsModifiers, vbGetFieldsInstanceOrStatic,
                s.vbGetFieldsBindingFlagsModifiers, s.vbGetFieldsBindingFlagsInstanceOrStatic)) return true
        if (vbSortDirection.selectedItem != s.vbSortDirection) return true
        if (vbGenerateVariableInitializer.isSelected != s.vbGenerateVariableInitializer) return true
        if (vbPrimitiveCollectionLayout.selectedItem != s.vbPrimitiveCollectionLayout) return true
        if (vbIntegralNumericFormat.text != s.vbIntegralNumericFormat) return true

        // JSON
        if (jsonEnabled.isSelected != s.jsonEnabled) return true
        if (jsonIgnoreNullValues.isSelected != s.jsonIgnoreNullValues) return true
        if (jsonIgnoreDefaultValues.isSelected != s.jsonIgnoreDefaultValues) return true
        if (jsonNamingStrategy.selectedItem != s.jsonNamingStrategy) return true
        if (jsonSerializeEnumAsString.isSelected != s.jsonSerializeEnumAsString) return true
        if (jsonTypeNameHandling.selectedItem != s.jsonTypeNameHandling) return true
        if (jsonDateTimeZoneHandling.selectedItem != s.jsonDateTimeZoneHandling) return true

        // XML
        if (xmlEnabled.isSelected != s.xmlEnabled) return true
        if (xmlIgnoreNullValues.isSelected != s.xmlIgnoreNullValues) return true
        if (xmlIgnoreDefaultValues.isSelected != s.xmlIgnoreDefaultValues) return true
        if (xmlNamingStrategy.selectedItem != s.xmlNamingStrategy) return true
        if (xmlSerializeEnumAsString.isSelected != s.xmlSerializeEnumAsString) return true
        if (xmlUseFullTypeName.isSelected != s.xmlUseFullTypeName) return true
        if (xmlDateTimeZoneHandling.selectedItem != s.xmlDateTimeZoneHandling) return true

        // YAML
        if (yamlEnabled.isSelected != s.yamlEnabled) return true
        if (yamlNamingConvention.selectedItem != s.yamlNamingConvention) return true

        return false
    }

    private fun isGetFieldsModified(
        enabledCheckbox: JBCheckBox,
        modifiersCombo: JComboBox<BindingFlagsModifiers>,
        instanceOrStaticCombo: JComboBox<BindingFlagsInstanceOrStatic>,
        savedModifiers: BindingFlagsModifiers?,
        savedInstanceOrStatic: BindingFlagsInstanceOrStatic?
    ): Boolean {
        val isEnabled = enabledCheckbox.isSelected
        if (!isEnabled && savedModifiers == null && savedInstanceOrStatic == null) return false
        if (!isEnabled && (savedModifiers != null || savedInstanceOrStatic != null)) return true
        if (isEnabled && savedModifiers == null) return true
        return modifiersCombo.selectedItem != savedModifiers || instanceOrStaticCombo.selectedItem != savedInstanceOrStatic
    }

    // ── apply ────────────────────────────────────────────────────────────

    override fun apply() {
        val s = ObjectDumperSettings.getInstance()

        // Common
        s.commonMaxDepth = commonMaxDepth.text.toIntOrNull() ?: 25
        s.commonOperationTimeoutSeconds = commonOperationTimeout.text.toIntOrNull() ?: 10
        s.dumpDestination = commonDumpDestination.selectedItem as DumpDestination

        // C#
        s.csharpEnabled = csharpEnabled.isSelected
        s.csharpIgnoreNullValues = csharpIgnoreNullValues.isSelected
        s.csharpIgnoreDefaultValues = csharpIgnoreDefaultValues.isSelected
        s.csharpUseFullTypeName = csharpUseFullTypeName.isSelected
        s.csharpDateTimeInstantiation = csharpDateTimeInstantiation.selectedItem as DateTimeInstantiation
        s.csharpDateKind = csharpDateKind.selectedItem as DateKind
        s.csharpMaxCollectionSize = csharpMaxCollectionSize.text.toIntOrNull()?.coerceAtLeast(1) ?: Int.MAX_VALUE
        s.csharpUseNamedArgumentsInConstructors = csharpUseNamedArgumentsInConstructors.isSelected
        s.csharpUsePredefinedConstants = csharpUsePredefinedConstants.isSelected
        s.csharpUsePredefinedMethods = csharpUsePredefinedMethods.isSelected
        s.csharpGetPropertiesBindingFlagsModifiers = csharpGetPropertiesModifiers.selectedItem as BindingFlagsModifiers
        s.csharpGetPropertiesBindingFlagsInstanceOrStatic = csharpGetPropertiesInstanceOrStatic.selectedItem as BindingFlagsInstanceOrStatic
        s.csharpIgnoreReadonlyProperties = csharpIgnoreReadonlyProperties.isSelected
        s.csharpIndentString = csharpIndentString.text
        applyGetFields(csharpGetFieldsEnabled, csharpGetFieldsModifiers, csharpGetFieldsInstanceOrStatic) { mod, inst ->
            s.csharpGetFieldsBindingFlagsModifiers = mod
            s.csharpGetFieldsBindingFlagsInstanceOrStatic = inst
        }
        s.csharpSortDirection = csharpSortDirection.selectedItem as? SortDirection
        s.csharpGenerateVariableInitializer = csharpGenerateVariableInitializer.isSelected
        s.csharpPrimitiveCollectionLayout = csharpPrimitiveCollectionLayout.selectedItem as CollectionLayout
        s.csharpIntegralNumericFormat = csharpIntegralNumericFormat.text

        // VB
        s.visualBasicEnabled = vbEnabled.isSelected
        s.vbIgnoreNullValues = vbIgnoreNullValues.isSelected
        s.vbIgnoreDefaultValues = vbIgnoreDefaultValues.isSelected
        s.vbUseFullTypeName = vbUseFullTypeName.isSelected
        s.vbDateTimeInstantiation = vbDateTimeInstantiation.selectedItem as DateTimeInstantiation
        s.vbDateKind = vbDateKind.selectedItem as DateKind
        s.vbMaxCollectionSize = vbMaxCollectionSize.text.toIntOrNull()?.coerceAtLeast(1) ?: Int.MAX_VALUE
        s.vbUseNamedArgumentsInConstructors = vbUseNamedArgumentsInConstructors.isSelected
        s.vbUsePredefinedConstants = vbUsePredefinedConstants.isSelected
        s.vbUsePredefinedMethods = vbUsePredefinedMethods.isSelected
        s.vbGetPropertiesBindingFlagsModifiers = vbGetPropertiesModifiers.selectedItem as BindingFlagsModifiers
        s.vbGetPropertiesBindingFlagsInstanceOrStatic = vbGetPropertiesInstanceOrStatic.selectedItem as BindingFlagsInstanceOrStatic
        s.vbIgnoreReadonlyProperties = vbIgnoreReadonlyProperties.isSelected
        s.vbIndentString = vbIndentString.text
        applyGetFields(vbGetFieldsEnabled, vbGetFieldsModifiers, vbGetFieldsInstanceOrStatic) { mod, inst ->
            s.vbGetFieldsBindingFlagsModifiers = mod
            s.vbGetFieldsBindingFlagsInstanceOrStatic = inst
        }
        s.vbSortDirection = vbSortDirection.selectedItem as? SortDirection
        s.vbGenerateVariableInitializer = vbGenerateVariableInitializer.isSelected
        s.vbPrimitiveCollectionLayout = vbPrimitiveCollectionLayout.selectedItem as CollectionLayout
        s.vbIntegralNumericFormat = vbIntegralNumericFormat.text

        // JSON
        s.jsonEnabled = jsonEnabled.isSelected
        s.jsonIgnoreNullValues = jsonIgnoreNullValues.isSelected
        s.jsonIgnoreDefaultValues = jsonIgnoreDefaultValues.isSelected
        s.jsonNamingStrategy = jsonNamingStrategy.selectedItem as NamingStrategy
        s.jsonSerializeEnumAsString = jsonSerializeEnumAsString.isSelected
        s.jsonTypeNameHandling = jsonTypeNameHandling.selectedItem as TypeNameHandling
        s.jsonDateTimeZoneHandling = jsonDateTimeZoneHandling.selectedItem as DateTimeZoneHandling

        // XML
        s.xmlEnabled = xmlEnabled.isSelected
        s.xmlIgnoreNullValues = xmlIgnoreNullValues.isSelected
        s.xmlIgnoreDefaultValues = xmlIgnoreDefaultValues.isSelected
        s.xmlNamingStrategy = xmlNamingStrategy.selectedItem as NamingStrategy
        s.xmlSerializeEnumAsString = xmlSerializeEnumAsString.isSelected
        s.xmlUseFullTypeName = xmlUseFullTypeName.isSelected
        s.xmlDateTimeZoneHandling = xmlDateTimeZoneHandling.selectedItem as DateTimeZoneHandling

        // YAML
        s.yamlEnabled = yamlEnabled.isSelected
        s.yamlNamingConvention = yamlNamingConvention.selectedItem as NamingConvention
    }

    private fun applyGetFields(
        enabledCheckbox: JBCheckBox,
        modifiersCombo: JComboBox<BindingFlagsModifiers>,
        instanceOrStaticCombo: JComboBox<BindingFlagsInstanceOrStatic>,
        setter: (BindingFlagsModifiers?, BindingFlagsInstanceOrStatic?) -> Unit
    ) {
        if (enabledCheckbox.isSelected) {
            setter(modifiersCombo.selectedItem as BindingFlagsModifiers, instanceOrStaticCombo.selectedItem as BindingFlagsInstanceOrStatic)
        } else {
            setter(null, null)
        }
    }

    // ── reset ────────────────────────────────────────────────────────────

    override fun reset() {
        val s = ObjectDumperSettings.getInstance()

        // Common
        commonMaxDepth.text = s.commonMaxDepth.toString()
        commonOperationTimeout.text = s.commonOperationTimeoutSeconds.toString()
        commonDumpDestination.selectedItem = s.dumpDestination

        // C#
        csharpEnabled.isSelected = s.csharpEnabled
        csharpIgnoreNullValues.isSelected = s.csharpIgnoreNullValues
        csharpIgnoreDefaultValues.isSelected = s.csharpIgnoreDefaultValues
        csharpUseFullTypeName.isSelected = s.csharpUseFullTypeName
        csharpDateTimeInstantiation.selectedItem = s.csharpDateTimeInstantiation
        csharpDateKind.selectedItem = s.csharpDateKind
        csharpMaxCollectionSize.text = s.csharpMaxCollectionSize.toString()
        csharpUseNamedArgumentsInConstructors.isSelected = s.csharpUseNamedArgumentsInConstructors
        csharpUsePredefinedConstants.isSelected = s.csharpUsePredefinedConstants
        csharpUsePredefinedMethods.isSelected = s.csharpUsePredefinedMethods
        csharpGetPropertiesModifiers.selectedItem = s.csharpGetPropertiesBindingFlagsModifiers
        csharpGetPropertiesInstanceOrStatic.selectedItem = s.csharpGetPropertiesBindingFlagsInstanceOrStatic
        csharpIgnoreReadonlyProperties.isSelected = s.csharpIgnoreReadonlyProperties
        csharpIndentString.text = s.csharpIndentString
        resetGetFields(csharpGetFieldsEnabled, csharpGetFieldsModifiers, csharpGetFieldsInstanceOrStatic,
            s.csharpGetFieldsBindingFlagsModifiers, s.csharpGetFieldsBindingFlagsInstanceOrStatic)
        csharpSortDirection.selectedItem = s.csharpSortDirection
        csharpGenerateVariableInitializer.isSelected = s.csharpGenerateVariableInitializer
        csharpPrimitiveCollectionLayout.selectedItem = s.csharpPrimitiveCollectionLayout
        csharpIntegralNumericFormat.text = s.csharpIntegralNumericFormat

        // VB
        vbEnabled.isSelected = s.visualBasicEnabled
        vbIgnoreNullValues.isSelected = s.vbIgnoreNullValues
        vbIgnoreDefaultValues.isSelected = s.vbIgnoreDefaultValues
        vbUseFullTypeName.isSelected = s.vbUseFullTypeName
        vbDateTimeInstantiation.selectedItem = s.vbDateTimeInstantiation
        vbDateKind.selectedItem = s.vbDateKind
        vbMaxCollectionSize.text = s.vbMaxCollectionSize.toString()
        vbUseNamedArgumentsInConstructors.isSelected = s.vbUseNamedArgumentsInConstructors
        vbUsePredefinedConstants.isSelected = s.vbUsePredefinedConstants
        vbUsePredefinedMethods.isSelected = s.vbUsePredefinedMethods
        vbGetPropertiesModifiers.selectedItem = s.vbGetPropertiesBindingFlagsModifiers
        vbGetPropertiesInstanceOrStatic.selectedItem = s.vbGetPropertiesBindingFlagsInstanceOrStatic
        vbIgnoreReadonlyProperties.isSelected = s.vbIgnoreReadonlyProperties
        vbIndentString.text = s.vbIndentString
        resetGetFields(vbGetFieldsEnabled, vbGetFieldsModifiers, vbGetFieldsInstanceOrStatic,
            s.vbGetFieldsBindingFlagsModifiers, s.vbGetFieldsBindingFlagsInstanceOrStatic)
        vbSortDirection.selectedItem = s.vbSortDirection
        vbGenerateVariableInitializer.isSelected = s.vbGenerateVariableInitializer
        vbPrimitiveCollectionLayout.selectedItem = s.vbPrimitiveCollectionLayout
        vbIntegralNumericFormat.text = s.vbIntegralNumericFormat

        // JSON
        jsonEnabled.isSelected = s.jsonEnabled
        jsonIgnoreNullValues.isSelected = s.jsonIgnoreNullValues
        jsonIgnoreDefaultValues.isSelected = s.jsonIgnoreDefaultValues
        jsonNamingStrategy.selectedItem = s.jsonNamingStrategy
        jsonSerializeEnumAsString.isSelected = s.jsonSerializeEnumAsString
        jsonTypeNameHandling.selectedItem = s.jsonTypeNameHandling
        jsonDateTimeZoneHandling.selectedItem = s.jsonDateTimeZoneHandling

        // XML
        xmlEnabled.isSelected = s.xmlEnabled
        xmlIgnoreNullValues.isSelected = s.xmlIgnoreNullValues
        xmlIgnoreDefaultValues.isSelected = s.xmlIgnoreDefaultValues
        xmlNamingStrategy.selectedItem = s.xmlNamingStrategy
        xmlSerializeEnumAsString.isSelected = s.xmlSerializeEnumAsString
        xmlUseFullTypeName.isSelected = s.xmlUseFullTypeName
        xmlDateTimeZoneHandling.selectedItem = s.xmlDateTimeZoneHandling

        // YAML
        yamlEnabled.isSelected = s.yamlEnabled
        yamlNamingConvention.selectedItem = s.yamlNamingConvention
    }

    private fun resetGetFields(
        enabledCheckbox: JBCheckBox,
        modifiersCombo: JComboBox<BindingFlagsModifiers>,
        instanceOrStaticCombo: JComboBox<BindingFlagsInstanceOrStatic>,
        savedModifiers: BindingFlagsModifiers?,
        savedInstanceOrStatic: BindingFlagsInstanceOrStatic?
    ) {
        val isEnabled = savedModifiers != null || savedInstanceOrStatic != null
        enabledCheckbox.isSelected = isEnabled
        modifiersCombo.isEnabled = isEnabled
        instanceOrStaticCombo.isEnabled = isEnabled
        if (isEnabled) {
            modifiersCombo.selectedItem = savedModifiers ?: BindingFlagsModifiers.Public
            instanceOrStaticCombo.selectedItem = savedInstanceOrStatic ?: BindingFlagsInstanceOrStatic.Instance
        } else {
            modifiersCombo.selectedItem = BindingFlagsModifiers.Public
            instanceOrStaticCombo.selectedItem = BindingFlagsInstanceOrStatic.Instance
        }
    }

    override fun disposeUIResources() {
        mainPanel = null
    }
}
