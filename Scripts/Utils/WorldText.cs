using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldText
{
    public static TextMesh CreateWorldText (Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int layer, int sortingOrder)
    {
        GameObject gameObject = new GameObject( "World_Text", typeof( TextMesh ) );
        Transform transform = gameObject.transform;
        transform.SetParent( parent, false );
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.transform.localScale = new Vector3( 0.1f, 0.1f );
        textMesh.GetComponent<MeshRenderer>().sortingLayerName = "Grid";
        gameObject.layer = layer;
        return textMesh;
    }

}
