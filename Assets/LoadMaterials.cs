using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public class LoadMaterials : ScriptableWizard
{
    
    public string[] path;
    public GameObject parent;
    public GameObject PrefabButton;
    public GameObject target;
    public GameObject PrefabScreenshot;
    public bool ModifyChild;
    public bool Recursive;
    private MeshRenderer mesh;
    private GameObject button;
    private Material[] materials;


    [MenuItem("My Tools/Create Material Buttons")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<LoadMaterials>("Create Buttons for materials", "Create Buttons");
    }

    void OnWizardCreate()
    {
        Image img;
        Sprite s;
        Camera camera;
        GameObject prefabCamera;

        // recherche les materiaux
        materials = researchMaterials();
        
        Debug.Log("Nombre de materiaux: " + materials.Length);

        // supprime les anciens boutons
        while (parent.transform.childCount > 0)
            DestroyImmediate(parent.transform.GetChild(0).gameObject);

        prefabCamera = Instantiate<GameObject>(PrefabScreenshot);

        // recupere le mesh de la sphere
        mesh = prefabCamera.GetComponentInChildren<MeshRenderer>();
        camera = prefabCamera.GetComponentInChildren<Camera>();

        // creer les boutons
        for (int i = 0; i < materials.Length; i++)
        {
            // modification du materiau de la sphere
            mesh.GetComponentInChildren<MeshRenderer>().material = materials[i];
            // enregistrement du screenshot
            s = getScreenshot(camera, 110, 110);
            // cree le bouton et edit ses parametres a partir du materiau
            button = Instantiate(PrefabButton, new Vector3(0, 0, 0), Quaternion.identity);
            button.transform.SetParent(parent.transform);
            button.transform.Find("Text").GetComponent<Text>().text = materials[i].name;


            // definir son image a partir du screenshot
            img = button.GetComponentInChildren<Image>();
            img.sprite = s;
        }

        DestroyImmediate(prefabCamera);

    }


    private Material[] researchMaterials()
    {
        string[] materialsName = AssetDatabase.FindAssets("t:Material", path);  // cherche les identifiants de tous les materiaux dans 'Assets/'
        Material[] materials = new Material[materialsName.Length];  // initialise le tableau de materiaux
        Object o;

        for (int i = 0; i < materials.Length; i++)
        {
            o = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(materialsName[i]), typeof(Material)); // recupere le chemin avec GUIDToAssetPath et charge l'asset

            if (o is Material)  // verifie s'il s'agit bien d'un materiau
            {
                materials[i] = (Material) o;
                Debug.Log("Material: "+materials[i].ToString()+" path: "+ AssetDatabase.GUIDToAssetPath(materialsName[i]));
            }
        }

        return materials;

    }

    private GameObject createCamera()
    {
        Camera cam;

        // creation  de la camera
        GameObject camera = new GameObject();
        camera.name = "cameraSphere";
        // proprietes camera
        cam = camera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 60;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000;
        cam.rect = new Rect(0, 0, 1, 1);
        cam.depth = -1;
        cam.useOcclusionCulling = true;
        cam.allowHDR = true;
        cam.allowMSAA = true;

        return camera;

    }

    private Sprite getScreenshot(Camera camera, int width, int height)
    {
        Sprite screenshot;
        int depth = 16;

        Rect r = new Rect(0, 0, width, height);
        RenderTexture render = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32);
        render.Create();
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        RenderTexture.active = render;

        camera.targetTexture = render;
        camera.Render();

        tex.ReadPixels(r, 0, 0);
        tex.Apply();

        screenshot = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        screenshot.name = "Materials";

        // reset 
        RenderTexture.active = null;
        camera.targetTexture = null;

        return screenshot;
    }

}
