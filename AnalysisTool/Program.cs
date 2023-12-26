using System.Text;
using UnityProjectAnalysis;

internal static class Program
{
    private static readonly Dictionary<long, bool> _visited = new();
    private static readonly Dictionary<long, TransformItem> _transforms = new();
    private static readonly Dictionary<long, GameObjectItem> _gameObjects = new();

    private static void GetObjectHierarchy(StringBuilder output, TransformItem transform, int level = 0)
    {
        for (int i = 0; i < level; i++)
        {
            output.Append("--");
        }

        output.AppendLine(_gameObjects[transform.GameObjectFileId].Name);
        _visited[transform.FileId] = true;

        foreach (long childFileId in transform.ChildFileIds)
        {
            if (!_visited.ContainsKey(childFileId))
            {
                GetObjectHierarchy(output, _transforms[childFileId], level + 1);
            }
        }
    }

    private static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("USAGE: AnalysisTool.exe <path-to-unity-project> <path-to-output-file>");
        }

        string projectPath = args[0];
        string outputPath = args[1];
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var projectAnalyzer = new ProjectAnalyzer(projectPath);
        DumpSceneHierarchies(projectAnalyzer, outputPath);

        string unusedScriptsOutputPath = Path.Combine(outputPath, "UnusedScripts.csv");
        StringBuilder unusedScriptsOutput = new();
        unusedScriptsOutput.AppendLine("Relative Path,GUID");
        foreach (ScriptItem script in projectAnalyzer.GetUnusedScripts())
        {
            unusedScriptsOutput.AppendLine($"{script.Path},{script.Guid.ToString("N")}");
        }

        File.WriteAllText(unusedScriptsOutputPath, unusedScriptsOutput.ToString());
    }

    private static void DumpSceneHierarchies(ProjectAnalyzer projectAnalyzer, string outputPath)
    {
        List<string> sceneNames = projectAnalyzer.GetSceneNames();
        foreach (string sceneName in sceneNames)
        {
            List<GameObjectItem> gameObjects = projectAnalyzer.GetGameObjectsFromScene(sceneName);
            PopulateDictionaries(gameObjects);

            var outputStringBuilder = new StringBuilder();
            foreach (TransformItem transform in _transforms.Values)
            {
                if (transform.ParentFileId == 0)
                {
                    _visited.Clear();
                    GetObjectHierarchy(outputStringBuilder, transform);
                }
            }

            string outputFilePath = Path.Combine(outputPath, $"{sceneName}.unity.dump");
            File.WriteAllText(outputFilePath, outputStringBuilder.ToString());
        }
    }

    private static void PopulateDictionaries(List<GameObjectItem> gameObjects)
    {
        _transforms.Clear();
        _gameObjects.Clear();

        foreach (GameObjectItem gameObject in gameObjects)
        {
            _gameObjects[gameObject.FileId] = gameObject;
            foreach (UnityYamlItem component in gameObject.Components)
            {
                if (component.ClassId == ClassIds.Transform)
                {
                    _transforms[component.FileId] = (TransformItem)component;
                }
            }
        }
    }
}