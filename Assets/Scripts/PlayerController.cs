using System.Collections;
using UnityEngine;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody playerRb;
    public float speed = 5.0f;
    public bool hasPowerup = false;
    private float powerupStrength = 15f;
    public GameObject powerupIndicator;

    private SpawnManager spawnManager;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        spawnManager = FindObjectOfType<SpawnManager>();

        if (powerupIndicator != null)
        {
            powerupIndicator.SetActive(false); // Start with the indicator deactivated
        }
        else
        {
            Debug.LogError("Powerup Indicator not assigned.");
        }
    }

    void Update()
    {
        if (!base.IsOwner) return;

        // Movement input
        float forwardInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 moveDirection = new Vector3(horizontalInput, 0.0f, forwardInput).normalized;

        playerRb.AddForce(moveDirection * speed * Time.deltaTime, ForceMode.VelocityChange);

        // Update power-up indicator position
        if (hasPowerup && powerupIndicator != null)
        {
            powerupIndicator.transform.position = transform.position + Vector3.up * 0.5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!base.IsOwner) return;

        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            ActivatePowerupIndicator();

            if (base.IsServerInitialized)
            {
                spawnManager.DestroyPowerupServerRpc((ulong)other.GetComponent<NetworkObject>().ObjectId);
            }

            StartCoroutine(PowerupCountdown());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && hasPowerup)
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayDirection = collision.gameObject.transform.position - transform.position;
            otherRb.AddForce(awayDirection.normalized * powerupStrength, ForceMode.Impulse);
        }
    }

    IEnumerator PowerupCountdown()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
        DeactivatePowerupIndicator();
    }

    private void ActivatePowerupIndicator()
    {
        if (powerupIndicator != null)
        {
            powerupIndicator.SetActive(true);
        }
    }

    private void DeactivatePowerupIndicator()
    {
        if (powerupIndicator != null)
        {
            powerupIndicator.SetActive(false);
        }
    }
}
