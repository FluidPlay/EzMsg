using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DisplayHelpText : MonoBehaviour
{
    [SerializeField] private string HelpText =
        "1. Hit Play in Unity\n2. Move `Projectile` until it hits `Target`\n3. Watch Debug.Log messages in Console";
    private void OnGUI()
    {
        GUI.Label(new Rect(20, 10, 300, 100), HelpText);
    }
}
