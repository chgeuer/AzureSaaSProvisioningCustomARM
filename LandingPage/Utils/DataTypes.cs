namespace LandingPage.Utils
{
    using Newtonsoft.Json;

    public class ARMDeploymentInfo
    {
        [JsonProperty("base")]
        public string BaseAddress { get; set; }

        [JsonProperty("armfile")]
        public string TemplateName { get; set; }

        [JsonProperty("uifile")]
        public string UIDefinitionName { get; set; }

        public bool HasCustomUI => !string.IsNullOrEmpty(UIDefinitionName);

        public DeploymentFileType DetermineFiletype(string filename)
        {
            if (string.Equals(filename, TemplateName))
            {
                return DeploymentFileType.ARMTemplate;
            }
            else if (string.Equals(filename, UIDefinitionName))
            {
                return DeploymentFileType.UIDefinitions;
            }
            else
            {
                return DeploymentFileType.Other;
            }
        }
    }

    public class LandingPageConfiguration
    {
        public string ApiKey { get; set; }

        public ARMDeploymentInfo ARM { get; set; }
    }
}