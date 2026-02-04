using UnityEngine;

namespace Ingame
{
    public interface IClickable
    {
        void OnClick(Vector3 hitPoint, Vector3 hitNormal);
    }
}
