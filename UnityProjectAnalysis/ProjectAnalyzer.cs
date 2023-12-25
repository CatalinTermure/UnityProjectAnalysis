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
}