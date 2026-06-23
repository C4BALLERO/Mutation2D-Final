namespace MutationSwarm
{
    [System.Serializable]
    public class WaveStats
    {
        public int   bulletsShot;
        public int   bulletsHit;
        public int   dashUsed;
        public float timeHigh;
        public float timeTotal;
        public int   defensesBuilt;
        public float poisonDmgTaken;
        public int   contactHitsFromSpeed;
        public int   defKilled;

        public float Accuracy => bulletsShot > 0 ? (float)bulletsHit / bulletsShot : 0f;
        public float HighRatio => timeTotal > 0 ? timeHigh / timeTotal : 0f;

        public void Reset()
        {
            bulletsShot = bulletsHit = dashUsed = defensesBuilt = contactHitsFromSpeed = defKilled = 0;
            timeHigh = timeTotal = poisonDmgTaken = 0f;
        }
    }
}
