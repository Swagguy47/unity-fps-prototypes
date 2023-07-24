using UnityEngine;

public class ContainerObject : MonoBehaviour
{
    public Weapon[] ItemInventory;
    [SerializeField] bool RandomItems;

    private void Start() {
        //replaces container inventory items with unique variants & generates random inventory if valid
        for (int i = 0; i < ItemInventory.Length; i++)
        {
            Debug.Log(gameObject.name + " " + i + " " + ItemInventory[i].ParentClass.name);
            if (RandomItems) { //Random weapon
                Weapon UniqueItem = StaticItemPool.Items.UniqueUnregistered(StaticItemPool.Items.ItemPool[Random.Range(0, StaticItemPool.Items.ItemPool.Length)].BaseClass.ParentClass);
                ItemInventory[i] = UniqueItem;
            }
            else { //uses input
                Weapon UniqueItem = StaticItemPool.Items.UniqueUnregistered(ItemInventory[i].ParentClass);
                ItemInventory[i] = UniqueItem;
            }
        }
    }

    public void OpenContainer()
    {
        PlayerCallback.Container.OpenContainer(this);
        PlayerCallback.Inventory.Offset = -400;
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = true;
        PlayerCallback.PlayerBrain.IsInInventory = true;
        PlayerCallback.PlayerBrain.OpenInventory();
    }
}
