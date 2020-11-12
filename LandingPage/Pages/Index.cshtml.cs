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
        public LandingPageConfiguration TemplateConfiguration { get; private set; }

        public string DeploymentURL { get; set; }
        public string Token { get; set; }
        public Func<string, string> GetAddress { get; set; } // Only needed for debugging...

        public IndexModel(LandingPageConfiguration cfg, ILogger<IndexModel> logger, LinkGenerator linkGenerator) =>
            (this.TemplateConfiguration, this.logger, this.linkGenerator) = (cfg, logger, linkGenerator);
       
        private SampleTemplateParametrization RetrieveParametrizationForClient()
            => new SampleTemplateParametrization { SomeClientInformation = "Greetings" };

        public IActionResult OnGet()
        {
            // This is the call where the publisher retrieves client-specific deployment information.
            var parametrization = RetrieveParametrizationForClient();

            var info = TemplateConfiguration.ARMDeploymentInfo;
            Token = new TemplateInformation<SampleTemplateParametrization> { Parametrization = parametrization, }.Serialize(TemplateConfiguration.ApiKey);
            GetAddress = (string filename) => Flurl.Url.Decode(DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename), interpretPlusAsSpace: false);

            string encoded(string filename) => DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename);

            DeploymentURL = string.IsNullOrEmpty(TemplateConfiguration.ARMDeploymentInfo.UIDefinitionName)
                ? $"https://portal.azure.com/#create/Microsoft.Template/uri/{encoded(info.TemplateName)}"
                : $"https://portal.azure.com/#create/Microsoft.Template/uri/{encoded(info.TemplateName)}/createUIDefinitionUri/{encoded(info.UIDefinitionName)}";

            return Page();
        }
    }
}