using R3;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;


Console.WriteLine("hello");

// System.Reactive.Linq.Observable.Sample(

//JsonSerializerOptions.Default.TypeInfoResolver
// JsonSerializerOptions.Default.Converters.Add(new IgnoreCaseStringReactivePropertyJsonConverter());

var options = new JsonSerializerOptions
{
    Converters = { new IgnoreCaseStringReactivePropertyJsonConverter() },
};

var v = new IgnoreCaseStringReactiveProperty("aaa");

// var v = new ReactiveProperty<int>(1000);





var json = JsonSerializer.Serialize(v, options);
Console.WriteLine(json);
var v2 = JsonSerializer.Deserialize<IgnoreCaseStringReactiveProperty>(json, options);
Console.WriteLine(v2!.Value);




//[JsonConverter(typeof(IgnoreCaseStringReactivePropertyJsonConverter))]
public class IgnoreCaseStringReactiveProperty : ReactiveProperty<string>
{
    public IgnoreCaseStringReactiveProperty(string value)
        : base(value, StringComparer.OrdinalIgnoreCase)
    {

    }
}

internal class IgnoreCaseStringReactivePropertyJsonConverter : ReactivePropertyJsonConverter<string>
{
    protected override ReactiveProperty<string> CreateReactiveProperty(string value)
    {
        return new IgnoreCaseStringReactiveProperty(value);
    }
}
