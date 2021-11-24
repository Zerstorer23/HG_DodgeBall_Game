using System.Collections.Generic;

[System.Serializable]
public class LocalizationData 
{
   // public LocalizationItem[] items;
    public List<LocalizationItem> items;
    public LocalizationData() {
        items = new List<LocalizationItem>(); 
    }
   /* public LocalizationData()
    {
        items = null;
    }*/
}

