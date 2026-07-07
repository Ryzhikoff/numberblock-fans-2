using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class S36_Collider : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    private void OnTriggerEnter(Collider other) {
        if(other.tag.Equals("ForTrigger")) {
            textMesh.text = other.gameObject.name;
            /*Scale scale = other.gameObject.GetComponent<Scale>();
            textMesh.text = scale.number.ToString("#,#", CultureInfo.InvariantCulture);*/
        }
    }
}
