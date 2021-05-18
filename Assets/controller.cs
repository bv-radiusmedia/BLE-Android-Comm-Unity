using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class controller : MonoBehaviour
{

    // -------------------------------------------------------------------
    // this project doesn't offer a "scan" and "connect" buttons - it
    // just gets on and does it!
    // So when it runs, it immediately starts scanning and lists all
    // found devices in a list. Click the one you want and it immediately
    // connects without the need to click a "connect" button
    // -------------------------------------------------------------------

    // -----------------------------------------------------------------
    // change these to match the bluetooth device you're connecting to:
    // -----------------------------------------------------------------
    private string _FullUID = "713d****-503e-4c75-ba94-3148f18d941e";
    private string _serviceUUID = "0000";
    private string _readCharacteristicUUID = "0002";
    private string _writeCharacteristicUUID = "0003";

    // ---------------------------------------------------------------------
    // if you want to connect to different devices, you *could* take a look
    // at the function [void connectTo] and add in some if statements to
    // change the FullUID pattern to match the device
    // ---------------------------------------------------------------------



    // ----------------------------------------------------------------------
    // the following are public because they need to be set by dragging and
    // dropping the various objects in the Unity editor
    // ----------------------------------------------------------------------
    // --------------------------------------------------------
    // these objects need to be set in the Unity visual editor
    // so they are actually linked to something!
    // --------------------------------------------------------
    public Transform PanelScrollContents;            // the panel on which you'll list the devices
    public Text txtDebug;                        // this is just for testing, can be removed safely
    public GameObject connectButton;               // the button to click to connect to a device
    public Text txtData;                        // the text box to type in your "send" data
    public Text txtReceive;                        // the text box data is received into (for testing)


    // -------------------------------------------------------------
    // leave the rest of this junk alone but edit the couple
    // of functions that send and receive data if you like:
    //
    // sendDataBluetooth('string to send');
    //
    // and when data is received, it's passed into the function:
    //
    // receiveText('string received over bluetooth');
    //
    // so change these functions to do whatever you want them to do
    // and leave all the other stuff alone and it should just work!
    // -------------------------------------------------------------

    public bool isConnected = false;
    private bool _readFound = false;
    private bool _writeFound = false;
    private string _connectedID = null;

    private Dictionary<string, string> _peripheralList;
    private float _subscribingTimeout = 0f;
    private bool _scanning = false;
    private bool _connecting = false;

    private int devicesFound = 0;

    private GameObject panelScan;
    private GameObject panelConnected;
    private GameObject panelSettings;


    void connectBluetooth(string addr)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(addr, (address) => {
        },
        (address, serviceUUID) => {
        },
        (address, serviceUUID, characteristicUUID) => {

          // discovered characteristic
          if (IsEqual(serviceUUID, _serviceUUID))
            {
                _connectedID = address;
                isConnected = true;

                if (IsEqual(characteristicUUID, _readCharacteristicUUID))
                {
                    _readFound = true;
                }
                else if (IsEqual(characteristicUUID, _writeCharacteristicUUID))
                {
                    _writeFound = true;
                }

                showConnected();
            }
        }, (address) => {

          // this will get called when the device disconnects
          // be aware that this will also get called when the disconnect
          // is called above. both methods get call for the same action
          // this is for backwards compatibility
          isConnected = false;
        });

        _connecting = false;
    }

    string FullUUID(string uuid)
    {
        // this has changed for the BTLE Mini devices
        // return "713d" + uuid + "-503e-4c75-ba94-3148f18d941e";
        return _FullUID.Replace("****", uuid);
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
        {
            uuid1 = FullUUID(uuid1);
        }
        if (uuid2.Length == 4)
        {
            uuid2 = FullUUID(uuid2);
        }
        return (uuid1.ToUpper().CompareTo(uuid2.ToUpper()) == 0);
    }

    public void connectTo(string sName, string sAddress)
    {
        if (_connecting == false)
        {
            txtDebug.text += "Connect to " + sName + " " + sAddress + "\n";
            _connecting = true;

            // stop scanning
            BluetoothLEHardwareInterface.StopScan();
            _scanning = false;

            // connect to selected device
            connectBluetooth(sAddress);
        }
    }

    public void scan()
    {
        if (_scanning == true)
        {
            txtDebug.text += "Stop scan\n";
            BluetoothLEHardwareInterface.StopScan();
            _scanning = false;
        }
        else
        {

            txtDebug.text += "Start scan\n";
            RemovePeripherals();

            devicesFound = 0;

            // the first callback will only get called the first time this device is seen
            // this is because it gets added to a list in the BluetoothDeviceScript
            // after that only the second callback will get called and only if there is
            // advertising data available
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {
                AddPeripheral(name, address);
            }, (address, name, rssi, advertisingInfo) => { });

            _scanning = true;
        }
    }


    void RemovePeripherals()
    {
        for (int i = 0; i < PanelScrollContents.childCount; ++i)
        {
            GameObject gameObject = PanelScrollContents.GetChild(i).gameObject;
            Destroy(gameObject);
        }

        if (_peripheralList != null)
        {
            _peripheralList.Clear();
        }
    }

    void AddPeripheral(string name, string address)
    {
        if (_peripheralList == null)
        {
            _peripheralList = new Dictionary<string, string>();
        }
        if (!_peripheralList.ContainsKey(address))
        {

            txtDebug.text += "Found " + name + "\n";
            devicesFound++;

            GameObject buttonObject = (GameObject)Instantiate(connectButton);
            connectButtonScript script = buttonObject.GetComponent<connectButtonScript>();
            script.TextName.text = name;
            script.TextAddress.text = address;
            script.controllerScript = this;

            // each button is 50 pixels high
            // the container panel is 544 pixels high
            var h = (544 / 2) - (55 * devicesFound);

            buttonObject.transform.SetParent(PanelScrollContents);
            buttonObject.transform.localScale = new Vector3(1f, 1f, 1f);
            buttonObject.transform.localPosition = new Vector3(0, h, 0);

            _peripheralList[address] = name;

            txtDebug.text += "Button created\n";
        }
    }


    void sendByte(byte value)
    {
        byte[] data = new byte[] { value };
        BluetoothLEHardwareInterface.WriteCharacteristic(_connectedID, FullUUID(_serviceUUID), FullUUID(_writeCharacteristicUUID), data, data.Length, true, (characteristicUUID) => {
            BluetoothLEHardwareInterface.Log("Write Succeeded");
        });
    }

    void sendBytesBluetooth(byte[] data)
    {
        BluetoothLEHardwareInterface.Log(string.Format("data length: {0} uuid: {1}", data.Length.ToString(), FullUUID(_writeCharacteristicUUID)));
        BluetoothLEHardwareInterface.WriteCharacteristic(_connectedID, FullUUID(_serviceUUID), FullUUID(_writeCharacteristicUUID), data, data.Length, true, (characteristicUUID) => {
            BluetoothLEHardwareInterface.Log("Write Succeeded");
        });
    }

    void sendDataBluetooth(string sData)
    {
        if (sData.Length > 0)
        {
            byte[] bytes = ASCIIEncoding.UTF8.GetBytes(sData);
            if (bytes.Length > 0)
            {
                sendBytesBluetooth(bytes);
            }
        }
    }

    void receiveText(string s)
    {
        txtReceive.text += s;
    }

    public void clearReceived()
    {
        txtReceive.text = "";
    }

    public void sendBluetooth()
    {
        string sData = txtData.text;
        txtDebug.text += "Sending: " + sData + "\n";
        sendDataBluetooth(sData);
        txtDebug.text += "Sent";
    }

    void disconnect(Action<string> action)
    {
        BluetoothLEHardwareInterface.DisconnectPeripheral(_connectedID, action);
    }

    void showScan()
    {
        panelSettings.SetActive(false);
        panelConnected.SetActive(false);
        panelScan.SetActive(true);
    }

    void showConnected()
    {
        panelSettings.SetActive(false);
        panelScan.SetActive(false);
        panelConnected.SetActive(true);
    }

    void showSettings()
    {
        panelScan.SetActive(false);
        panelConnected.SetActive(false);
        panelSettings.SetActive(true);
    }

    void Initialise()
    {
        BluetoothLEHardwareInterface.Initialize(true, false, () => { }, (error) => { });
    }

    // Use this for initialization
    void Start()
    {

        panelScan = GameObject.Find("panelScan");
        panelSettings = GameObject.Find("panelSettings");
        panelConnected = GameObject.Find("panelConnected");

        // set up the panels
        showScan();

        // initialise the bluetooth library
        Initialise();

        // start scanning after 1 second
        Invoke("scan", 1000);
    }



    // Update is called once per frame
    void Update()
    {

        if (_readFound && _writeFound)
        {
            _readFound = false;
            _writeFound = false;
            _subscribingTimeout = 1f;
        }

        if (_subscribingTimeout > 0f)
        {
            _subscribingTimeout -= Time.deltaTime;
            if (_subscribingTimeout <= 0f)
            {
                _subscribingTimeout = 0f;

                BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_connectedID, FullUUID(_serviceUUID), FullUUID(_readCharacteristicUUID), (deviceAddress, notification) => {
                }, (deviceAddress2, characteristic, data) => {

                    BluetoothLEHardwareInterface.Log("id: " + _connectedID);
                    if (deviceAddress2.CompareTo(_connectedID) == 0)
                    {
                        BluetoothLEHardwareInterface.Log(string.Format("data length: {0}", data.Length));
                        if (data.Length == 0)
                        {
                            // do nothing
                        }
                        else
                        {
                            string s = ASCIIEncoding.UTF8.GetString(data);
                            BluetoothLEHardwareInterface.Log("data: " + s);
                            receiveText(s);
                        }
                    }

                });

            }

        }

    }
}