namespace LandingPage.Utils
{
    using System;
    using Newtonsoft.Json.Linq;

    public class GenericJSONPatcher : IContentPatcher
    {
        private readonly Action<JObject> patchARM;
        private readonly Action<JObject> patchUI;

        public GenericJSONPatcher(Action<JObject> patchARM = null, Action<JObject> patchUI = null)
            => (this.patchARM, this.patchUI) = (patchARM, patchUI);

        string PatchImpl(string jsonStr, Action<JObject> patcherImpl)
        {
            var json = JObject.Parse(jsonStr);
            patcherImpl?.Invoke(json);
            return json.ToString();
        }

        public PatchedContent Patch(string filename, string content)
        {
            var patched = (filename) switch
            {
                "azuredeploy.json" => PatchImpl(content, patchARM),
                "createUiDefinition.json" => PatchImpl(content, patchUI),
                _ => content,
            };

            return new PatchedContent { Content = patched, ContentType = filename.DetermineContentTypeFromFilename() };
        }
    }
}