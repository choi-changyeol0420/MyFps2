using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class WaveSpawn : MonoBehaviour
    {
        #region Variables
        public GameObject[] HoverBotPrefab;
        private float lastDieTime = Mathf.NegativeInfinity;
        private float lastSpawnTime = 0.2f;
        public int hoverlist;

        public Health health;
        public PatrolPath patrolPath;
        #endregion
        private void Awake()
        {
            hoverlist = HoverBotPrefab.Length;
            health.OnDie += OnDie;

            for (int i = 0; i < hoverlist; i++)
            {
                if(lastSpawnTime < 0)
                {
                    GameObject gameObject = Instantiate(HoverBotPrefab[i], transform.position, Quaternion.identity);
                    gameObject.transform.parent = transform;
                    patrolPath.enemiesToAssign.Add(gameObject.GetComponent<EnemyController>());
                }
                lastSpawnTime -= Time.deltaTime;
            }
        }
        void OnDie()
        {
            
        }
    }
}