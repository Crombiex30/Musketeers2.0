using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    private Vector3 _movement;
    private Animator _animator;
    [SerializeField] private float _rotationSpeed = 500f;
    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

        // Update is called once per frame
        void Update()
    {
        agent = GetComponent<NavMeshAgent>();

        //Follows player within certain radius
        if((player.position.x - agent.transform.position.x < 3 && player.position.x - agent.transform.position.x > -3)
            && (player.position.z - agent.transform.position.z < 3 && player.position.z - agent.transform.position.z > -3))
        {
            agent.SetDestination(player.position);
        }

        if (_movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(_movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
        }
        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.z);

    }
}
