namespace LandingPage
{
    using Newtonsoft.Json.Linq;
    using Utils;

    public class SampleTemplateParametrization
    {
        public string SomeClientInformation { get; set; }

        public IContentPatcher GetContentPatcher() =>
            new GenericJSONPatcher(patchARM: PatchARMTemplate, patchUI: PatchUIDefinition);

        internal void PatchARMTemplate(JObject json)
        {
            var variables = json.SelectToken("$.variables");
            variables["dynamically_injected"] = new JObject(new JProperty("some_client_stuff", this.SomeClientInformation));
        }

        internal void PatchUIDefinition(JObject json)
        {
            json["$greetings"] = "Hello World";
        }
    }
}