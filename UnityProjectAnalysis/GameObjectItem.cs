namespace UnityProjectAnalysis;

public record GameObjectItem(long FileId) : UnityYamlItem(ClassIds.GameObject, FileId);