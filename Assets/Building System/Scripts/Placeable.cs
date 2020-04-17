using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeable : MonoBehaviour
{
    [SerializeField] public int AfterBuildLayer = 4;

    [Header("Build Time")]
    [SerializeField] public float BuildTime = 2f;
    [SerializeField] public GameObject ConstructionEffect;

    [HideInInspector] public bool CanBePlaced = false;

    bool BeingBuilt = false;

    LayerMask PlacementLayer;
    List<GameObject> CollisionList = new List<GameObject>();

    List<CachedRenderer> Renderers = new List<CachedRenderer>();

    private void Awake()
    {
        if (!GetComponent<Rigidbody>())
        {
            gameObject.AddComponent<Rigidbody>();
        }
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;

        if (GetComponent<MeshRenderer>())
        {
            Renderers.Add(new CachedRenderer(GetComponent<MeshRenderer>(), GetComponent<MeshRenderer>().materials));
        }

        foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
        {
            Renderers.Add(new CachedRenderer(rend, rend.materials));
        }

        PlacementLayer = PlacementManager.Instance.DetectionLayer;
    }

    public Bounds GetBounds()
    {
        Bounds bounds = new Bounds(this.transform.position, Vector3.zero);

        foreach (Collider item in GetComponentsInChildren<Collider>())
        {
            bounds.Encapsulate(item.bounds);
        }

        return bounds;
    }

    public void InitializeObjectPlacement(bool useBuildTime)
    {
        BeingBuilt = true;
        SetLayer();

        if (useBuildTime)
        {
            StartCoroutine(BuildObject());
        }
        else
        {
            PlaceObject();
        }

    }

    IEnumerator BuildObject()
    {
        GameObject Effect = null;

        if (ConstructionEffect != null)
        {
            Effect = Instantiate(ConstructionEffect, GetBounds().center, ConstructionEffect.transform.rotation, this.transform);
        }

        float passedTime = 0f;

        while (passedTime <= BuildTime)
        {
            passedTime += Time.deltaTime;
            yield return null;
        }

        if (Effect != null)
            Destroy(Effect);

        PlaceObject();
    }

    void PlaceObject()
    {
        foreach (CachedRenderer rend in Renderers)
        {
            rend.Renderer.materials = rend.Materials;
        }

        Destroy(GetComponent<Rigidbody>());
        Destroy(this);
    }

    public void SetMaterial(Material material)
    {

        foreach (CachedRenderer rend in Renderers)
        {
            Material[] mats = new Material[rend.Materials.Length];

            for (int i = 0; i < rend.Materials.Length; i++)
            {
                mats[i] = material;
            }

            rend.Renderer.materials = mats;
        }
    }

    public void SetLayer()
    {
        foreach (CachedRenderer go in Renderers)
        {
            go.Renderer.gameObject.layer = AfterBuildLayer;
        }
    }

    private void FixedUpdate()
    {
        if (BeingBuilt)
            return;

        if (Input.GetKey(PlacementManager.Instance.RotateKey))
        {
            transform.Rotate(Vector3.up * Time.deltaTime * PlacementManager.Instance.RotateSpeed);
        }

        if (CollisionList.Count > 0)
        {
            CanBePlaced = false;
        }
        else
        {
            CanBePlaced = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (PlacementLayer == (PlacementLayer | (1 << other.gameObject.layer)))
        {
            if (!CollisionList.Contains(other.gameObject))
                CollisionList.Add(other.gameObject);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (PlacementLayer == (PlacementLayer | (1 << other.gameObject.layer)))
        {
            CollisionList.Remove(other.gameObject);
        }
    }
}

[System.Serializable]
public class CachedRenderer
{
    public MeshRenderer Renderer;
    public Material[] Materials;

    public CachedRenderer(MeshRenderer Rend, Material[] Mats)
    {
        Renderer = Rend;
        Materials = Mats;
    }
}
