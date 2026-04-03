using UnityEditor.Build.Content;
using UnityEngine;

public class LevelEntrance : MonoBehaviour
{
    [SerializeField] private string nextLevelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<HeroKnight>() != null)
        {
            SceneChanger.instance.ChangeLevelTo(nextLevelName);
        }
    }
}
