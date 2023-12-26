namespace UnityProjectAnalysis;

public record GameObjectItem(long FileId, string Name, UnityYamlItem[] Components) : UnityYamlItem(ClassIds.GameObject, FileId);