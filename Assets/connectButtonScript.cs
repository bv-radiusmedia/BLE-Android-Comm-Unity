using UnityEngine;
using UnityEngine.UI;

public class connectButtonScript : MonoBehaviour
{

    public controller controllerScript;
    public Text TextName;
    public Text TextAddress;

    public void onClick()
    {
        controllerScript.connectTo(TextName.text, TextAddress.text);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}