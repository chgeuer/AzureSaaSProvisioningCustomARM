namespace LandingPage.Utils
{
    using System;
    using Newtonsoft.Json.Linq;

    public interface IContentPatcher
    {
        PatchedContent Patch(string content, string filename);
    }

    public class GenericJSONPatcher : IContentPatcher
    {
        private readonly Action<JObject> patchARM, patchUI;

        public GenericJSONPatcher(Action<JObject> patchARM = null, Action<JObject> patchUI = null)
            => (this.patchARM, this.patchUI) = (patchARM, patchUI);

        PatchedContent IContentPatcher.Patch(string content, string filename)
        {
            var patched = (filename) switch
            {
                "azuredeploy.json" => PatchImpl(content, patchARM),
                "createUiDefinition.json" => PatchImpl(content, patchUI),
                _ => content,
            };

            var contentType = filename.DetermineContentTypeFromFilename();

            return new PatchedContent { Content = patched, ContentType = contentType };
        }

        private static string PatchImpl(string jsonStr, Action<JObject> patcherImpl)
        {
            var json = JObject.Parse(jsonStr);
            patcherImpl?.Invoke(json);
            return json.ToString();
        }
    }
}