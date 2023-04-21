using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ToTrillionFast : MonoBehaviour
{
    public GameObject prefabMillion;
    public GameObject prefabFace;
    public GameObject prefabTextureTop;
    public GameObject prefabTextureRight;
    public float delay = 0.2f;
    public AudioClip millionSound;

    private bool[,] matrix = new bool[100, 100];
    private int counter = 1;
    private int staticCounter = 0;
    private GameObject uText;
    private GameObject sText;
    private GameObject ssText;
    private Text text;
    private Text stext;
    private Text sstext;

    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip clip3;
    public AudioClip clip4;
    public AudioClip clip5;
    private int clipCounter = 1;
    private AudioSource audioMusic;

    private Renderer renderFace;
    private Renderer renderDynamicTop;
    private Renderer renderDynamicRight;
    private Renderer renderStaticTop;
    private Renderer renderStaticRight;
    private GameObject face;
    private GameObject textureStaticTop;
    private GameObject textureStaticRight;
    private GameObject textureDynamicTop;
    private GameObject textureDynamicRight;

    private bool paint = false;
    private bool needMusic = true;
    private void Awake() {
        audioMusic = gameObject.AddComponent<AudioSource>();
        audioMusic.volume = 0.6f;
        audioMusic.clip = clip1;
        audioMusic.Play();
    }

    private void Start() {
        uText = GameObject.Find("Text");
        text = uText.GetComponent<Text>();

        sText = GameObject.Find("TextCounter");
        stext = sText.GetComponent<Text>();

        Invoke("startPaint", 1f);
    }

    private void Update() {
        if (needMusic) {
            checkSound();
        }
    }

    private void FixedUpdate() {
        if (paint) {
            paint = false;
            paintBlock();
        }
    }

    private void startPaint() {
        paint = true;
    }

    private void paintBlock() {

        if ( sstext == null) {
            ssText = GameObject.Find("Text3");
            sstext = ssText.GetComponent<Text>();
            sstext.text = "3";
        }

        //лица
        if (face == null) {
            face = Instantiate<GameObject>(prefabFace);
            face.transform.position = Vector3.zero;
            renderFace = face.GetComponent<Renderer>();

            textureDynamicTop = Instantiate<GameObject>(prefabTextureTop);
            renderDynamicTop = textureDynamicTop.GetComponent<Renderer>();

            textureDynamicRight = Instantiate<GameObject>(prefabTextureRight);
            renderDynamicRight = textureDynamicRight.GetComponent<Renderer>();
        }

        //
        textureDynamicTop.transform.localScale = new Vector3(counter, counter, counter);
        textureDynamicTop.transform.position = new Vector3((float)counter / 2 - 0.5f, counter - 0.5f, (float)-counter/2);
        renderDynamicTop.material.mainTextureScale = new Vector2(counter, counter);

        textureDynamicRight.transform.localScale = new Vector3(counter, counter, counter);
        textureDynamicRight.transform.position = new Vector3(counter - 0.5f, (float)counter / 2 - 0.5f, (float)-counter / 2);
        renderDynamicRight.material.mainTextureScale = new Vector2(counter, counter);

        face.transform.localScale = new Vector3(counter, counter, counter);
        face.transform.position = new Vector3((float)counter / 2 - 0.5f, (float)counter / 2 - 0.5f, -counter);
        renderFace.material.mainTextureScale = new Vector2(counter, counter);

        Camera.main.GetComponent<CameraOffSet>().setView(counter);
        int sum = counter * counter * counter;
        text.text = counter + " * " + counter + " * " + counter + " = " + sum.ToString("#,#", CultureInfo.InvariantCulture);
        stext.text = counter.ToString();

        counter++;

        if (counter < 101) {
            Invoke("startPaint", delay);
        } else {
            clearBlock("Texture");
            GameObject g = Instantiate<GameObject>(prefabMillion);
            g.transform.position = new Vector3(0, 0, -100f);
        }
        /*if (!matrix[99, 99]) {
            for (int xy = 0; xy < 100; xy++) {
                for (int z = 0; z < 100; z++) {
                    if (matrix[xy, z])
                        continue;

                    //инициализируем объекты текстур динамических блоков
                    if (textureDynamicTop == null) {
                        textureDynamicTop = Instantiate<GameObject>(prefabTextureTop);
                        renderDynamicTop = textureDynamicTop.GetComponent<Renderer>();

                        textureDynamicRight = Instantiate<GameObject>(prefabTextureRight);
                        renderDynamicRight = textureDynamicRight.GetComponent<Renderer>();
                    }

                    //верх динамик
                    textureDynamicTop.transform.localScale = new Vector3(xy + 1, z + 1, 1);
                    textureDynamicTop.transform.position = new Vector3((float)(xy + 1) / 2 - 0.5f, xy + 0.5f, (float)(z + 1) / 2);
                    //print("xy: " + xy + " z: " + z + " || localScale: " + new Vector3(xy + 1, z + 1, 1) + " transform position: " + new Vector3(xy, xy+0.5f, (float)(z + 1) / 2));
                    renderDynamicTop.material.mainTextureScale = new Vector2(xy + 1, z + 1);

                    //бок динамик
                    textureDynamicRight.transform.localScale = new Vector3(z + 1, xy + 1, 1);
                    textureDynamicRight.transform.position = new Vector3(xy + 0.5f, (float)(xy + 1) / 2 - 0.5f, (float)(z + 1) / 2);
                    renderDynamicRight.material.mainTextureScale = new Vector2(z + 1, xy + 1);

                    //лица
                    if (face == null) {
                        face = Instantiate<GameObject>(prefabFace);
                        face.transform.position = Vector3.zero;
                        renderFace = face.GetComponent<Renderer>();
                    }
                    face.transform.localScale = new Vector3(xy + 1, xy + 1, 1);
                    renderFace.material.mainTextureScale = new Vector2(xy + 1, xy + 1);

                    //обхекм кубов умножаем на не используемое простанство + формирующийся куб
                    counter = (staticCounter * (100 - (z + 1))) +
                        (int)((textureDynamicTop.transform.localScale.x * textureDynamicRight.transform.localScale.y) * (z + 1));
                    text.text = counter.ToString("#,#", CultureInfo.InvariantCulture);

                    matrix[xy, z] = true;

                    //выставляем камеру
                    if (z == 0) {
                        Camera.main.GetComponent<CameraOffSet>().setView(new Vector3(xy, xy, z));
                    }

                    if (z == 99) {
                        face.transform.position = new Vector3(
                            face.transform.position.x + 0.5f,
                            face.transform.position.y + 0.5f,
                            face.transform.position.z);
                        //инициализация статических блоков
                        if (textureStaticTop == null) {
                            textureStaticTop = Instantiate<GameObject>(prefabTextureTop);
                            renderStaticTop = textureStaticTop.GetComponent<Renderer>();

                            textureStaticRight = Instantiate<GameObject>(prefabTextureRight);
                            renderStaticRight = textureStaticRight.GetComponent<Renderer>();
                        }

                        //присваиваем статическим блокам последнее значение динамических
                        textureStaticTop.transform.localScale = textureDynamicTop.transform.localScale;
                        textureStaticTop.transform.position = textureDynamicTop.transform.position;
                        renderStaticTop.material.mainTextureScale = renderDynamicTop.material.mainTextureScale;

                        textureStaticRight.transform.localScale = textureDynamicRight.transform.localScale;
                        textureStaticRight.transform.position = textureDynamicRight.transform.position;
                        renderStaticRight.material.mainTextureScale = renderDynamicRight.material.mainTextureScale;

                        //сумма кубок в статике
                        staticCounter = (int)(textureStaticTop.transform.localScale.x * textureStaticRight.transform.localScale.y);

                    }
                    Invoke("startPaint", 0.001f);
                    return;
                }
            }
            //если вся матрица true - убираем все блоки - ставим миллион и проигрываем звук
        } else {
            clearBlock("Texture");
            GameObject g = Instantiate<GameObject>(prefabMillion);
            g.transform.position = Vector3.zero;
            needMusic = false;
            audioMusic.Stop();
            audioMusic.PlayOneShot(millionSound);

        */
    }

    

    private void clearBlock(string tag) {
        GameObject[] gList = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject g in gList) {
            Destroy(g);
        }
    }

    private void checkSound() {
        if (!audioMusic.isPlaying) {
            clipCounter++;
            if (clipCounter > 5) {
                clipCounter = 1;
            }
            switch (clipCounter) {
                case 1:
                    audioMusic.clip = clip1;
                    break;
                case 2:
                    audioMusic.clip = clip2;
                    break;
                case 3:
                    audioMusic.clip = clip3;
                    break;
                case 4:
                    audioMusic.clip = clip4;
                    break;
                case 5:
                    audioMusic.clip = clip5;
                    break;
            }
            audioMusic.Play();
        }
    }
    
}
