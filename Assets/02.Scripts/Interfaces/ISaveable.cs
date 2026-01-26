/// <summary>
/// M2 저장 시스템을 위한 인터페이스
/// M1에서는 정의만 하고, M2에서 구현
/// </summary>
public interface ISaveable
{
    string SaveKey { get; }
    object CaptureState();
    void RestoreState(object state);
}
