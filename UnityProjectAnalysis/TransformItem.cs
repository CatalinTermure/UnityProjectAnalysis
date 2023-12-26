namespace UnityProjectAnalysis;

public record TransformItem(long FileId, long GameObjectFileId, long ParentFileId, long[] ChildFileIds) :
    UnityYamlItem(ClassIds.Transform, FileId);