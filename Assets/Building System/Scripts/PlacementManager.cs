using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : Singleton<PlacementManager>
{
    [Header("Base")]
    [SerializeField] private Placeable[] Placeables;
    [SerializeField] private Camera Cam;

    [Header("Layers")]
    [SerializeField] LayerMask PlacementLayer;
    [SerializeField] LayerMask detectionLayer;

    [Header("Build Time")]
    [SerializeField] private bool UseBuildTime = false;

    [Header("Materials")]
    [SerializeField] private Material PlaceableMat;
    [SerializeField] private Material NotPlaceableMat;

    [Header("Rotation")]
    [SerializeField] private KeyCode rotateKey = KeyCode.R;
    [SerializeField] private float rotateSpeed = 5f;

    Placeable selectedPlaceable;
    Placeable SelectedPlaceable
    {
        get { return selectedPlaceable; }
        set
        {
            selectedPlaceable = value;
        }
    }

    public LayerMask DetectionLayer { get => detectionLayer; }
    public KeyCode RotateKey { get => rotateKey; }
    public float RotateSpeed { get => rotateSpeed; set => rotateSpeed = value; }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (SelectedPlaceable == null)
            return;

        UpdateObjectPosition();
    }

    private void Update()
    {
        PickPlaceable();

        if (SelectedPlaceable == null)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0) && SelectedPlaceable.CanBePlaced)
            PlaceObject();

    }

    void PickPlaceable()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetPlaceable(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetPlaceable(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetPlaceable(2);
        }
    }

    void SetPlaceable(int index)
    {
        if (SelectedPlaceable != null)
            Destroy(SelectedPlaceable.gameObject);

        SelectedPlaceable = Instantiate(Placeables[index].gameObject, Vector3.zero, Quaternion.identity, null).GetComponent<Placeable>();
    }

    void PlaceObject()
    {
        SelectedPlaceable.InitializeObjectPlacement(UseBuildTime);
        SelectedPlaceable = null;
    }

    void UpdateObjectPosition()
    {
        Bounds bounds = SelectedPlaceable.GetBounds();

        Ray cameraRay = Cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity, PlacementLayer))
        {
            float yOffset = SelectedPlaceable.transform.position.y - bounds.min.y;
            SelectedPlaceable.transform.position = Vector3.Slerp(SelectedPlaceable.transform.position, new Vector3(hit.point.x, hit.point.y + yOffset, hit.point.z), 1);
        }

        if (SelectedPlaceable.CanBePlaced)
        {
            SelectedPlaceable.SetMaterial(PlaceableMat);
        }
        else
        {
            SelectedPlaceable.SetMaterial(NotPlaceableMat);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (SelectedPlaceable != null)
        {
            Bounds bounds = SelectedPlaceable.GetBounds();
            var center = bounds.center;
            var size = bounds.size;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, size.z));
        }
    }
}
