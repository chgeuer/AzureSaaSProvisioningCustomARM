namespace LandingPage
{
    using Newtonsoft.Json.Linq;
    using Utils;

    public class SampleTemplateParametrization
    {
        public string SomeClientInformation { get; set; }

        public IContentPatcher GetContentPatcher() => 
            new GenericJSONPatcher<SampleTemplateParametrization>(
                mutateArmTemplate: PatchARMTemplate,
                mutateUIDefinition: PatchUIDefinition,
                t: this);

        internal static void PatchARMTemplate(JObject json, SampleTemplateParametrization parametrization)
        {
            var variables = json.SelectToken("$.variables");
            variables["dynamically_injected"] = new JObject(new JProperty("some_client_stuff", parametrization.SomeClientInformation));
        }

        internal static void PatchUIDefinition(JObject json, SampleTemplateParametrization parametrization)
        {
            json["$greetings"] = "Hello World";
        }
    }
}