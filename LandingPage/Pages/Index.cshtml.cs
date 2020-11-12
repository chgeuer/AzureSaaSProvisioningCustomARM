namespace LandingPage.Pages
{
    using System;
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

        public LandingPageConfiguration Config { get; private set; }
        public string DeploymentURL { get; set; }
        public string Token { get; set; }
        public Func<string, string> GetAddress { get; set; } // Only needed for debugging...

        public IndexModel(LandingPageConfiguration cfg, ILogger<IndexModel> logger, LinkGenerator linkGenerator)
            => (this.Config, this.logger, this.linkGenerator) = (cfg, logger, linkGenerator);
       
        private SampleTemplateParametrization RetrieveParametrizationForClient()
            => new SampleTemplateParametrization { SomeClientInformation = "Greetings" };

        public IActionResult OnGet()
        {
            // This is the call where the publisher retrieves client-specific deployment information.
            Token = RetrieveParametrizationForClient().Serialize(Config.ApiKey);

            GetAddress = (string filename) => Flurl.Url.Decode(DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename), interpretPlusAsSpace: false);

            string encoded(string fn) => DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, fn);

            var info = Config.ARM;

            var prefix = "https://portal.azure.com/#create/Microsoft.Template";
            DeploymentURL = info.HasCustomUI
                ? $"{prefix}/uri/{encoded(info.TemplateName)}/createUIDefinitionUri/{encoded(info.UIDefinitionName)}"
                : $"{prefix}/uri/{encoded(info.TemplateName)}";

            return Page();
        }
    }
}