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
        public DeploymentController(LandingPageConfiguration cfg) : base(cfg) { }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DeploymentControllerBase<T> : ControllerBase
        where T: IPatcherGenerator
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly LandingPageConfiguration cfg;

        public DeploymentControllerBase(LandingPageConfiguration cfg) => (this.cfg) = (cfg);

        [HttpGet]
        [NoCache]
        [Route("{token}/{filename}")]
        public async Task<IActionResult> Get(string token, string filename)
        {
            T templateInformation = MyExtensions.DecryptValidateDeserialize<T>(token, cfg.ApiKey);

            var templateUrl =  cfg.ARM.BaseAddress.AppendPathSegment(filename); 
            
            var unpatchedContent = await httpClient.GetByteArrayAsync(templateUrl);
            
            var patchedContent = templateInformation
                .CreatePatcher()
                .Patch(
                    content: unpatchedContent,
                    dft: cfg.ARM.DetermineFiletype(filename),
                    filename: filename);
            
            return File(
                fileContents: patchedContent.Content, 
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