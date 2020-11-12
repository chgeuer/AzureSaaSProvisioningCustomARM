namespace LandingPage.Utils
{
    using Newtonsoft.Json;
    using System.Collections;

    public class ARMDeploymentInfo
    {
        [JsonProperty("base")]
        public string BaseAddress { get; set; }

        [JsonProperty("armfile")]
        public string TemplateName { get; set; }

        [JsonProperty("uifile")]
        public string UIDefinitionName { get; set; }

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

    public class TemplateInformation<T>
    {
        public T Parametrization { get; set; }
    }

    public class LandingPageConfiguration
    {
        public string ApiKey { get; set; }

        public ARMDeploymentInfo ARMDeploymentInfo { get; set; }
    }
}