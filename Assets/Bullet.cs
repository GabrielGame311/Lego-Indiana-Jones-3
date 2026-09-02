using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 25f;
    public float lifeTime = 3f;
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        //transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorera spelaren som sköt
        if (other.CompareTag("Player"))
        {
             PlayerHealth player = other.GetComponent<PlayerHealth>();
            
                player.TakeDamage(damage);
            
        }

        // Kolla om objektet vi träffade har EnemyAI-skriptet
      

        Destroy(gameObject); // Förstör skottet vid träff
    }


    void OnParticleCollision(GameObject other)
    {
         // Ignorera spelaren som sköt
        if (other.CompareTag("Player")) return;

        // Kolla om objektet vi träffade har EnemyAI-skriptet
       PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
        // Förstör skottet vid kollision med andra objekt
        Destroy(gameObject);
    }
}