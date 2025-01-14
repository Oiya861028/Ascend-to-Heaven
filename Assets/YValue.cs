using UnityEngine;

public class YValue : MonoBehaviour
{
    public float yValue = 0;
    //Using Static make it so you can access the variable from other scripts at real time
    public static YValue ins;
    void Awake()
    {
        ins = this;
    }

}
