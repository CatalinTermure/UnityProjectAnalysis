using YamlDotNet.RepresentationModel;

namespace UnityProjectAnalysis;

public class ProjectAnalyzer
{
    private readonly string _projectPath;

    public ProjectAnalyzer(string projectPath)
    {
        _projectPath = projectPath;
    }

    public List<string> GetScenePaths()
    {
        string scenesDirectory = Path.Combine(_projectPath, "Assets", "Scenes");
        string[] scenePaths = Directory.GetFiles(scenesDirectory, "*.unity", SearchOption.AllDirectories);

        return scenePaths.Select(scenePath => Path.GetRelativePath(_projectPath, scenePath)).ToList();
    }

    public List<string> GetSceneNames()
    {
        return GetScenePaths().Select(Path.GetFileNameWithoutExtension).ToList()!;
    }

    public List<GameObjectItem> GetGameObjectsFromScene(string sceneName)
    {
        List<GameObjectItem> gameObjects = new();
        
        string scenePath = Path.Combine(_projectPath, "Assets", "Scenes", $"{sceneName}.unity");
        var sceneYaml = new YamlStream();
        sceneYaml.Load(new StreamReader(scenePath));
        
        // TODO: This can be optimized with parallelism
        foreach (YamlDocument yamlDocument in sceneYaml)
        {
            string[] tags = yamlDocument.RootNode.Tag.Value.Split(':');
            int classId = Convert.ToInt32(tags[2]);
            if (classId != ClassIds.GameObject)
            {
                continue;
            }
            
            long fileId = Convert.ToInt64(yamlDocument.RootNode.Anchor.Value);
            gameObjects.Add(new GameObjectItem(fileId));
        }

        return gameObjects;
    }
}