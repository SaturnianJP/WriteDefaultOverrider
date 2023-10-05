using UnityEngine;
using VRC.SDKBase;

namespace satania.ndmfscript.test
{
    public class WriteDefaultOverriderBehaviour : MonoBehaviour, IEditorOnly
    {
        [SerializeField] public bool isWriteDefault;
    }
}