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
        public TemplateDeploymentConfiguration TemplateConfiguration { get; private set; }

        public string DeploymentURL { get; set; }
        public string Token { get; set; }
        public Func<string, string> GetAddress { get; set; } // Only needed for debugging...

        public IndexModel(TemplateDeploymentConfiguration cfg, ILogger<IndexModel> logger, LinkGenerator linkGenerator) =>
            (this.TemplateConfiguration, this.logger, this.linkGenerator) = (cfg, logger, linkGenerator);
       
        private SampleTemplateParametrization RetrieveParametrizationForClient()
            => new SampleTemplateParametrization { SomeClientInformation = "Greetings" };

        public IActionResult OnGet()
        {
            // This is the call where the publisher retrieves client-specific deployment information.
            var parametrization = RetrieveParametrizationForClient();

            var spi = new TemplateInformation<SampleTemplateParametrization> { BaseAddress = TemplateConfiguration.BaseAdress, Parametrization = parametrization, };
            Token = spi.Serialize(TemplateConfiguration.ApiKey);
            GetAddress = (string filename) => Flurl.Url.Decode(DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename), interpretPlusAsSpace: false);

            string encoded(string filename) => DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename);

            DeploymentURL = string.IsNullOrEmpty(TemplateConfiguration.UIDefinitionName)
                ? $"https://portal.azure.com/#create/Microsoft.Template/uri/{encoded(TemplateConfiguration.TemplateName)}"
                : $"https://portal.azure.com/#create/Microsoft.Template/uri/{encoded(TemplateConfiguration.TemplateName)}/createUIDefinitionUri/{encoded(TemplateConfiguration.UIDefinitionName)}";

            return Page();
        }
    }
}