namespace Maeve.Database;

public enum DocumentState {
    Uploading,
    Processing,
    Vectorizing,
    Vectorized,
    Failed
}