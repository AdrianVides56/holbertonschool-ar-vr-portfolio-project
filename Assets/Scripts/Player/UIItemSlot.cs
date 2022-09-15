using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public TextMeshProUGUI slotAmount;

    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool HasItem
    {
        get
        {
            if (itemSlot == null)
                return false;
            else
                return itemSlot.HasItem;
        }
    }

    public void Link(ItemSlot _itemSlot)
    {
        itemSlot = _itemSlot;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void Unlink()
    {
        itemSlot.UnlinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blocktypes[itemSlot.stack.id].icon;
            slotAmount.text = itemSlot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
            Clear();
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if (isLinked)
            itemSlot.UnlinkUISlot();//Unlink();
    }
}

public class ItemSlot
{
    public ItemStack stack = null;
    private UIItemSlot uIItemSlot = null;

    public bool isCreative;

    public ItemSlot(UIItemSlot _uIItemSlot)
    {
        stack = null;
        uIItemSlot = _uIItemSlot;
        uIItemSlot.Link(this);

    }

    public ItemSlot(UIItemSlot _uIItemSlot, ItemStack _stack)
    {
        stack = _stack;
        uIItemSlot = _uIItemSlot;
        uIItemSlot.Link(this);

    }

    public void LinkUISlot(UIItemSlot uiSlot)
    {
        uIItemSlot = uiSlot;
    }

    public void UnlinkUISlot()
    {
        uIItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if (uIItemSlot != null)
            uIItemSlot.UpdateSlot();
    }

    public int Take(int amt)
    {
        if (amt > stack.amount)
        {
            int _amount = stack.amount;
            EmptySlot();
            return _amount;
        }
        else if (amt < stack.amount)
        {
            stack.amount -= amt;
            uIItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStack(ItemStack _stack)
    {
        stack = _stack;
        uIItemSlot.UpdateSlot();
    }

    public bool HasItem
    {
        get 
        {
            if (stack != null)
                return true;
            else
                return false;
        }
    }
}