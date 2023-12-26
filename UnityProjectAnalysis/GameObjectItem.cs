namespace UnityProjectAnalysis;

public record GameObjectItem(long FileId, UnityYamlItem[] Components) : UnityYamlItem(ClassIds.GameObject, FileId);