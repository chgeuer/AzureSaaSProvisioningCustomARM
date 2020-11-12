namespace LandingPage.Utils
{
    using System;
    using System.Text;
    using Newtonsoft.Json.Linq;

    public class GenericJSONPatcher : IContentPatcher
    {
        private readonly Action<JObject> patchARM, patchUI;

        public GenericJSONPatcher(Action<JObject> patchARM = null, Action<JObject> patchUI = null)
            => (this.patchARM, this.patchUI) = (patchARM, patchUI);

        PatchedContent IContentPatcher.Patch(byte[] content, DeploymentFileType dft, string filename)
        {
            var patched = (dft) switch
            {
                DeploymentFileType.ARMTemplate => PatchImpl(content, patchARM),
                DeploymentFileType.UIDefinitions=> PatchImpl(content, patchUI),
                DeploymentFileType.Other => content,
                _ => throw new NotSupportedException($"Unsupported value for {nameof(dft)} ({typeof(DeploymentFileType)})")
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