namespace LandingPage
{
    using Newtonsoft.Json.Linq;
    using Utils;

    public class SampleTemplateParametrization
    {
        public string SomeClientInformation { get; set; }
    }

    public class SampleAzureDeployTemplatePatcher : IContentPatcher
    {
        private readonly TemplateInformation<SampleTemplateParametrization> templateInformation;

        public SampleAzureDeployTemplatePatcher(TemplateInformation<SampleTemplateParametrization> templateInformation) => this.templateInformation = templateInformation;

        PatchedContent IContentPatcher.Patch(string filename, string content)
        {
            string PatchAzureDeployJson(string jsonStr, SampleTemplateParametrization parametrization) {
                var template = JObject.Parse(jsonStr);
                JToken variables = template.SelectToken("$.variables");
                variables["dynamically_injected"] = new JObject(
                    new JProperty("client_data", parametrization.SomeClientInformation)
                    );
                return template.ToString();
            }

            string PatchUIDefinitionjson(string jsonStr)
            {
                var json = JObject.Parse(jsonStr);
                // json["$greetings"] = "Hello World";
                return json.ToString();
            }

            var patched = (filename) switch
            {
                "azuredeploy.json" => PatchAzureDeployJson(content, templateInformation.Parametrization),
                "createUiDefinition.json" => PatchUIDefinitionjson(content),
                _ => content,
            };

            return new PatchedContent { Content = patched, ContentType = filename.DetermineContentTypeFromFilename() };
        }
    }
}