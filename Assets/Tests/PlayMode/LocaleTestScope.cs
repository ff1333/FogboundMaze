using NUnit.Framework;
namespace FogboundMaze.Tests
{
    [SetUpFixture]
    public sealed class LocaleTestScope
    {
        private bool chinese;
        [OneTimeSetUp] public void Before() { chinese=PortfolioSettings.Chinese;PortfolioSettings.SetLanguage(false); }
        [OneTimeTearDown] public void After() { PortfolioSettings.SetLanguage(chinese);PortfolioSettings.Save(); }
    }
}
