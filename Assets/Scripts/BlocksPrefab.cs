using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksPrefab : MonoBehaviour
{
    //Список блоков
    public GameObject prefabOne;
    public GameObject prefabTen;
    public GameObject prefabOneHundred;
    public GameObject prefabOneThousand;
    public GameObject prefabTenThousand;
    public GameObject prefabOneHundredThousand;
    public GameObject prefabBigSquare;
    public GameObject prefabBigNotSquare;
    public GameObject prefabBigTen;
    public GameObject prefabBigOneHundred;

    private List<GameObject> listPrefab;

    //сначала маленькие блоки
    private bool _isBigBlock = false;
    public bool isBigBlock {
        get {
            return _isBigBlock;
        }
        set {
            _isBigBlock = value;
        }
    }

    //главный счетчик для блоков
    private int _counter = 0;
    //счетчик для больших блоков
    private int _counterForBig = 1;
    //квадратный блок или нет
    private bool _isSquare = true;

    public bool isSquare {
        get { return _isSquare; }
        set { _isSquare = value; }
    }

    public int counterForBig {
        get { return _counterForBig; }
        set { _counterForBig = value; }
    }

    private void Start() {
        createListPrefab();
    }

    public GameObject getPrefab() {
        _counter++;
        return getPrefab(_counter);
    }

    public GameObject getPrefab(int counter) {
        //print("counter: " + counter + " isBigBlock: " + isBigBlock + " counterForBig " + counterForBig  );
        GameObject link = null;
        if (!isBigBlock) {
            switch (counter) {
                case 1:
                    link = prefabOne;
                    break;
                case 2:
                    link = prefabTen;
                    break;
                case 3:
                    link = prefabOneHundred;
                    break;
                case 4:
                    link = prefabOneThousand;
                    break;
                case 5:
                    link = prefabTenThousand;
                    break;
                case 6:
                    link = prefabOneHundredThousand;
                    // с маленькими закончили - начинаем большие
                    isBigBlock = true;
                    break;
            }
        } else {
            //стали большие блоки
            switch (counterForBig) {
                case 1:
                    if (isSquare) {
                        link = prefabBigSquare;
                    } else {
                        link = prefabBigNotSquare;
                    }
                    isSquare = !isSquare;
                    counterForBig++;
                    break;
                case 2:
                    link = prefabBigTen;
                    counterForBig++;
                    break;
                case 3:
                    link = prefabBigOneHundred;
                    counterForBig = 1;
                    break;
            }
        }
        //print("link prefab: " + link);
        return link;
    }

    private void createListPrefab() {
        listPrefab = new List<GameObject>() {
            prefabOne,
            prefabTen,
            prefabOneHundred,
            prefabOneThousand,
            prefabTenThousand,
            prefabOneHundredThousand,
            prefabBigSquare,
            prefabBigNotSquare,
            prefabBigTen,
            prefabBigOneHundred
        };
    }
    public GameObject getPrefabFormList(int number) {
        return listPrefab[number];
    }
}
