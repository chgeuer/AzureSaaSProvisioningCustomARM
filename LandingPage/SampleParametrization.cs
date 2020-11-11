namespace LandingPage
{
    using Newtonsoft.Json.Linq;
    using Utils;

    public class SampleTemplateParametrization
    {
        public string SomeClientInformation { get; set; }

        public IContentPatcher CreatePatcher() 
        {
            void patchARM(JObject json)
            {
                var variables = json.SelectToken("$.variables");
 
                variables["dynamically_injected"] = new JObject(new JProperty("some_client_stuff", this.SomeClientInformation));
            }

            void patchUI(JObject json)
            {
                json["$greetings"] = this.SomeClientInformation;
            }

            return new GenericJSONPatcher(patchARM: patchARM, patchUI: patchUI);
        }
    }
}