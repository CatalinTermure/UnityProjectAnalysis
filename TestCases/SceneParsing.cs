using UnityProjectAnalysis;

namespace TestCases;

public class SceneParsing
{
    [Test]
    public void ScenePathsFound()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<string> scenes = projectAnalyzer.GetScenePaths();
        Assert.That(scenes, Has.Count.EqualTo(2));
        Assert.That(scenes, Has.One.SamePath("Assets/Scenes/SampleScene.unity"));
        Assert.That(scenes, Has.One.SamePath("Assets/Scenes/SecondScene.unity"));
    }
}