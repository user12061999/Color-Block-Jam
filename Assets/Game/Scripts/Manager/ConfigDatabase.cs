using UnityEngine;
using HAVIGAME;
using HAVIGAME.Audios;

[CreateAssetMenu(fileName = "ConfigDatabase", menuName = "Database/ConfigDatabase")]
public class ConfigDatabase : Database<ConfigDatabase> {
    [SerializeField] private Audio defaultButtonPressAudio;
    [SerializeField] private int maxLevel;
    [SerializeField] private ItemStack[] defaultInventory;
    [SerializeField] private string defaultValueFormat = "#,##0";
    [SerializeField] private int heartRegenCooldown = 300;
    [SerializeField] private int maxHeart = 5;
    public Audio DefaultButtonPressAudio => defaultButtonPressAudio;
    public int HeartRegenCooldown => heartRegenCooldown;
    public int MaxHeart => maxHeart;
    public int MaxLevel => maxLevel;

    public ItemStack[] DefaultInventory => defaultInventory;

    public string DefaultValueFormat => defaultValueFormat;
    
    
    
}
