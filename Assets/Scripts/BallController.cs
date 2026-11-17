
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BallController : MonoBehaviour
{
    [Header("Throw Settings")]
    public float minSwipeDistance = 30f;
    public float throwForce = 12f;
    public float upwardForce = 0.8f;
    public float swipeSensitivity = 1.5f;

    [Header("References")]
    public GameManager gameManager;
    public Transform ballSpawnPoint;
    public BasketScoreTrigger scoreTrigger;

    [Header("Wall Bounce")]
    public Collider wallCollider;

    [Range(0.1f, 1.5f)]
    public float wallBounceMultiplier = 0.8f;

    public ParticleSystem bounceEffectPrefab;

    [Header("Ball Position On Screen")]
    [Range(0.05f, 0.95f)]
    public float ballScreenX = 0.5f;

    [Range(0.05f, 0.95f)]
    public float ballScreenY = 0.18f;

    public float ballDistanceFromCamera = 0.8f;

    [Header("Ball Reset")]
    public float resetDelay = 6f;

    [Header("Tutorial Settings")]
    public bool canThrowBall = false;

    [Header("Touch Detection")]
    public float touchRadiusPixels = 160f;

    private Rigidbody rb;
    private Camera mainCamera;

    private bool hasBeenThrown;
    private bool isDragging;
    private bool gameHasStarted;

    private Vector2 startTouch;
    private Vector2 endTouch;

    private Coroutine resetCoroutine;

    // =====================================
    // START
    // =====================================

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (rb == null)
        {
            Debug.LogError("BallController: Rigidbody tidak ditemukan!");
            enabled = false;
            return;
        }

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("BallController: GameManager tidak ditemukan!");
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("BallController: Main Camera tidak ditemukan!");
            enabled = false;
            return;
        }

        // Reset velocity sebelum mengaktifkan Kinematic.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        hasBeenThrown = false;
        isDragging = false;
        gameHasStarted = false;

        PlaceBallAtSpawn();

        Debug.Log("Bola siap dimainkan!");
    }

    // =====================================
    // UPDATE
    // =====================================

    private void Update()
    {
        if (!canThrowBall)
            return;

        if (hasBeenThrown || rb == null)
            return;

        if (gameManager == null || gameManager.IsGameOver())
            return;

        HandleInput();
    }

    // =====================================
    // LATE UPDATE
    // =====================================

    private void LateUpdate()
    {
        if (!hasBeenThrown && rb != null && mainCamera != null)
        {
            PlaceBallAtSpawn();
        }
    }

    // =====================================
    // INPUT
    // =====================================

    private void HandleInput()
    {
        // INPUT HP
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(touch.fingerId))
                    return;

                if (IsTouchingBall(touch.position))
                {
                    startTouch = touch.position;
                    isDragging = true;

                    StartGameOnFirstTouch();

                    Debug.Log("Bola disentuh!");
                }
            }

            if (touch.phase == TouchPhase.Ended && isDragging)
            {
                endTouch = touch.position;
                isDragging = false;

                ThrowBall();
            }

            if (touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // INPUT MOUSE UNTUK UNITY EDITOR
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverUI())
                    return;

                if (IsTouchingBall(Input.mousePosition))
                {
                    startTouch = Input.mousePosition;
                    isDragging = true;

                    StartGameOnFirstTouch();

                    Debug.Log("Bola disentuh menggunakan mouse!");
                }
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                endTouch = Input.mousePosition;
                isDragging = false;

                ThrowBall();
            }
        }
    }

    // =====================================
    // DETECT BALL TOUCH
    // =====================================

    private bool IsTouchingBall(Vector2 screenPosition)
    {
        if (mainCamera == null)
            return false;

        Vector3 ballScreenPosition =
            mainCamera.WorldToScreenPoint(transform.position);

        if (ballScreenPosition.z <= 0f)
            return false;

        float distance = Vector2.Distance(
            screenPosition,
            new Vector2(
                ballScreenPosition.x,
                ballScreenPosition.y
            )
        );

        bool touched = distance <= touchRadiusPixels;

        if (touched)
            Debug.Log("BOLA BERHASIL DISENTUH!");

        return touched;
    }

    // =====================================
    // DETECT UI
    // =====================================

    private bool IsPointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null)
            return false;

        if (fingerId >= 0)
        {
            return EventSystem.current
                .IsPointerOverGameObject(fingerId);
        }

        return EventSystem.current
            .IsPointerOverGameObject();
    }

    // =====================================
    // START TIMER ON FIRST TOUCH
    // =====================================

    private void StartGameOnFirstTouch()
    {
        if (gameHasStarted)
            return;

        gameHasStarted = true;

        if (gameManager != null)
            gameManager.StartTimerOnFirstThrow();

        Debug.Log("Timer dimulai saat bola disentuh!");
    }

    // =====================================
    // THROW BALL
    // =====================================

    private void ThrowBall()
    {
        Vector2 swipe = endTouch - startTouch;

        if (swipe.magnitude < minSwipeDistance)
        {
            Debug.Log("Swipe terlalu pendek. Coba geser lebih jauh.");
            return;
        }

        if (gameManager == null || mainCamera == null)
            return;

        if (gameManager.IsGameOver())
            return;

        hasBeenThrown = true;

        // SIMPAN JARAK KAMERA KE RING
        if (scoreTrigger != null)
        {
            float distanceInMeters = Vector3.Distance(
                mainCamera.transform.position,
                scoreTrigger.transform.position
            );

            scoreTrigger.SetThrowDistance(distanceInMeters);

            Debug.Log(
                "JARAK KAMERA KE RING: " +
                distanceInMeters.ToString("F2") +
                " meter"
            );
        }
        else
        {
            Debug.LogWarning(
                "BasketScoreTrigger belum dihubungkan!"
            );
        }

        // LEPASKAN BOLA DARI PARENT
        transform.SetParent(null, true);

        // AKTIFKAN FISIKA
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // HITUNG KEKUATAN SWIPE
        float swipeStrength = Mathf.Clamp01(
            swipe.magnitude / Screen.height
        );

        float horizontal =
            (swipe.x / Screen.width) * swipeSensitivity;

        float vertical =
            (swipe.y / Screen.height) * swipeSensitivity;

        // ARAH LEMPARAN
        Vector3 direction =
            mainCamera.transform.forward
            + mainCamera.transform.right * horizontal
            + mainCamera.transform.up * (upwardForce + vertical);

        float finalForce =
            throwForce * (0.5f + swipeStrength);

        rb.AddForce(
            direction.normalized * finalForce,
            ForceMode.Impulse
        );

        Debug.Log("BOLA BERHASIL DILEMPAR!");

        // RESET BOLA
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetBall());
    }

    // =====================================
    // WALL COLLISION / BOUNCE
    // =====================================

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown)
            return;

        if (wallCollider == null)
        {
            Debug.LogWarning("WallCollider belum dihubungkan!");
            return;
        }

        bool hitWall =
            collision.collider == wallCollider ||
            collision.collider.transform == wallCollider.transform ||
            collision.collider.transform.IsChildOf(
                wallCollider.transform
            );

        if (!hitWall)
            return;

        if (collision.contactCount == 0)
            return;

        ContactPoint contact = collision.GetContact(0);

        Debug.Log("BOLA MEMBENTUR DINDING!");

        Vector3 incomingVelocity = rb.linearVelocity;

        Vector3 reflectedVelocity = Vector3.Reflect(
            incomingVelocity,
            contact.normal
        );

        if (!rb.isKinematic &&
            reflectedVelocity.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity =
                reflectedVelocity * wallBounceMultiplier;
        }

        PlayBounceEffect(contact);

        Debug.Log(
            "KECEPATAN SETELAH PANTUL: " +
            rb.linearVelocity
        );
    }

    // =====================================
    // BOUNCE EFFECT
    // =====================================

    private void PlayBounceEffect(ContactPoint contact)
    {
        if (bounceEffectPrefab == null)
            return;

        ParticleSystem effect = Instantiate(
            bounceEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        effect.Play();

        float effectDuration =
            effect.main.duration +
            effect.main.startLifetime.constantMax +
            0.5f;

        Destroy(effect.gameObject, effectDuration);
    }

    // =====================================
    // RESET BALL
    // =====================================

    private IEnumerator ResetBall()
    {
        yield return new WaitForSeconds(resetDelay);

        if (rb == null)
            yield break;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        if (scoreTrigger != null)
            scoreTrigger.ResetTrigger();

        hasBeenThrown = false;
        isDragging = false;

        PlaceBallAtSpawn();

        resetCoroutine = null;

        Debug.Log("Bola kembali ke posisi awal!");
    }

    // =====================================
    // PLACE BALL AT SPAWN
    // =====================================

    private void PlaceBallAtSpawn()
    {
        if (mainCamera == null)
            return;

        Vector3 viewportPosition = new Vector3(
            ballScreenX,
            ballScreenY,
            ballDistanceFromCamera
        );

        Vector3 spawnPosition =
            mainCamera.ViewportToWorldPoint(viewportPosition);

        transform.position = spawnPosition;
        transform.rotation = mainCamera.transform.rotation;
    }

    // =====================================
    // TUTORIAL CONTROL
    // =====================================

    public void SetCanThrowBall(bool allowed)
    {
        canThrowBall = allowed;

        if (!allowed)
            isDragging = false;

        Debug.Log(
            "Kontrol bola: " +
            (allowed ? "AKTIF" : "NONAKTIF")
        );
    }
}
