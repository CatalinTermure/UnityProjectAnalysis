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

    private static bool GameObjectContainsComponent(GameObjectItem gameObject, int classId, long fileId)
    {
        return gameObject.Components.Any(component => component.ClassId == classId && component.FileId == fileId);
    }


    private static bool TransformHasCorrectGameObjectId(GameObjectItem gameObject)
    {
        return gameObject.Components.Any(component =>
            component.ClassId == ClassIds.Transform &&
            ((TransformItem)component).GameObjectFileId == gameObject.FileId);
    }

    [Test]
    public void GameObjectTransformsAssignedCorrectly()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<GameObjectItem> gameObjects = projectAnalyzer.GetGameObjectsFromScene("SampleScene");
        Assert.That(gameObjects,
            Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 136406834).And
                .Matches<GameObjectItem>(gameObject =>
                    GameObjectContainsComponent(gameObject, ClassIds.Transform, 136406838)));
        Assert.That(gameObjects,
            Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 705507993).And
                .Matches<GameObjectItem>(gameObject =>
                    GameObjectContainsComponent(gameObject, ClassIds.Transform, 705507995)));
        Assert.That(gameObjects,
            Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 963194225).And
                .Matches<GameObjectItem>(gameObject =>
                    GameObjectContainsComponent(gameObject, ClassIds.Transform, 963194228)));
        Assert.That(gameObjects,
            Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 2115756237).And
                .Matches<GameObjectItem>(gameObject =>
                    GameObjectContainsComponent(gameObject, ClassIds.Transform, 2115756241)));
        Assert.That(gameObjects,
            Has.One.Matches<GameObjectItem>(gameObject => gameObject.FileId == 2118425382).And
                .Matches<GameObjectItem>(gameObject =>
                    GameObjectContainsComponent(gameObject, ClassIds.Transform, 2118425386)));
    }

    [Test]
    public void GameObjectTransformsHaveCorrectGameObjectId()
    {
        var projectAnalyzer = new ProjectAnalyzer("tests/TestCase01");
        List<GameObjectItem> gameObjects = projectAnalyzer.GetGameObjectsFromScene("SampleScene");
        Assert.That(gameObjects, Has.All.Matches<GameObjectItem>(TransformHasCorrectGameObjectId));

        gameObjects = projectAnalyzer.GetGameObjectsFromScene("SecondScene");
        Assert.That(gameObjects, Has.All.Matches<GameObjectItem>(TransformHasCorrectGameObjectId));
    }
}