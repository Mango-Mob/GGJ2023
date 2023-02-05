using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFactory : MonoBehaviour
{
    public List<Prop> propList = new List<Prop>();
    public List<GameObject> otherProps = new List<GameObject>();

    public void SpawnTree(GameObject prefab, Vector3 position)
    {
        GameObject propObj = GameObject.Instantiate(prefab, this.transform);
        propObj.transform.position = position;

        RaycastHit result;
        if (Physics.Raycast(propObj.transform.position + new Vector3(0, 1, 0), Vector3.down, out result))
        {
            propObj.transform.up = result.normal;
        }

        Prop prop = propObj.AddComponent<Prop>();
        prop.owner = this;
        propList.Add(prop);
    }

    public void SpawnBigRock(GameObject prefab, Vector3 position)
    {
        GameObject propObj = GameObject.Instantiate(prefab, this.transform);
        propObj.transform.position = position;
        propObj.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360.0f), 0);

        Prop prop = propObj.AddComponent<Prop>();
        prop.owner = this;
        propList.Add(prop);
    }

    public void Collect(Prop prop)
    {
        Remove(prop);
        GameObject.Instantiate(Resources.Load<GameObject>("MoneyProjectile"), prop.GetComponent<Collider>().bounds.center, Quaternion.identity);
        prop.GetComponent<Tree>().DetachVFX(0.8f);
        Destroy(prop.gameObject, 1.0f);
        Destroy(prop);
        GameManager.Instance.AddCash(200);
        var prefabs = GetComponent<TerrainGenerator>().treePrefab;
        SpawnTree(prefabs[Random.Range(0, prefabs.Length)], GetComponent<TerrainGenerator>().PlaceTree());
    }

    public void SpawnGrass(GameObject prefab, Vector3 position)
    {
        GameObject propObj = GameObject.Instantiate(prefab, this.transform);
        propObj.transform.position = position;
        propObj.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360.0f), 0);

        otherProps.Add(propObj);
    }
    public void SpawnPlatform(GameObject prefab, Vector3 localPosition)
    {
        GameObject propObj = GameObject.Instantiate(prefab, this.transform);
        propObj.transform.localPosition = localPosition;

        PlayerSpawnLocation spawn = propObj.GetComponentInChildren<PlayerSpawnLocation>();
        Car.Spawn(spawn.transform.position, spawn.transform.rotation);

        Prop prop = propObj.AddComponent<Prop>();
        prop.owner = this;
        propList.Add(prop);
    }

    public void Clear()
    {
        foreach (Prop item in propList)
        {
            item.owner = null;
            Destroy(item.gameObject);
        }
        propList.Clear();
        foreach (GameObject obj in otherProps)
            Destroy(obj);
        otherProps.Clear();
    }

    public void Remove(Prop prop)
    {
        propList.Remove(prop);
    }
}
