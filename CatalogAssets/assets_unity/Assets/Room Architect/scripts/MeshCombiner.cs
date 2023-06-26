using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    List<Material> materials;
    List<Mesh> submeshes;
    MeshFilter[] filters;
    List<GameObject> childResults;
    bool limitReached = false;
    List<GameObject> toDestroy;
    int materialCounter = 0;
    int filterCounter = 0;
    List<Material> materialSubList;


    public void combine(bool destroyRemnants, GameObject colliderHolder = null)
    {
        Quaternion oldRot = this.transform.rotation;
        Vector3 oldPos = this.transform.position;
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
        limitReached = true;
        materialCounter = 0;
        filterCounter = 0;
        for (int i = 0; i < 50 && limitReached; i++)
        {
            limitReached = false;
            seekMaterials();
            seekSubMeshes();
            // The final mesh: combine all the material-specific meshes as independent submeshes.
            GameObject newChild = addChildResult();
            List<CombineInstance> finalCombiners = new List<CombineInstance>();
            for (int j = 0; j < submeshes.Count; j++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = submeshes[j];
                ci.subMeshIndex = 0;
                ci.transform = Matrix4x4.identity;
                finalCombiners.Add(ci);
            }
            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
            newChild.gameObject.AddComponent<MeshFilter>();
            newChild.gameObject.AddComponent<MeshRenderer>();
            newChild.GetComponent<MeshFilter>().sharedMesh = finalMesh;
            while (materialSubList.Count > finalMesh.subMeshCount)
                materialSubList.RemoveAt(materialSubList.Count - 1);
            newChild.GetComponent<MeshRenderer>().sharedMaterials = materialSubList.ToArray();
        }

        reportBoxColliders(colliderHolder);

        Transform[] childList = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childList.Length; i++)
        {
            try
            {
                if (childList[i].parent == this.transform && childList[i].gameObject.name != "_combinedResult")
                {
                    GameObject.DestroyImmediate(childList[i].gameObject);
                }
            }
            catch { }
        }

        foreach (GameObject newChild in childResults)
        {
            newChild.transform.SetParent(this.transform, false);
        }

        if (childResults.Count == 1)
        {
            childResults[0].gameObject.name = this.gameObject.name;
            childResults[0].transform.SetParent(this.transform, false);
        }
        transform.rotation = oldRot;
        transform.position = oldPos;
    }

    public void reportBoxColliders(GameObject colliderHolder)
    {
        BoxCollider[] colliders = transform.GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < colliders.Length && colliderHolder != null; i++)
        {
            if (colliders[i].gameObject.GetComponent<RetrievedCollider>() != null)
                continue;
            colliders[i].gameObject.AddComponent<RetrievedCollider>();
            colliders[i].transform.SetParent(colliderHolder.transform, true);
            Component[] cmp = colliders[i].gameObject.GetComponents(typeof(Component));
            for (int j = 0; j < cmp.Length; j++)
            {
                if ((cmp[j] is BoxCollider) == false && (cmp[j] is Transform) == false)
                {
                    GameObject.DestroyImmediate(cmp[j]);
                }
            }
        }
    }

    public GameObject addChildResult()
    {
        if (childResults == null)
            childResults = new List<GameObject>();
        GameObject res = new GameObject("_combinedResult");
        childResults.Add(res);
        return res;
    }

    public void seekMaterials()
    {
        materials = new List<Material>();
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.transform == transform)
                continue;
            Material[] localMats = renderer.sharedMaterials;
            foreach (Material localMat in localMats)
                if (!materials.Contains(localMat))
                {
                    materials.Add(localMat);
                }
        }
    }

    public void seekSubMeshes()
    {
        toDestroy = new List<GameObject>();
        submeshes = new List<Mesh>();
        int vertCounter = 0;
        filters = GetComponentsInChildren<MeshFilter>(false);
        materialSubList = new List<Material>();
        for (int i = 0; materialCounter < materials.Count; materialCounter++)
        {
            Material material = materials[materialCounter];
            materialSubList.Add(material);
            List<CombineInstance> combiners = new List<CombineInstance>();
            for (int j = 0; filterCounter < filters.Length; filterCounter++)
            {
                MeshFilter filter = filters[filterCounter];
                if (filter.transform == transform) continue;
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    Debug.LogError(filter.name + " has no MeshRenderer");
                    continue;
                }
                Material[] localMaterials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < localMaterials.Length && !limitReached; materialIndex++)
                {
                    if (localMaterials[materialIndex] != material)
                        continue;
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = filter.sharedMesh;
                    ci.subMeshIndex = materialIndex;
                    ci.transform = filter.gameObject.transform.localToWorldMatrix;
                    if (ci.mesh != null)
                        vertCounter += ci.mesh.vertexCount;
                    if (vertCounter > 65500)
                    {
                        limitReached = true;
                        break;
                    }
                    combiners.Add(ci);
                    if (toDestroy.Contains(filter.gameObject) == false && !limitReached)
                    {
                        toDestroy.Add(filter.gameObject);
                    }
                }
                if (limitReached)
                    break;
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combiners.ToArray(), true);
            submeshes.Add(mesh);
            if (vertCounter > 65500)
                return;
            filterCounter = 0;
        }
    }
}

public class RetrievedCollider : MonoBehaviour
{

}
