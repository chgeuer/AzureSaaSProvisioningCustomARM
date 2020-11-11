﻿namespace LandingPage.Pages
{
    using System;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Controllers;
    using Utils;

    public class IndexModel : PageModel
    {
        private readonly DeploymentControllerAppConfiguration _cfg;
        private readonly ILogger<IndexModel> _logger;
        private readonly LinkGenerator _linkGenerator;

        public string DeploymentURL { get; set; }
        private string token;

        public IndexModel(DeploymentControllerAppConfiguration cfg, ILogger<IndexModel> logger, LinkGenerator linkGenerator) =>
            (_cfg, _logger, _linkGenerator) = (cfg, logger, linkGenerator);

        private string EncodedAddress(string token, string filename)
            => Flurl.Url.Encode(new Uri(
                baseUri: new Uri(UriHelper.GetEncodedUrl(HttpContext.Request)),
                relativeUri: _linkGenerator.GetPathByAction(
                    action: nameof(DeploymentController.Get),
                    controller: nameof(DeploymentController).Replace("Controller", ""),
                    values: new { token, filename }
                )).AbsoluteUri);

        public string GetAddress(string filename) => Flurl.Url.Decode(EncodedAddress(this.token, filename), interpretPlusAsSpace: false);

        public IActionResult OnGet()
        {
            var spi = new TemplateInformation<SampleTemplateParametrization>
            {
                BaseAddress = "https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/101-vm-simple-windows",
                ARMTemplate = "azuredeploy.json",
                UIDefinitions = "createUiDefinition.json",
                Parametrization = new SampleTemplateParametrization { SomeClientInformation = "Greetings" },
            };
            this.token = spi.Serialize(_cfg.ApiKey);

            DeploymentURL = spi switch
            {
                { ARMTemplate: var tf, UIDefinitions: var ui } when !string.IsNullOrEmpty(ui) => $"https://portal.azure.com/#create/Microsoft.Template/uri/{EncodedAddress(token, tf)}/createUIDefinitionUri/{EncodedAddress(token, ui)}",
                { ARMTemplate: var tf } => $"https://portal.azure.com/#create/Microsoft.Template/uri/{EncodedAddress(token, tf)}",
            };

            return Page();
        }
    }
}