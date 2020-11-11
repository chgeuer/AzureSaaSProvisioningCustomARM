namespace LandingPage.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Utils;
    using Url2 = Flurl.Url;

    [Route("api/[controller]")]
    [ApiController]
    public class DeploymentController : ControllerBase
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly DeploymentControllerAppConfiguration cfg;

        internal static string EncodedAddress(HttpRequest request, LinkGenerator linkGenerator, string token, string filename)
            => Url2.Encode(new Uri(
                baseUri: new Uri(UriHelper.GetEncodedUrl(request)),
                relativeUri: linkGenerator.GetPathByAction(
                    action: nameof(DeploymentController.Get),
                    controller: nameof(DeploymentController).Replace("Controller", ""),
                    values: new { token, filename }
                )).AbsoluteUri);

        public DeploymentController(DeploymentControllerAppConfiguration cfg) => (this.cfg) = (cfg);

        [HttpGet]
        [NoCache]
        [Route("{token}/{filename}")]
        public async Task<IActionResult> Get(string token, string filename)
        {
            var parametrization = MyExtensions.Deserialize<TemplateInformation<SampleTemplateParametrization>>(token, cfg.ApiKey);
            var templateUrl = Url2.Combine(parametrization.BaseAddress, filename); // https://flurl.dev/docs/fluent-url/
            var unpatchedContent = await httpClient.GetStringAsync(templateUrl);
            IContentPatcher patcher = new SampleAzureDeployTemplatePatcher(parametrization);
            var patchedContent = patcher.Patch(filename, unpatchedContent);
            return Content(patchedContent.Content, patchedContent.ContentType);
        }
    }
}