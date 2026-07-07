using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S41_DisplayWinner : MonoBehaviour
{
    private bool oneUse = true;
    public GameObject panel;
    public TextMeshProUGUI textMeshWinner;
    public float sizeText = 300f;
    private void OnTriggerEnter(Collider other) {
        if (other.tag.Equals("Block") && oneUse ) {
            oneUse = false;
            int winScore = 0;
            string resultText = "";
            Transform[] transforms = panel.GetComponentsInChildren<Transform>();
            foreach (Transform trans in transforms) {
                if (trans.name.Equals("Rating"))
                    continue;
                TextMeshProUGUI textMesh = trans.gameObject.GetComponent<TextMeshProUGUI>();
                int score = int.Parse(textMesh.text.Split(" ")[2]);
                if (score > winScore) {
                    winScore = score;
                    resultText = "Winner block - " + trans.gameObject.name;
                } else if(winScore == score) {
                    resultText = "Friendship won!";
                }
            }
            textMeshWinner.fontSize = sizeText;
            textMeshWinner.text = resultText;
        }
    }
}
