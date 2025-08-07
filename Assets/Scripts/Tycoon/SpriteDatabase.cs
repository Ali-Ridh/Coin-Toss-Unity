using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Ini adalah kelas kecil untuk menautkan sebuah string ID dengan sebuah aset Sprite.
[System.Serializable]
public class ItemSpriteLink
{
    public string itemID;
    public Sprite iconSprite;
}

// Gunakan atribut ini agar Anda bisa membuat aset database ini dari menu di Unity Editor.
[CreateAssetMenu(fileName = "New Sprite Database", menuName = "Diner/Sprite Database")]
public class SpriteDatabase : ScriptableObject
{
    // Ini adalah daftar di mana Anda akan men-drag semua sprite makanan Anda di Inspector.
    public List<ItemSpriteLink> itemSprites;

    // Fungsi bantuan untuk dengan mudah menemukan sprite berdasarkan ID-nya.
    public Sprite GetSpriteByID(string id)
    {
        ItemSpriteLink link = itemSprites.FirstOrDefault(item => item.itemID == id);
        if (link != null)
        {
            return link.iconSprite;
        }
        else
        {
            Debug.LogWarning("Sprite not found in database for ID: " + id);
            return null;
        }
    }
}
