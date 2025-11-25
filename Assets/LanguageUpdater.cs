using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Yarn.Unity;

public class LanguageUpdater : MonoBehaviour
{
    void Start()
    {
        GetComponent<TextLineProvider>().textLanguageCode = CultureInfo.GetCultureInfoByIetfLanguageTag(Multilang.GetLanguage()).Name;
    }

    public void Refresh()
    {
        Start();
    }
}
