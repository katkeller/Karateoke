using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoFloorController : MonoBehaviour
{
    [SerializeField]
    private GameObject floorSegmentPrefab;

    [SerializeField]
    private Material darkMaterial, lightMaterial;

    [SerializeField]
    private float secondsBetweenSwitch = 2.0f;

    private MeshRenderer[] firstFloorRenderers = new MeshRenderer[40];
    private MeshRenderer[] secondFloorRenderers = new MeshRenderer[40];
    private Material firstGroupMaterial, secondGroupMaterial;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = this.transform.position;
        this.transform.position = Vector3.zero;
        firstGroupMaterial = darkMaterial;
        secondGroupMaterial = lightMaterial;
        CreateFloor();
    }

    private void CreateFloor()
    {
        bool setAsGroupOne = true;
        int xPosition = 0;
        int zPosition = 0;
        int firstGroupCount = 0;
        int secondGroupCount = 0;

        for (int e = 0; e < 80; e++)
        {
            GameObject segment = (GameObject)Instantiate(floorSegmentPrefab);
            segment.transform.position = this.transform.position;
            segment.transform.parent = this.transform;
            segment.name = $"Segment {e}";
            segment.transform.position = new Vector3(xPosition, 0, zPosition);

            if (setAsGroupOne)
            {
                var renderer = segment.GetComponent<MeshRenderer>();
                renderer.material = firstGroupMaterial;
                firstFloorRenderers[firstGroupCount] = renderer;
                firstGroupCount++;
            }
            else
            {
                var renderer = segment.GetComponent<MeshRenderer>();
                renderer.material = secondGroupMaterial;
                secondFloorRenderers[secondGroupCount] = renderer;
                secondGroupCount++;
            }

            if (zPosition >= 9)
            {
                xPosition++;
                zPosition = 0;
            }
            else
            {
                zPosition++;
                setAsGroupOne = !setAsGroupOne;
            }
        }

        this.transform.position = originalPosition;
        firstGroupMaterial = lightMaterial;
        secondGroupMaterial = darkMaterial;
    }

    public void StartFloor()
    {
        StartCoroutine(WaitThenSwitchFloorPattern());
    }

    private IEnumerator WaitThenSwitchFloorPattern()
    {
        yield return new WaitForSeconds(secondsBetweenSwitch);

        foreach(MeshRenderer renderer in firstFloorRenderers)
        {
            renderer.material = firstGroupMaterial;
        }

        foreach(MeshRenderer renderer in secondFloorRenderers)
        {
            renderer.material = secondGroupMaterial;
        }

        Material currentFirstGroupMaterial = firstGroupMaterial;

        firstGroupMaterial = secondGroupMaterial;
        secondGroupMaterial = currentFirstGroupMaterial;

        StartCoroutine(WaitThenSwitchFloorPattern());
    }
}
