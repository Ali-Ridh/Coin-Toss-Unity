// Mendefinisikan aturan dan poin yang dibutuhkan untuk setiap rank
// dimuat pake JSON nanti
using UnityEngine;

[System.Serializable]
public class RankData
{
    public int rank;
    public int pointsNeeded;
    public string rankUpReward; // Opsional: ID untuk reward, misal "UnlockSpecialAbility"
}
