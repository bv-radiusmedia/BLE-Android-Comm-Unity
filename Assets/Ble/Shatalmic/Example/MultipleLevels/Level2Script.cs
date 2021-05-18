using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Text;

public class Level2Script : MonoBehaviour
{
    public List<GameObject> Buttons;
	public List<TextMeshProUGUI> Buttons_Text;
	public List<string> Services;
	public List<string> Characteristics;

    public TextMeshProUGUI display_text, Main_text;
    private string address, ServiceUUID_, CharactersticUUID_, Device_Name;
    private bool connected_;
    public GameObject TextPanel;

    private string subscribedCharacteristic, subscribedService;

    // Use this for initialization
    void Start ()
	{
        connected_ = false;
        display_text.text = "List count : " + FoundDeviceListScript.DeviceAddressList.Count;
        //foreach (var device in FoundDeviceListScript.DeviceAddressList)
        //{
        for (int i = 0; i < FoundDeviceListScript.DeviceAddressList.Count; i++)
        {
            Buttons_Text[i].text = FoundDeviceListScript.DeviceAddressList[i].Name;
            Buttons[i].SetActive(true);
        }
		//}
	}

	void OnCharacteristic (string characteristic, byte[] bytes)
	{
		BluetoothLEHardwareInterface.Log ("received: " + characteristic);
	}

	public void OnSubscribeClick (int buttonID)
	{
		DeviceObject device = FoundDeviceListScript.DeviceAddressList[buttonID];
        string subscribedService = ServiceUUID_;//Services[buttonID];
        string subscribedCharacteristic = CharactersticUUID_; //Characteristics[buttonID];
        Main_text.text = "Subscribe clicked" + " \r\n";
        if (!string.IsNullOrEmpty (subscribedService) && !string.IsNullOrEmpty (subscribedCharacteristic))
		{
			BluetoothLEHardwareInterface.Log ("subscribing to: " + subscribedService + ", " + subscribedCharacteristic);
            Debug.Log("subscribing to: " + subscribedService + ", " + subscribedCharacteristic);
            Main_text.text += "About to subscribe to : " + subscribedService + ", " + subscribedCharacteristic + " \r\n";
            BluetoothLEHardwareInterface.SubscribeCharacteristic(device.Address, subscribedService, subscribedCharacteristic, null, (characteristic, bytes) => {

                //BluetoothLEHardwareInterface.Log("received data: " + characteristic);
                Main_text.text = "received data: " + characteristic;
                Main_text.text += "subscribing to: " + subscribedService + ", " + subscribedCharacteristic;
                Debug.Log("received data: " + characteristic);
            });
        }
    }
    public void ActivateTextPanel(TextMeshProUGUI Name_Text)
    {
        if (Name_Text.text.Contains("connected"))
        {
            connected_ = true;
            TextPanel.SetActive(true);
        }
    }

	public void OnButtonClick (int buttonID)
	{
		if (buttonID >= 0 && buttonID < 7)
		{
			DeviceObject device = FoundDeviceListScript.DeviceAddressList[buttonID];
			TextMeshProUGUI button = Buttons_Text[buttonID];
			subscribedService = Services[buttonID];
			subscribedCharacteristic = Characteristics[buttonID];

			if (device != null && button != null)
			{
				if (button.text.Contains ("connected"))
				{
					if (!string.IsNullOrEmpty (subscribedService) && !string.IsNullOrEmpty (subscribedCharacteristic))
					{
						BluetoothLEHardwareInterface.UnSubscribeCharacteristic (device.Address, subscribedService, subscribedCharacteristic, (characteristic) => {
							
							Services[buttonID] = null;
							Characteristics[buttonID] = null;
							
							BluetoothLEHardwareInterface.DisconnectPeripheral (device.Address, (disconnectAddress) => {
								
								button.text = device.Name;
							});
						});
					}
					else
					{
						BluetoothLEHardwareInterface.DisconnectPeripheral (device.Address, (disconnectAddress) => {
							
							button.text = device.Name;
						});
					}
				}
				else
				{
					BluetoothLEHardwareInterface.ConnectToPeripheral (device.Address, (address) => {

					}, null, (address, service, characteristic) => {

						if (string.IsNullOrEmpty (Services[buttonID]) && string.IsNullOrEmpty (Characteristics[buttonID]))
						{
							Services[buttonID] = FullUUID (service);
							Characteristics[buttonID] = FullUUID (characteristic);
                            
							button.text = device.Name + " connected";
                            Device_Name = device.Name;
                            if (button.text.Contains("connected"))
                            {
                                address = device.Address;
                                ServiceUUID_ = FullUUID("1801");
                                CharactersticUUID_ = FullUUID("2a05");
                            }
						}

					}, null);
				}
			}
		}
	}

    private void Update()
    {
        if(connected_)
        {
            display_text.text = "Waitinf for info : " + ServiceUUID_ + " \r\n" + CharactersticUUID_ + " \r\n" + Device_Name;
            BluetoothLEHardwareInterface.ReadCharacteristic(address, ServiceUUID_, CharactersticUUID_, (characteristic, bytes) => {
                Main_text.text = Encoding.ASCII.GetString(bytes);
            });
        }
    }

    string FullUUID (string uuid)
	{
		if (uuid.Length == 4)
			return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";

		return uuid;
	}
}
