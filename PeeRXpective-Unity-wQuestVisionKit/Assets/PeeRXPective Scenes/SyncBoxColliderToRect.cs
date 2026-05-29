using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider))]
public class SyncBoxColliderToRect : MonoBehaviour
{
    public float depth = 0.01f; // collider thickness

    void OnEnable()                           => Sync();
    void OnValidate()                         => Sync();
    void OnRectTransformDimensionsChange()    => Sync();

    void Sync()
    {
        var rt = (RectTransform)transform;
        var bc = GetComponent<BoxCollider>();
        if (rt == null || bc == null) return;

        var size  = rt.rect.size;      // in local units
        var pivot = rt.pivot;          // 0..1

        // Match size (X/Y from rect, thin Z)
        bc.size = new Vector3(size.x, size.y, depth);

        // Center so the BoxCollider aligns with the RectTransform pivot
        bc.center = new Vector3(
            (0.5f - pivot.x) * size.x,
            (0.5f - pivot.y) * size.y,
            0f
        );
    }
}
