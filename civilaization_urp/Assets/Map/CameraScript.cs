using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask tilesMask;
    [SerializeField] private MapTilegen2 tileGen;
    private RaycastHit info;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out info, Mathf.Infinity, tilesMask.value))
            {
                tileGen.SelectTile(int.Parse(info.collider.gameObject.name));
            }
            else
            {
                tileGen.DeselectLand();
            }
        }
    }
}
