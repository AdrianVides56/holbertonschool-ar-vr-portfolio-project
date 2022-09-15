using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar : MonoBehaviour
{
    public PlayerVR player;

    public UIItemSlot[] slots;
    public RectTransform highlight;
    public int slotIndex = 0;
    bool canChangeSlot = true;

    private void Start()
    {
        byte index = 1;
        ItemStack stack;
        ItemSlot slot;
        foreach (UIItemSlot s in slots)
        {
            stack = new ItemStack(index, Random.Range(2, 65));
            slot = new ItemSlot(s, stack);
            index++;
        }
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
            
            if (slotIndex > slots.Length - 1)
                slotIndex = 0;
            else if (slotIndex < 0)
                slotIndex = slots.Length - 1;

        }

        highlight.position = slots[slotIndex].slotIcon.transform.position;

        yield return new WaitForSeconds(.2f);
        canChangeSlot = true;

    } 
}
