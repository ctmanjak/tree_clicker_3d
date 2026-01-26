using UnityEngine;

/// <summary>
/// 클릭 가능한 오브젝트 인터페이스
/// </summary>
public interface IClickable
{
    void OnClick(Vector3 hitPoint);
}
