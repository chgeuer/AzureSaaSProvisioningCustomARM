namespace LandingPage.Pages
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Controllers;
    using Utils;
    using System;

    public class IndexModel : PageModel
    {
        private readonly DeploymentControllerAppConfiguration cfg;
        private readonly ILogger<IndexModel> logger;
        private readonly LinkGenerator linkGenerator;

        public string DeploymentURL { get; set; }
        public string Token { get; set; }
        public Func<string, string> GetAddress { get; set; } // Only needed for debugging...

        public IndexModel(DeploymentControllerAppConfiguration cfg, ILogger<IndexModel> logger, LinkGenerator linkGenerator) =>
            (this.cfg, this.logger, this.linkGenerator) = (cfg, logger, linkGenerator);
       
        public IActionResult OnGet()
        {
            var spi = new TemplateInformation<SampleTemplateParametrization>
            {
                BaseAddress = "https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-vm-simple-windows",
                ARMTemplate = "azuredeploy.json",
                UIDefinitions = "createUiDefinition.json",
                Parametrization = new SampleTemplateParametrization { SomeClientInformation = "Greetings" },
            };
            Token = spi.Serialize(cfg.ApiKey);
            GetAddress = (string filename) => Flurl.Url.Decode(DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename), interpretPlusAsSpace: false);

            string encoded(string filename) => DeploymentController.EncodedAddress(HttpContext.Request, linkGenerator, Token, filename);

            DeploymentURL = spi switch
            {
                { ARMTemplate: var tf, UIDefinitions: var ui } when !string.IsNullOrEmpty(ui) => $"https://portal.azure.com/#create/Microsoft.Template/uri/{encoded(tf)}/createUIDefinitionUri/{encoded(ui)}",
                { ARMTemplate: var tf } => $"https://portal.azure.com/#create/Microsoft.Template/uri/{encoded(tf)}",
            };

            return Page();
        }
    }
}