namespace LandingPage.Utils
{
    public class PatchedContent
    {
        public byte[] Content { get; set; }

        public string ContentType { get; set; }
    }

    public enum DeploymentFileType
    {
        ARMTemplate = 0,
        UIDefinitions = 1,
        Other = 2
    }

    public interface IContentPatcher
    {
        PatchedContent Patch(byte[] content, DeploymentFileType dft, string filename);
    }

    public interface IPatcherGenerator
    {
        IContentPatcher CreatePatcher();
    }
}