using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class Map : Item
{
    // Duration map stays visible
    public float displayDuration = 10f;
    // Position and size of map on screen
    public Vector2 mapPosition = new Vector2(0.8f, 0.8f);
    public Vector2 mapSize = new Vector2(200f, 200f);
    
    private Camera minimapCamera;
    private RawImage minimapImage;
    private RenderTexture renderTexture;
    
    public override void Use()
    {
        // Create minimap camera
        GameObject camObj = new GameObject("MinimapCamera");
        minimapCamera = camObj.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 50f;
        minimapCamera.transform.position = playerTransform.position + Vector3.up * 100f;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Create render texture
        renderTexture = new RenderTexture(512, 512, 16);
        minimapCamera.targetTexture = renderTexture;
        
        // Create UI image
        GameObject imageObj = new GameObject("MinimapImage");
        imageObj.transform.SetParent(GameObject.Find("Canvas").transform, false);
        minimapImage = imageObj.AddComponent<RawImage>();
        minimapImage.texture = renderTexture;
        
        // Position and size the image
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = mapPosition;
        rect.sizeDelta = mapSize;
        StartCoroutine(DestroyMapAfterDelay());
        OnUse();
    }
    
    private IEnumerator DestroyMapAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        Destroy(minimapCamera.gameObject);
        Destroy(minimapImage.gameObject);
        renderTexture.Release();
    }
}