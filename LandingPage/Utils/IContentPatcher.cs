namespace LandingPage.Utils
{
    public interface IContentPatcher
    {
        PatchedContent Patch(string filename, string content);
    }
}
