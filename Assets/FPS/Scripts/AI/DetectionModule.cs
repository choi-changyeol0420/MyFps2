using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 적군 디텍팅 구현
    /// </summary>
    public class DetectionModule : MonoBehaviour
    {
        #region Variables
        private ActorManager actorManager;

        public UnityAction OnDetectedTarget;        //적을 감지하면 등록된 함수 호출
        public UnityAction OnLostTarget;            //적을 놓치면 등록된 함수 호출

        public GameObject KnownDetectedTarget {  get; private set; }
        public bool HadKnowTarget {  get; private set; }
        public bool IsSeeingTraget {  get; private set; }
        public Transform detectionSourcePonit;
        public float detectionRange = 20f;                              //적 감지 거리

        public float knownTargetTimeout = 4f;
        private float TimeLastSeenTraget = Mathf.NegativeInfinity;

        //attack
        public float attackRange = 10f;                                 //적 공격 거리
        public bool IsTargetInAttackRange {  get; private set; }
        #endregion
        private void Start()
        {
            //참조
            actorManager = GameObject.FindObjectOfType<ActorManager>();
        }
        //디텍팅
        public void HandleTargetDetection(Actor actor, Collider[] selfCollider)
        {
            if(KnownDetectedTarget && !IsSeeingTraget && (Time.time - TimeLastSeenTraget) > knownTargetTimeout)
            {
                KnownDetectedTarget = null;
            }

            float sqrDetectionRange = detectionRange * detectionRange;
            IsSeeingTraget = false;
            float closetSqristance = Mathf.Infinity;

            foreach (var otheractor in actorManager.Actors)
            {
                //아군이면
                if (otheractor.affiliation == actor.affiliation)
                    continue;
                float sqrDistance = (otheractor.aimPoint.position - detectionSourcePonit.position).sqrMagnitude;
                if(sqrDistance < sqrDetectionRange && sqrDistance< closetSqristance)
                {
                    RaycastHit[] hits = Physics.RaycastAll(detectionSourcePonit.position,
                        (otheractor.aimPoint.position - detectionSourcePonit.position).normalized,
                        detectionRange,-1, QueryTriggerInteraction.Ignore);
                    
                    RaycastHit cloesHit = new RaycastHit();
                    cloesHit.distance = Mathf.Infinity;
                    bool foundVAliedHit = false;
                    foreach(var hit in hits)
                    {
                        if(hit.distance < cloesHit.distance && selfCollider.Contains(hit.collider) == false)
                        {
                            cloesHit = hit;
                            foundVAliedHit = true;
                        }
                    }

                    //적을 찾았으면
                    if (foundVAliedHit)
                    {
                        Actor hitActor = cloesHit.collider.GetComponentInParent<Actor>();
                        if (hitActor == otheractor)
                        {
                            IsSeeingTraget = true;
                            closetSqristance = sqrDistance;
                            TimeLastSeenTraget = Time.time;
                            KnownDetectedTarget = otheractor.aimPoint.gameObject;
                        }
                    }
                }
            }
            //attack Range Check
            IsTargetInAttackRange = (KnownDetectedTarget != null) &&
                Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= attackRange;

            //적을 모르고 있다가 적을 발견한 순간에 실행
            if(!HadKnowTarget && KnownDetectedTarget != null)
            {
                OnDetected();
            }

            //적을 계속 주시하고 있다가 놓치는 순간 실행
            if(HadKnowTarget && KnownDetectedTarget == null)
            {
                OnLost();
            }

            HadKnowTarget = KnownDetectedTarget != null;
        }
        //적을 감지하면
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();
        }
        //적을 놓치면
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }
    }
}