namespace LandingPage.Utils
{
    using System;
    using Newtonsoft.Json.Linq;

    public class GenericJSONPatcher<T> : IContentPatcher
    {
        private readonly Action<JObject, T> mutateArmTemplate;
        private readonly Action<JObject, T> mutateUIDefinition;
        private readonly T t;

        public GenericJSONPatcher(Action<JObject, T> mutateArmTemplate = null, Action<JObject, T> mutateUIDefinition = null, T t = default)
            => (this.mutateArmTemplate, this.mutateUIDefinition, this.t) = (mutateArmTemplate, mutateUIDefinition, t);

        public PatchedContent Patch(string filename, string content)
        {
            string PatchAzureDeployJson(string jsonStr)
            {
                var json = JObject.Parse(jsonStr);
                mutateArmTemplate?.Invoke(json, t);
                return json.ToString();
            }

            string PatchUIDefinitionjson(string jsonStr)
            {
                var json = JObject.Parse(jsonStr);
                mutateUIDefinition?.Invoke(json, t);
                return json.ToString();
            }

            var patched = (filename) switch
            {
                "azuredeploy.json" => PatchAzureDeployJson(content),
                "createUiDefinition.json" => PatchUIDefinitionjson(content),
                _ => content,
            };

            return new PatchedContent { Content = patched, ContentType = filename.DetermineContentTypeFromFilename() };
        }
    }
}