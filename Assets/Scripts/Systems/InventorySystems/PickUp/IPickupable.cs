interface IPickupable {
    public ItemData GetItemData();
    public int GetItemAmount();
    public void SetItem(ItemData itemData);
    public void SetAmount(int amount);
}
