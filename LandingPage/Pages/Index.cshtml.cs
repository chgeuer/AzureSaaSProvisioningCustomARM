namespace LandingPage.Pages
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Controllers;
    using Utils;

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly LinkGenerator linkGenerator;
        private string Token { get; set; }

        public LandingPageConfiguration Config { get; private set; }
        public string DeploymentURL { get; private set; }

        public IndexModel(LandingPageConfiguration cfg, ILogger<IndexModel> logger, LinkGenerator linkGenerator)
            => (this.Config, this.logger, this.linkGenerator) = (cfg, logger, linkGenerator);
       
        private SampleTemplateParametrization RetrieveParametrizationForClient()
            => new SampleTemplateParametrization { SomeClientInformation = "Greetings" };

        public IActionResult OnGet()
        {
            // This is the call where the publisher retrieves client-specific deployment information.
            Token = RetrieveParametrizationForClient().SerializeEncryptSign(Config.ApiKey);

            string protect(string fn) => DeploymentController.ProtectAddress(
                HttpContext.Request, linkGenerator, Token, fn);

            var (azuredeploy_json, createuidefinition_json) =
                (Config.ARM.TemplateName, Config.ARM.UIDefinitionName);

            var prefix = "https://portal.azure.com/#create/Microsoft.Template";
            DeploymentURL = Config.ARM.HasCustomUI
                ? $"{prefix}/uri/{protect(azuredeploy_json)}/createUIDefinitionUri/{protect(createuidefinition_json)}"
                : $"{prefix}/uri/{protect(azuredeploy_json)}";

            return Page();
        }
    }
}