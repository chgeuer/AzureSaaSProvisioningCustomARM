namespace LandingPage.Controllers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Flurl;
    using Url2 = Flurl.Url;
    using Utils;

    public class DeploymentController : DeploymentControllerBase<SampleTemplateParametrization>
    {
        public DeploymentController(TemplateDeploymentConfiguration cfg) : base(cfg) { }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DeploymentControllerBase<T> : ControllerBase
        where T: IPatcherGenerator 
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly TemplateDeploymentConfiguration cfg;

        public DeploymentControllerBase(TemplateDeploymentConfiguration cfg) => (this.cfg) = (cfg);

        [HttpGet]
        [NoCache]
        [Route("{token}/{filename}")]
        public async Task<IActionResult> Get(string token, string filename)
        {
            TemplateInformation<T> templateInformation = MyExtensions.Deserialize<TemplateInformation<T>>(token, cfg.ApiKey);

            var templateUrl = templateInformation.BaseAddress.AppendPathSegment(filename); 
            
            var unpatchedContent = await httpClient.GetStringAsync(templateUrl);
            
            var patchedContent = templateInformation
                .Parametrization
                .CreatePatcher()
                .Patch(
                    content: unpatchedContent, 
                    filename: filename);
            
            return Content(
                content: patchedContent.Content, 
                contentType: patchedContent.ContentType);
        }

        internal static string EncodedAddress(HttpRequest request, LinkGenerator linkGenerator, string token, string filename)
        {
            var controllerPath = linkGenerator.GetPathByAction(
                action: nameof(DeploymentController.Get),
                controller: nameof(DeploymentController).Replace("Controller", ""),
                values: new { token = token, filename = filename });

            return Url2.Encode(request.GetEncodedUrl().AppendPathSegment(controllerPath)); // https://flurl.dev/docs/fluent-url/
        }
    }
}