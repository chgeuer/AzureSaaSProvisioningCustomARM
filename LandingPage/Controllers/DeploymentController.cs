namespace LandingPage.Controllers
{
    using Jose;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.StaticFiles;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Url2 = Flurl.Url;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoCacheAttribute : ActionFilterAttribute
    {
        // Ensure no cache somewhere in ARM believes it could cache
        public override void OnResultExecuting(ResultExecutingContext ctx)
        {
            ctx.HttpContext.Response.Headers["Cache-Control"] = new[] { "no-cache", "no-store", "private" };
            base.OnResultExecuting(ctx);
        }
    }

    public class DeploymentControllerAppConfiguration
    {
        public string ApiKey { get; set; }
    }

    public interface IContentPatcher
    {
        string Patch(string content);
    }

    public class AzureDeployTemplatePatcher : IContentPatcher
    {
        private readonly TemplateInformation spInfo;

        public AzureDeployTemplatePatcher(TemplateInformation spInfo) => (this.spInfo) = (spInfo);

        private static string PatchTemplateVariable(string templateJson, string variableName, TemplateInformation spInfo)
        {
            var template = JObject.Parse(templateJson);

            JToken variables = template.SelectToken("$.variables");
            variables[variableName] = new JObject(
                new JProperty("client_data", spInfo.SomeClientInformation)
                );

            return template.ToString();
        }

        string IContentPatcher.Patch(string content)
            => PatchTemplateVariable(templateJson: content, variableName: "dynamically_injected", spInfo: this.spInfo);
    }

    public class TemplateInformation
    {
        public string BaseAddress { get; set; }
        public string TemplateFile { get; set; }
        public string UIDefinitionsFile { get; set; }
        public string SomeClientInformation { get; set; }
    }

    internal static class MyExtensions
    {
        internal static string Serialize<T>(this T t, string secretKey) => 
            JWT.Encode(
               payload: JsonSerializer.Serialize(t),
               key: secretKey,
               alg: JweAlgorithm.PBES2_HS256_A128KW,
               enc: JweEncryption.A256CBC_HS512,
               compression: JweCompression.DEF);

        internal static T Deserialize<T>(string token, string secretKey) =>
            JsonSerializer.Deserialize<T>(JWT.Decode(token, secretKey));

        internal static string DetermineContentTypeFromFilename(this string filename)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out string contentType))
            {
                contentType = MediaTypeNames.Application.Octet;
            }
            return contentType;
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DeploymentController : ControllerBase
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly DeploymentControllerAppConfiguration cfg;

        public DeploymentController(DeploymentControllerAppConfiguration cfg) => (this.cfg) = (cfg);

        

        [HttpGet]
        [NoCache]
        [Route("{token}/{filename}")]
        public async Task<IActionResult> Get(string token, string filename)
        {
            var parametrization = MyExtensions.Deserialize<TemplateInformation>(token, cfg.ApiKey);

            // https://flurl.dev/docs/fluent-url/
            var templateUrl = Url2.Combine(parametrization.BaseAddress, filename);

            var unpatchedContent = await httpClient.GetStringAsync(templateUrl);

            IContentPatcher patcher = new AzureDeployTemplatePatcher(parametrization);

            var contentType = filename.DetermineContentTypeFromFilename();

            return (filename) switch
            {
                "azuredeploy.json" => Content(patcher.Patch(unpatchedContent), contentType),
                "createUiDefinition.json" => Content(unpatchedContent, contentType),
                _ => Content(unpatchedContent, contentType),
            };
        }
    }
}