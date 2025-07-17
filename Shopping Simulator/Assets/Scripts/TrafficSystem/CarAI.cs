using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class CarAI : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 1.1f;

    public float stopDistance=4;

    [SerializeField, Tooltip("An array of transforms representing the car's wheels.")]
    private Transform[] wheels;

    public Waypoint nearestWayPoint;

    private List<Tween> wheelTweens = new List<Tween>();

    [Header("Audio")]
    [SerializeField, Tooltip("The sound clip to play for the car's engine.")]
    private AudioClip engineSound;

    [SerializeField, Tooltip("The audio mixer group to route the engine sound to.")]
    private AudioMixerGroup sfxMixer;


    void Start()
    {
        CreateEngineAudio();
    }

    /*
        IEnumerator MoveRoutine(Waypoint current)
        {
            while (true)
            {
                // 1) Trafik ışığı beklemesi
                if (current.LightAtPoint != null && !current.LightAtPoint.IsGreen)
                {
                    PauseWheels();
                    yield return new WaitUntil(() => current.LightAtPoint.IsGreen);
                }

                // 2) Bir sonraki waypoint'i seç
                Waypoint next = GetNextWaypoint(current);
                if (next == null) {
                     Destroy(gameObject);
                }

                // 3) Dönüş doğrultusunu belirle
                //    Eğer "Turn" noktasındaysan, lineDir olarak nextNext yönünü alabilirsin:
                Waypoint nextNext = next.ConnectedWaypoints
                                        .FirstOrDefault(wp => wp != current && wp.Type == WaypointType.Pass);
                Vector3 lineDir;
                if (current.Type == WaypointType.Turn && nextNext != null)
                {
                    // Turn noktasında: next → nextNext doğrultusu
                    lineDir = (nextNext.transform.position - next.transform.position).normalized;
                    Debug.DrawLine(next.transform.position, nextNext.transform.position, Color.green, 1f);
                }
                else
                {
                    // Pass noktasında veya nextNext yoksa: doğrudan next yönü
                    lineDir = (next.transform.position - transform.position).normalized;
                }

                // 4) Hareket ve dönüş için başlangıç/değerler
                Vector3 startPos = transform.position;
                Vector3 endPos = next.transform.position;
                float totalDist = Vector3.Distance(startPos, endPos);
                Quaternion startRot = transform.rotation;
                Quaternion endRot = Quaternion.LookRotation(lineDir);

                // Tekerlekleri harekete hazırla
                ResumeWheels();

                float travelled = 0f;
                while (travelled < totalDist)
                {
                    // 4a) İleri hareket
                    float step = speed * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, endPos, step);

                    // 4b) Ne kadar yol kat ettik?
                    travelled = Vector3.Distance(startPos, transform.position);
                    float t = Mathf.Clamp01(travelled / totalDist);

                    // 4c) Orantılı dönüş
                    transform.rotation = Quaternion.Slerp(startRot, endRot, t);

                    yield return null;
                }

                // Varışta pozisyonu ve rotasyonu tam olarak garantile
                transform.position = endPos;
                transform.rotation = endRot;

                // 5) Varışta tekerlekleri durdur
                PauseWheels();

                // 6) Sonraki döngü
                current = next;
                yield return null;
            }
        }






        Waypoint GetNextWaypoint(Waypoint current)
        {
            Waypoint next = null;

            if (current.Type == WaypointType.Pass)
            {
                next = current.ConnectedWaypoints.FirstOrDefault();
                _justTurned = false;
            }
            else
            {
                if (!_justTurned)
                {
                    var options = current.ConnectedWaypoints.Where(wp => wp.Type == WaypointType.Pass).ToList();
                    if (options.Count > 0)
                    {
                        next = options[Random.Range(0, options.Count)];
                        _justTurned = true;
                    }
                }

                if (next == null)
                {
                    next = current.ConnectedWaypoints.FirstOrDefault();
                    _justTurned = false;
                }
            }

            return next;
        }
    */


    public void Play()
    {
        RotateWheels();
        StartCoroutine(MoveRoutine());
    }

    public IEnumerator MoveRoutine()
    {
        // 1) Sahnedeki tüm waypoint'leri topla
        /* var allWaypoints = FindObjectsOfType<Waypoint>();
         if (allWaypoints.Length == 0)
         {
             Debug.LogError("Hiç waypoint bulunamadı!");
             yield break;
         }

         // 2) İlk hedef: en yakındaki waypoint
         Waypoint current = allWaypoints
             .OrderBy(wp => Vector3.Distance(transform.position, wp.transform.position))
             .First();
 */
        Waypoint current = nearestWayPoint;

        Waypoint next = current;
        bool isFirstMove = true;

        while (true)
        {
            // Trafik ışığı beklemesi (varsa)
            if (current.LightAtPoint != null && !current.LightAtPoint.IsGreen)
            {
                PauseWheels();
                yield return new WaitUntil(() => current.LightAtPoint.IsGreen);
            }

            // Sıradaki waypoint'i belirle
            if (isFirstMove)
            {
                isFirstMove = false;
            }
            else
            {
                var connections = current.ConnectedWaypoints;
                if (connections == null || connections.Count == 0)
                {
                    CarCountControl.Instance.DecreaseCarNumber();
                    Destroy(gameObject);
                    yield break;
                }

                next = connections.Count >= 2
                    ? connections[Random.Range(0, connections.Count)]
                    : connections[0];
                current = next;
            }

            if (next == null)
            {
                Debug.Log("Next waypoint bulunamadı, MoveRoutine sonlanıyor.");
                CarCountControl.Instance.DecreaseCarNumber();
                Destroy(gameObject);
                yield break;
            }

            // 4) Hareket ve dönüş için hazırlık
            Vector3 startPos = transform.position;
            Vector3 endPos = next.transform.position;
            float totalDist = Vector3.Distance(startPos, endPos);        
            Vector3 direction = (endPos - startPos).normalized;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation(direction);

            float travelled = 0f;
            while (travelled < totalDist)
            {

                Vector3 rayOrigin = transform.position + new Vector3(0f, 0.6f, 0f);
                // --- ÖNCE RAYCAST KONTROLÜ ---
                Debug.DrawRay(rayOrigin, transform.forward * stopDistance, Color.red);

                Vector3 rayDir = transform.forward;

                // Tüm çarpışmaları al:
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDir, stopDistance);

                // “Geçerli araç” hariç, ilk başka aracı tespit et:
                bool vehicleAhead = false;
                foreach (var hit in hits)
                {
                    // Sadece Vehicle tag’li, ama kendimiz olmayan collider’ları say:
                    if (hit.collider.CompareTag("Car")
                        && hit.collider.gameObject != gameObject)
                    {
                        vehicleAhead = true;
                        break;
                    }
                }

                if (vehicleAhead)
                {
                    // Önünde araç var: dur
                    PauseWheels();
                    yield return null;
                    continue;
                }
                else
                {
                    // Önü temiz: devam et
                    ResumeWheels();
                }

         
                float step = speed * Time.deltaTime;             
                transform.position = Vector3.MoveTowards(transform.position, endPos, step);

                travelled = Vector3.Distance(startPos, transform.position);
                float t = Mathf.Clamp01(travelled / totalDist);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);

                yield return null;
            }

         
            transform.position = endPos;
            transform.rotation = endRot;
            PauseWheels();

      
            yield return null;
        }
    }


    private void RotateWheels()
    {
        wheelTweens.Clear();

        foreach (Transform wheel in wheels)
        {
            float diameter = wheel.GetComponent<MeshRenderer>().bounds.size.z;
            float circumference = Mathf.PI * diameter;
            float rotationSpeed = (speed / circumference) * 360f;

            Tween t = wheel.DOLocalRotate(Vector3.right * 360f, rotationSpeed, RotateMode.FastBeyond360)
                          .SetSpeedBased()
                          .SetEase(Ease.Linear)
                          .SetLoops(-1, LoopType.Restart)
                          .Pause(); // Başta durur

            wheelTweens.Add(t);
        }
    }

    private void ResumeWheels()
    {
        foreach (var tween in wheelTweens)
        {
            tween.Play();
        }
    }

    // Tekerlek animasyonlarını durdurur
    private void PauseWheels()
    {
        foreach (var tween in wheelTweens)
        {
            tween.Pause();
        }
    }

    private void CreateEngineAudio()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = engineSound;
        audioSource.outputAudioMixerGroup = sfxMixer;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.maxDistance = 15f;
        audioSource.Play();
    }

}
