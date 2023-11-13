using UnityEngine;

[CreateAssetMenu(menuName = "Force Graph/Force Graph Settings")]
public class ForceGraphSettings : ScriptableObject
{
    public float PushForce = .01f;
    public float PullForce = .01f;
    public int Iterations = 100;
    public AnimationCurve BlendingOverDistance;
}
