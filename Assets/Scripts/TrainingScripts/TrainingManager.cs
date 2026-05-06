using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class TrainingManager : MonoBehaviour
{
    public HiveManager hiveManager;
    private List<NectarSource> cachedNectarSources = new List<NectarSource>();

    [Header("Training Settings")]
    public float timeBetweenLessons = 2.0f;

    void Awake()
    {
        // Cache once in Awake to prevent lag spikes in Update/Coroutines
        cachedNectarSources = transform.parent.GetComponentsInChildren<NectarSource>().ToList();
        hiveManager = transform.parent.GetComponentInChildren<HiveManager>();
    }

    void Start()
    {
        if (cachedNectarSources.Count > 0)
        {
            // Post the very first lesson
            PostNewLesson();
            // Start the repeating loop
            StartCoroutine(LessonLoop());
        }
    }

    IEnumerator LessonLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2.0f));

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenLessons);
            PostNewLesson();
        }
    }

    void PostNewLesson()
    {
        if (cachedNectarSources.Count == 0 || hiveManager == null) return;

        NectarSource randomSource = cachedNectarSources[Random.Range(0, cachedNectarSources.Count)];
        hiveManager.RegisterWaggleDance(randomSource.transform, 1.0f);
    }
}