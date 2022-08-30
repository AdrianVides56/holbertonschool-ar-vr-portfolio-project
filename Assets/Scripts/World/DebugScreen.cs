using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    World world;
    TextMeshProUGUI text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TextMeshProUGUI>();

        halfWorldSizeInVoxels = VoxelData.worldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.worldSizeInChunks / 2;
    }

    void Update()
    {
        string debugText = "VR-Craft _ Made by DevMi" + "\n";
        debugText += frameRate + " FPS" + "\n";
        debugText += "XYZ:" + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + "," + Mathf.FloorToInt(world.player.transform.position.y) + "," + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels) + "\n";
        debugText += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + "," + (world.playerChunkCoord.z - halfWorldSizeInChunks) + "\n";

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;
    }
}
