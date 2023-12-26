using UnityProjectAnalysis;

namespace TestCases;

public class ScriptParsing
{
    [Test]
    public void CorrectNumberOfScriptsFound()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<ScriptItem> scripts = projectAnalyzer.GetScripts();
        Assert.That(scripts, Has.Count.EqualTo(5));
    }

    [Test]
    public void CorrectPathsFound()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<ScriptItem> scripts = projectAnalyzer.GetScripts();
        List<string> scriptPaths = scripts.Select(script => script.Path).ToList();
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/ScriptA.cs"));
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/ScriptB.cs"));
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/ScriptC.cs"));
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/UnusedScript.cs"));
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/Nested/UnusedScript2.cs"));
    }


    [Test]
    public void CorrectUnusedScriptsFound()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<ScriptItem> scripts = projectAnalyzer.GetUnusedScripts().ToList();
        Assert.That(scripts, Has.Count.EqualTo(2));
        List<string> scriptPaths = scripts.Select(script => script.Path).ToList();
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/UnusedScript.cs"));
        Assert.That(scriptPaths, Has.One.SamePath("Assets/Scripts/Nested/UnusedScript2.cs"));
    }
}