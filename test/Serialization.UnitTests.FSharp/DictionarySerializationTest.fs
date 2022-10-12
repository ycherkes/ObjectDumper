module Tests

open Newtonsoft.Json
open YellowFlavor.Serialization.Implementation
open Xunit

[<Fact>]
let ``SerializeDictionaryOfAnonymousTypeCSharp`` () =
    let data = {| X = 1; Y = 2 |} 
    //let anonymousDictionary = System.Linq.Enumerable.ToDictionary((dict [ (1, data); (2, data); (3, data) ]), fst, snd)
    let anonymousDictionary = 
        let list = [ (1, data); (2, data); (3, data) ] 
        System.Linq.Enumerable.ToDictionary(list, fst, snd)
    let serializer = new CSharpSerializer()
    let result = serializer.Serialize(anonymousDictionary, JsonConvert.SerializeObject(
    {|
        IgnoreDefaultValues = true;
        IgnoreNullValues = true;
        MaxDepth = 5;
        UseFullTypeName = false;
        DateTimeInstantiation = "New";
        DateKind = "ConvertToUtc"
    |}))

    Assert.Equal(result,"var dictionaryOfAnonymousType = new []
{
    new 
    {
        Key = 1,
        Value = new 
        {
            X = 1,
            Y = 2
        }
    },
    new 
    {
        Key = 2,
        Value = new 
        {
            X = 1,
            Y = 2
        }
    },
    new 
    {
        Key = 3,
        Value = new 
        {
            X = 1,
            Y = 2
        }
    }
}.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
")
