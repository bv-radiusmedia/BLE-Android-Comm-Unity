using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;

public class Level1Script : MonoBehaviour
{
    public Transform Content;
    private List<string> names;
    public TextMeshProUGUI display_text;
    private void Start()
    {
        //StartCoroutine(Searching());
        OnScanClick();
        names = new List<string>();
    }
    public void OnScanClick ()
	{
		BluetoothLEHardwareInterface.Initialize (true, false, () => {

            FoundDeviceListScript.DeviceAddressList = new List<DeviceObject> ();

			BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {

				FoundDeviceListScript.DeviceAddressList.Add (new DeviceObject (address, name));
                Debug.Log("Bluetooth initialized : "+FoundDeviceListScript.DeviceAddressList.Count);
                
                //AddButtons(name);

			}, null);

		}, (error) => {
			BluetoothLEHardwareInterface.Log ("BLE Error: " + error);

		});
        //StopAllCoroutines();
        //StartCoroutine(Searching());
        StartCoroutine(ChangeLevel());
    }

    IEnumerator Searching()
    {
        yield return new WaitForSeconds(0.1f);
        OnScanClick();
        //display_text.text = "Devices list : " + FoundDeviceListScript.DeviceAddressList.Count;
        StartCoroutine(ChangeLevel());
    }

    IEnumerator ChangeLevel()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
    }

    private void AddButtons(string name)
    {
        display_text.text = "Devices list : " + FoundDeviceListScript.DeviceAddressList.Count;
        for(int i = 0; i < FoundDeviceListScript.DeviceAddressList.Count; i++)
        {
            GameObject x = Instantiate(Resources.Load("Button")) as GameObject;
            x.transform.SetParent(Content);
            x.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = FoundDeviceListScript.DeviceAddressList[i].Name;
            if(i == FoundDeviceListScript.DeviceAddressList.Count - 1)
            {
                StartCoroutine(ChangeLevel());
            }
        }
        
    }

	public void OnStartLevel2 ()
	{
		SceneManager.LoadScene ("Level2");
	}
}
