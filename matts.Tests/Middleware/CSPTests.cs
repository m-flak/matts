using Xunit;
using matts.Middleware;

namespace matts.Tests.Middleware;

public class CSPTests
{
    public CSPTests() { }

    public List<string> CspComponents => new()
    {
        $"{CSP.GetUrlCasing("DefaultSrc")} {CSP.Self}",
        $"{CSP.GetUrlCasing("ObjectSrc")} {CSP.None}",
        $"{CSP.GetUrlCasing("BaseUri")} {CSP.Self}",
        $"{CSP.GetUrlCasing("FrameAncestors")} {CSP.None}",
        $"{CSP.GetUrlCasing("Sandbox")} allow-forms allow-same-origin allow-scripts",
        $"{CSP.GetUrlCasing("ScriptSrc")} {CSP.Self}",
        $"{CSP.GetUrlCasing("FontSrc")} {CSP.Self} fonts.googleapis.com fonts.gstatic.com",
        $"{CSP.GetUrlCasing("ImgSrc")} {CSP.Wildcard} {CSP.Data}",
        $"{CSP.GetUrlCasing("StyleSrc")} {CSP.Self} {CSP.UnsafeInline} fonts.googleapis.com"
    };


    [Fact]
    public void CSP_Generates()
    {
        string cspString = CSP.DefaultPolicy.ToString();
        Assert.All(CspComponents, expected => cspString.Contains($"{expected}; "));
    }

    [Fact]
    public void CSP_Clones()
    {
        CSP jangoFett = CSP.DefaultPolicy.Clone();
        string cspString = jangoFett.ToString();
        Assert.All(CspComponents, expected => cspString.Contains($"{expected}; "));
    }

    [Fact]
    public void CSP_HandlesEmptyRules()
    {
        CSP jangoFett = CSP.DefaultPolicy.Clone();
        jangoFett.BaseUri = string.Empty;
        string cspString = jangoFett.ToString();
        Assert.DoesNotContain(CSP.GetUrlCasing("BaseUri"), cspString);
    }
}
