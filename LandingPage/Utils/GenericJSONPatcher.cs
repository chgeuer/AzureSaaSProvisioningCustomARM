namespace LandingPage.Utils
{
    using System;
    using System.Text;
    using Newtonsoft.Json.Linq;

    public interface IContentPatcher
    {
        PatchedContent Patch(byte[] content, string filename);
    }

    public class GenericJSONPatcher : IContentPatcher
    {
        private readonly Action<JObject> patchARM, patchUI;

        public GenericJSONPatcher(Action<JObject> patchARM = null, Action<JObject> patchUI = null)
            => (this.patchARM, this.patchUI) = (patchARM, patchUI);

        PatchedContent IContentPatcher.Patch(byte[] content, string filename)
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

        private static byte[] PatchImpl(byte[] content, Action<JObject> patcherImpl)
        {
            if (patcherImpl == null)
            {
                return content;
            }

            var encoding = Encoding.UTF8;

            var jsonStr = encoding.GetString(content);
            var json = JObject.Parse(jsonStr);
 
            patcherImpl(json); 
            
            return encoding.GetBytes(json.ToString());
        }
    }
}