using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    World world;
    public PlayerVR player;

    public RectTransform highlight;
    public ItemSlot[] itemSlots;

    int slotIndex = 0;
    bool canChangeSlot = true;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        
        foreach(ItemSlot slot in itemSlots)
        {
            slot.icon.sprite = world.blocktypes[slot.itemID].icon;
            slot.icon.enabled = true;
        }

        player.selectedBlockIndex = itemSlots[slotIndex].itemID;
    }

    void Update()
    {
        if (canChangeSlot)
            StartCoroutine(ChangeSlot());
    }

    IEnumerator ChangeSlot()
    {
        if (canChangeSlot)
        {
            canChangeSlot = false;

            if (player._scrollToolBar > 0.7f)
                slotIndex++;
            else if (player._scrollToolBar < -0.7f)
                slotIndex--;
            
            if (slotIndex > itemSlots.Length - 1)
                slotIndex = 0;
            else if (slotIndex < 0)
                slotIndex = itemSlots.Length - 1;

        }

        highlight.position = itemSlots[slotIndex].icon.transform.position;
        player.selectedBlockIndex = itemSlots[slotIndex].itemID;

        yield return new WaitForSeconds(.2f);
        canChangeSlot = true;

    } 
}

[System.Serializable]
public class ItemSlot
{
    public byte itemID;
    public Image icon;
}
