using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    World world;
    PlayerVR player;
    
    RaycastHit hit;

    public float distance = 0.1f;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("XR Origin").GetComponent<PlayerVR>();

        cursorItemSlot = new ItemSlot(cursorSlot, null);
    }

    private void Update()
    {
        if (!world.inUI)
            return;

        cursorSlot.transform.position = player.lHand.position + player.lHand.forward * distance;

        if (player.inputActions.Player.Click.triggered)
            HandleSlotClick(CheckForSlot());
    }

    private void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null) // clicked on empty slot
            return;
        
        if (!cursorSlot.HasItem && !clickedSlot.HasItem) // clicked on empty slot and empty cursor
            return;

        if (clickedSlot.itemSlot.isCreative)
        {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }

        if (!cursorSlot.HasItem && clickedSlot.HasItem) // clicked on slot with item and empty cursor
        {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }
        if (cursorSlot.HasItem && !clickedSlot.HasItem) // clicked on empty slot and item in cursor
        {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }
        if (cursorSlot.HasItem && clickedSlot.HasItem) // clicked on slot with item and cursor with item
        {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id) // items are different
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldClickedSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                cursorSlot.itemSlot.InsertStack(oldClickedSlot);
            }
            else // items are the same
            {
                byte clickedSlotItemID = clickedSlot.itemSlot.stack.id;
                int stackLimit = world.blocktypes[clickedSlotItemID].stackLimit;

                ItemStack oldCursorSlotStack = cursorSlot.itemSlot.TakeAll();
                ItemStack oldClickedSlotStack = clickedSlot.itemSlot.TakeAll();
                int sumAmount = oldCursorSlotStack.amount + oldClickedSlotStack.amount;

                if (sumAmount > stackLimit)
                {
                    ItemStack  sumClickedSlotStackFull = new ItemStack(clickedSlotItemID, stackLimit);
                    ItemStack sumClickedSlotStackLeft = new ItemStack(clickedSlotItemID, sumAmount - stackLimit);
                    clickedSlot.itemSlot.InsertStack(sumClickedSlotStackFull);
                    cursorSlot.itemSlot.InsertStack(sumClickedSlotStackLeft);
                }
                else
                {
                    ItemStack sumClickedSlotStack = new ItemStack(clickedSlotItemID, sumAmount);
                    clickedSlot.itemSlot.InsertStack(sumClickedSlotStack);
                }
            }
            return;
        }
    }

    private UIItemSlot CheckForSlot()
    {

        if (Physics.Raycast(player.lHand.position, player.lHand.forward, out hit, 2f, LayerMask.GetMask("UI")))
        {
            if (hit.collider.gameObject.tag == "UIItemSlot")
                return hit.collider.gameObject.GetComponent<UIItemSlot>();
        }
        return null;
    }
}
