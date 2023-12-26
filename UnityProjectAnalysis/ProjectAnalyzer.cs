using YamlDotNet.RepresentationModel;

namespace UnityProjectAnalysis;

public class ProjectAnalyzer
{
    private readonly string _projectPath;

    public ProjectAnalyzer(string projectPath)
    {
        _projectPath = projectPath;
    }

    public List<string> GetSceneRelativePaths()
    {
        string scenesDirectory = Path.Combine(_projectPath, "Assets", "Scenes");
        string[] scenePaths = Directory.GetFiles(scenesDirectory, "*.unity", SearchOption.AllDirectories);

        return scenePaths.Select(scenePath => Path.GetRelativePath(_projectPath, scenePath)).ToList();
    }

    public List<string> GetSceneNames()
    {
        return GetSceneRelativePaths().Select(Path.GetFileNameWithoutExtension).ToList()!;
    }

    public List<ScriptItem> GetScripts()
    {
        string scriptsDirectory = Path.Combine(_projectPath, "Assets", "Scripts");
        string[] scriptPaths = Directory.GetFiles(scriptsDirectory, "*.cs", SearchOption.AllDirectories);

        List<ScriptItem> scriptItems = new();
        foreach (string scriptPath in scriptPaths)
        {
            string scriptMetaPath = Path.ChangeExtension(scriptPath, ".cs.meta");
            if (!File.Exists(scriptMetaPath))
            {
                throw new FileNotFoundException($"Script meta file not found: {scriptMetaPath}");
            }

            var scriptMetaYaml = new YamlStream();
            scriptMetaYaml.Load(new StreamReader(scriptMetaPath));
            var rootNode = (YamlMappingNode)scriptMetaYaml.Documents[0].RootNode;
            var guidNode = (YamlScalarNode)rootNode.Children[new YamlScalarNode("guid")];
            Guid guid = Guid.Parse(guidNode.Value!);

            scriptItems.Add(new ScriptItem(Path.GetRelativePath(_projectPath, scriptPath), guid));
        }

        return scriptItems;
    }

    public IEnumerable<ScriptItem> GetUnusedScripts()
    {
        List<ScriptItem> scripts = GetScripts();
        List<string> sceneRelativePaths = GetSceneRelativePaths();

        foreach (ScriptItem script in scripts)
        {
            bool isUsed = false;
            foreach (string sceneRelativePath in sceneRelativePaths)
            {
                string scenePath = Path.Combine(_projectPath, sceneRelativePath);
                string sceneYamlText = File.ReadAllText(scenePath);
                // if its GUID is in the scene file, it's used in some way
                if (sceneYamlText.Contains(script.Guid.ToString("N")))
                {
                    isUsed = true;
                }
            }
            
            if (!isUsed)
            {
                yield return script;
            }
        }
    }

    public List<GameObjectItem> GetGameObjectsFromScene(string sceneName)
    {
        List<GameObjectItem> gameObjects = new();

        string scenePath = Path.Combine(_projectPath, "Assets", "Scenes", $"{sceneName}.unity");
        var sceneYaml = new YamlStream();
        sceneYaml.Load(new StreamReader(scenePath));

        Dictionary<long, UnityYamlItem> itemsByFileId = new();
        foreach (YamlDocument yamlDocument in sceneYaml)
        {
            string[] tags = yamlDocument.RootNode.Tag.Value.Split(':');
            int classId = Convert.ToInt32(tags[2]);
            long fileId = Convert.ToInt64(yamlDocument.RootNode.Anchor.Value);

            if (classId != ClassIds.Transform)
            {
                continue;
            }

            itemsByFileId[fileId] = ParseTransform(yamlDocument);
        }

        // This is in a separate loop because we need to parse all the components first
        foreach (YamlDocument yamlDocument in sceneYaml)
        {
            string[] tags = yamlDocument.RootNode.Tag.Value.Split(':');
            int classId = Convert.ToInt32(tags[2]);

            if (classId != ClassIds.GameObject)
            {
                continue;
            }

            var rootNode = (YamlMappingNode)yamlDocument.RootNode;
            long fileId = Convert.ToInt64(yamlDocument.RootNode.Anchor.Value);
            var gameObjectMapping = (YamlMappingNode)rootNode.Children[new YamlScalarNode("GameObject")];
            var componentsNode = (YamlSequenceNode)gameObjectMapping.Children[new YamlScalarNode("m_Component")];
            IEnumerable<UnityYamlItem> components =
                componentsNode.SelectMany(node =>
                    {
                        var componentWrapperNode = (YamlMappingNode)node;
                        var fileIdNode =
                            (YamlMappingNode)componentWrapperNode.Children[new YamlScalarNode("component")];
                        long componentFileId = Convert.ToInt64(((YamlScalarNode)fileIdNode
                            .Children[new YamlScalarNode("fileID")]).Value);

                        return itemsByFileId.TryGetValue(componentFileId, out UnityYamlItem? value)
                            ? new[] { value }
                            : Array.Empty<UnityYamlItem>();
                    }
                );

            string name = ((YamlScalarNode)gameObjectMapping.Children[new YamlScalarNode("m_Name")]).Value!;

            gameObjects.Add(new GameObjectItem(fileId, name, components.ToArray()));
        }

        return gameObjects;
    }

    private static TransformItem ParseTransform(YamlDocument yamlDocument)
    {
        var rootNode = (YamlMappingNode)yamlDocument.RootNode;
        long fileId = Convert.ToInt64(rootNode.Anchor.Value);
        var transformMapping = (YamlMappingNode)rootNode.Children[new YamlScalarNode("Transform")];
        var parentNode = (YamlMappingNode)transformMapping.Children[new YamlScalarNode("m_Father")];
        long parentFileId = Convert.ToInt64(((YamlScalarNode)parentNode.Children[new YamlScalarNode("fileID")]).Value);

        var gameObjectNode = (YamlMappingNode)transformMapping.Children[new YamlScalarNode("m_GameObject")];
        long gameObjectFileId =
            Convert.ToInt64(((YamlScalarNode)gameObjectNode.Children[new YamlScalarNode("fileID")]).Value);

        var childrenNode = (YamlSequenceNode)transformMapping.Children[new YamlScalarNode("m_Children")];
        List<long> childFileIds = new();
        foreach (YamlNode yamlNode in childrenNode)
        {
            var childNode = (YamlMappingNode)yamlNode;
            var fileIdNode = (YamlScalarNode)childNode.Children[new YamlScalarNode("fileID")];
            long childFileId = Convert.ToInt64(fileIdNode.Value);
            childFileIds.Add(childFileId);
        }

        return new TransformItem(fileId, gameObjectFileId, parentFileId, childFileIds.ToArray());
    }
}