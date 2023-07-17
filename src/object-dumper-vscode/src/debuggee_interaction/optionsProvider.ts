import * as vscode from 'vscode';

export class OptionsProvider{
    
    public getOptions(language: string): any
    {
        const extensionConfiguration = vscode.workspace.getConfiguration().get('objectDumper') as any;

        switch(language)
		{
			case "cs":{
                  return {
					ignoreDefaultValues: extensionConfiguration.csharp.ignoreDefaultValues,
					ignoreNullValues: extensionConfiguration.csharp.ignoreNullValues,
					maxDepth: extensionConfiguration.common.maxDepth,
					useFullTypeName: extensionConfiguration.csharp.useFullTypeName,
					dateTimeInstantiation: extensionConfiguration.csharp.dateTimeInstantiation,
					dateKind: extensionConfiguration.csharp.dateKind,
					useNamedArgumentsForReferenceRecordTypes: extensionConfiguration.csharp.useNamedArgumentsForReferenceRecordTypes,
					getPropertiesBindingFlags: this.getBindingFlags(extensionConfiguration.csharp.getPropertiesBindingFlagsModifiers, extensionConfiguration.csharp.getPropertiesBindingFlagsInstanceOrStatic),
					writablePropertiesOnly: extensionConfiguration.csharp.writablePropertiesOnly,
					getFieldsBindingFlags: this.getBindingFlags(extensionConfiguration.csharp.getFieldsBindingFlagsModifiers, extensionConfiguration.csharp.getFieldsBindingFlagsInstanceOrStatic),
					sortDirection: extensionConfiguration.csharp.sortDirection
				  };
			}
			case "vb":{
				return {
				  ignoreDefaultValues: extensionConfiguration.vb.ignoreDefaultValues,
				  ignoreNullValues: extensionConfiguration.vb.ignoreNullValues,
				  maxDepth: extensionConfiguration.common.maxDepth,
				  useFullTypeName: extensionConfiguration.vb.useFullTypeName,
				  dateTimeInstantiation: extensionConfiguration.vb.dateTimeInstantiation,
				  dateKind: extensionConfiguration.vb.dateKind,
				  useNamedArgumentsForReferenceRecordTypes: extensionConfiguration.vb.useNamedArgumentsForReferenceRecordTypes,
				  getPropertiesBindingFlags: this.getBindingFlags(extensionConfiguration.vb.getPropertiesBindingFlagsModifiers, extensionConfiguration.vb.getPropertiesBindingFlagsInstanceOrStatic),
				  writablePropertiesOnly: extensionConfiguration.vb.writablePropertiesOnly,
				  getFieldsBindingFlags: this.getBindingFlags(extensionConfiguration.vb.getFieldsBindingFlagsModifiers, extensionConfiguration.vb.getFieldsBindingFlagsInstanceOrStatic),
				  sortDirection: extensionConfiguration.vb.sortDirection
				};
			}
			case "json":{
				return {
					ignoreDefaultValues: extensionConfiguration.json.ignoreDefaultValues,
					ignoreNullValues: extensionConfiguration.json.ignoreNullValues,
					maxDepth: extensionConfiguration.common.maxDepth,
					typeNameHandling: extensionConfiguration.json.typeNameHandling,
					namingStrategy: extensionConfiguration.json.namingStrategy,
					serializeEnumAsString: extensionConfiguration.json.serializeEnumAsString,
					dateTimeZoneHandling: extensionConfiguration.json.dateTimeZoneHandling
				};
			}
			case "xml":{
				return {
					ignoreDefaultValues: extensionConfiguration.xml.ignoreDefaultValues,
					ignoreNullValues: extensionConfiguration.xml.ignoreNullValues,
					maxDepth: extensionConfiguration.common.maxDepth,
					namingStrategy: extensionConfiguration.xml.namingStrategy,
					dateTimeZoneHandling: extensionConfiguration.xml.dateTimeZoneHandling
				};
			}
			case "yaml":{
				return {
					maxDepth: extensionConfiguration.common.maxDepth,
					namingConvention: extensionConfiguration.yaml.namingConvention
				};
			}
			default: return {};
		}
    }

	private getBindingFlags(modifiers: string|null, instance: string|null): string|null{
		var result: string|null = null;
		if(modifiers ==="all")
		{
			result = "public,nonPublic";
		}
		else if(modifiers !== null){
			result = modifiers;
		}
		if(instance ==="all"){
			if(result){
			    result+=",instance,static";
		    } else{
				result="instance,static";
			}
		}
		else if(instance !== null){
            if(result){
			    result+=`,${instance}`;
		    } else{
				result=`${instance}`;
			}
		}
		return result;
	}
}