using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletUI : MonoBehaviour
{
    [SerializeField] Text moneyTxt;

    private void Start()
    {
        Wallet.i.OnMoneyChanged += SetMoneyTxt;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetMoneyTxt();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void SetMoneyTxt()
    {
        moneyTxt.text = "$ " + Wallet.i.Money;
    }
}
