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
    
    [Test]
    public void SceneNamesFound()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<string> scenes = projectAnalyzer.GetSceneNames();
        Assert.That(scenes, Has.Count.EqualTo(2));
        Assert.That(scenes, Contains.Item("SampleScene"));
        Assert.That(scenes, Contains.Item("SecondScene"));
    }
    
    [Test]
    public void SceneGameObjectsFound()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<GameObjectItem> gameObjects = projectAnalyzer.GetGameObjectsFromScene("SampleScene");
        Assert.That(gameObjects, Has.Count.EqualTo(5));
        Assert.That(gameObjects, Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 136406834));
        Assert.That(gameObjects, Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 705507993));
        Assert.That(gameObjects, Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 963194225));
        Assert.That(gameObjects, Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 2115756237));
        Assert.That(gameObjects, Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 2118425382));
        
        gameObjects = projectAnalyzer.GetGameObjectsFromScene("SecondScene");
        Assert.That(gameObjects, Has.Count.EqualTo(6));
    }
}