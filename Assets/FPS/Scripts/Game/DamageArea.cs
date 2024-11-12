using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 일정범위 안에 있는 콜라이더 오브젝트 데미지 주기
    /// </summary>
    public class DamageArea : MonoBehaviour
    {
        #region Variables
        [SerializeField]private float areaofEffectDistance = 10f;
        [SerializeField]private AnimationCurve damageRatioOverDistance;
        #endregion

        public void InflictDamageArea(float damage, Vector3 center, LayerMask layers, QueryTriggerInteraction interaction, GameObject owner)
        {
            Dictionary<Health,Damageable> uniqueDamageHealth = new Dictionary<Health, Damageable> ();

            Collider[] affectedColliders = Physics.OverlapSphere(center, areaofEffectDistance, layers, interaction);
            foreach (Collider collider in affectedColliders)
            {
                Damageable damageable = collider.GetComponent<Damageable>();
                if(damageable)
                {
                    Health health = damageable.GetComponentInParent<Health>();
                    if (health != null && uniqueDamageHealth.ContainsKey(health) == false)
                    {
                        uniqueDamageHealth.Add(health, damageable);
                    }
                }
            }
            //데미지 주기
            foreach (var uniqueDamageable in uniqueDamageHealth.Values)
            {
                float distance = Vector3.Distance(uniqueDamageable.transform.position, center);
                float curveDamage = damageRatioOverDistance.Evaluate(distance);
                Debug.Log(curveDamage);
                uniqueDamageable.InflictDamage(damage, true, owner);
            }
        }
    }
}