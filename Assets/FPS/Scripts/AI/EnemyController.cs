using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;

        //death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpwanPosition;
        #endregion

        private void Start()
        {
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;
        }
        void OnDamaged(float damage, GameObject damageSource)
        {
            
        }
        void OnDie()
        {
            GameObject VfxGO = Instantiate(deathVfxPrefab, deathVfxSpwanPosition.position,Quaternion.identity);
            Destroy(VfxGO,5f);

            //enemy kill
            Destroy(gameObject);
        }
    }
}