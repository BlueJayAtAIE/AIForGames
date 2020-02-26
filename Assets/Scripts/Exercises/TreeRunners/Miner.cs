using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : MonoBehaviour
{
    public Transform home;
    public Transform gold;

    public int goldMined;
    public int goldCapacity = 10;

    public float speed = 4;

    IDecision decisionTreeRoot;

    void Start()
    {
        decisionTreeRoot = new IsGoldFull(gameObject,
                                    new WhereAmI(gameObject, home,
                                            new DumpGold(gameObject),
                                            new MoveTo(home, gameObject, speed)),
                                    new WhereAmI(gameObject, gold,
                                            new GatherGold(gameObject),
                                            new MoveTo(gold, gameObject, speed)));
    }

    void Update()
    {
        IDecision currentDecision = decisionTreeRoot;
        while (currentDecision != null)
        {
            currentDecision = currentDecision.MakeDecision();
        }
    }

    class IsGoldFull : IDecision
    {
        private GameObject agent;
        private IDecision trueBranch;
        private IDecision falseBranch;
        private int gold;
        private int goldCap;

        public IsGoldFull() { }

        public IsGoldFull(GameObject agent, IDecision trueBranch, IDecision falseBranch)
        {
            this.agent = agent;
            this.trueBranch = trueBranch;
            this.falseBranch = falseBranch;
        }

        public IDecision MakeDecision()
        {
            gold = agent.GetComponent<Miner>().goldMined;
            goldCap = agent.GetComponent<Miner>().goldCapacity;
            return gold >= goldCap ? trueBranch : falseBranch;
        }
    }

    class WhereAmI : IDecision
    {
        private GameObject agent;
        private Transform target;
        private IDecision trueBranch;
        private IDecision falseBranch;

        public WhereAmI() { }

        public WhereAmI(GameObject agent, Transform target, IDecision trueBranch, IDecision falseBranch)
        {
            this.agent = agent;
            this.target = target;
            this.trueBranch = trueBranch;
            this.falseBranch = falseBranch;
        }

        public IDecision MakeDecision()
        {
            return Vector3.Distance(agent.transform.position, target.transform.position) < 1 ? trueBranch : falseBranch;
        }
    }

    class DumpGold : IDecision
    {
        GameObject agent;

        public DumpGold() { }

        public DumpGold(GameObject agent)
        {
            this.agent = agent;
        }

        public IDecision MakeDecision()
        {
            agent.GetComponent<Miner>().goldMined = 0;
            return null;
        }
    }

    class GatherGold : IDecision
    {
        GameObject agent;

        public GatherGold() { }

        public GatherGold(GameObject agent)
        {
            this.agent = agent;
        }

        public IDecision MakeDecision()
        {
            agent.GetComponent<Miner>().goldMined = agent.GetComponent<Miner>().goldCapacity;
            return null;
        }
    }

    class MoveTo : IDecision
    {
        private Vector3 CurrentVelocity = new Vector3(0, 0, 0);
        private Transform target;
        private GameObject agent;
        private float speed;

        public MoveTo() { }

        public MoveTo(Transform target, GameObject agent, float speed)
        {
            this.target = target;
            this.agent = agent;
            this.speed = speed;
        }

        public IDecision MakeDecision()
        {
            Vector3 v = ((target.transform.position - agent.transform.position) * speed).normalized;
            Vector3 force = v - CurrentVelocity;
            CurrentVelocity += force * Time.deltaTime;
            agent.transform.position += CurrentVelocity * speed * Time.deltaTime;

            return null;
        }
    }
}