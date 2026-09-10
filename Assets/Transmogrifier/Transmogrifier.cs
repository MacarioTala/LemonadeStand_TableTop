using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Transmogrifier:MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds2 = new(2f);
    private Image _productImage;
    [SerializeField] private Button _button;
    [SerializeField]private TransmogrifierSlot _slot1;
    [SerializeField]private TransmogrifierSlot _slot2;
    private GameObject _statusBar;
    private TextMeshProUGUI _resultText;
    private Button _sellIt;
    private Button _storeIt;
    private Button _chuckIt;
    private GameObject _decision;
    private static readonly Color32 _junkFontColour = new(255, 59, 48, 255);
    private static readonly Color32 _successFontColour = new(57, 255, 136, 255);
    readonly Dictionary<string,Combination> combinations = new ();

    //public event Action<InventoryEntry> OnForSaleRequested;

    private void Awake()
    {
        //Product Window
        _productImage = transform.Find("Product").GetComponentInChildren<Image>();
        _productImage.gameObject.SetActive(false);

        //Status Bar
        _statusBar = transform.Find("StatusBar").gameObject;
        _resultText = _statusBar.transform.Find("Result").GetComponentInChildren<TextMeshProUGUI>();
        _resultText.text = "";

        //Decision Buttons
        _decision = _statusBar.transform.Find("Decision").gameObject;
        _decision.SetActive(false);
        _sellIt=_decision.transform.Find("SellIt").GetComponent<Button>();
        _storeIt=_decision.transform.Find("StoreIt").GetComponent<Button>();
        _chuckIt=_decision.transform.Find("ChuckIt").GetComponent<Button>();
        _sellIt.onClick.AddListener(()=>ProductChoice("SellIt"));
        _storeIt.onClick.AddListener(()=>ProductChoice("StoreIt"));
        _chuckIt.onClick.AddListener(()=>ProductChoice("ChuckIt"));

        LoadGoodCombinationsFromResources();
        _button.onClick.AddListener(Transmogrify);
    }

    private void ProductChoice(string choice)
    {
        switch (choice)
        {
            case "SellIt":
               // OnForSaleRequested?.Invoke();
                break;
            case "StoreIt":
                _resultText.text += " Stored!";
                break;
            case "ChuckIt":
                _resultText.text += " Chucked!";
                break;
            default:
                _resultText.text = "How did you even do this?";
                break;
        }
        ResetTransmogrifierStatusBar();
    }

    public void Transmogrify()
    {
        if(_slot1.IsEmpty || _slot2.IsEmpty)
            {
                _resultText.color = _junkFontColour;
                _resultText.text ="Empty reagents. No transmogrify for you!";
            }
        else
        {
            var ingredients = new InventoryEntry[2];
            ingredients[0] = _slot1.GetInventoryEntry();
            ingredients[1] = _slot2.GetInventoryEntry();

            var result = Make(false, ingredients);//TODO:remove hardcode
            if(result==null ||result.good.GoodName=="Nothing")
            {
                _resultText.color=_junkFontColour;
                _resultText.text= $"You made nothing";
                return;
            }

            StartCoroutine(RenderChoice(result));
        }
    }
    private InventoryEntry Make(bool hasRecipe,InventoryEntry[] ingredients)
    {
        var key = MakeKey(ingredients[0].good.GoodName, ingredients[1].good.GoodName);

        combinations.TryGetValue(key,out var combination);
        var isJunk=false;

        if(combination == null)
        {
            _resultText.color = _junkFontColour;
            _resultText.text = $"You made nothing!";
            return null;
        }

        string goodName;
        if (hasRecipe)
        {
            _resultText.color = _successFontColour;
            goodName = combination.Product;
        }
        else
        {
            var roll = MathHelper.RollD(1, 100);
            (goodName, _resultText.color,isJunk) =  roll < combination.PercentageJunk
                    ? (combination.Junk, _junkFontColour,true)
                    : (combination.Product, _successFontColour,false);
        }

        //TODO : Add attributes of goods
        var returnGood = new GoodBuilder()
                    .Named(goodName)
                    .Build();
        
        var qty = GetProducedQuantity(combination,hasRecipe,isJunk);
        var price = GetProducedPrice(combination,ingredients,qty);
        var returnValue = new InventoryEntry(returnGood,qty,price,TheEconomy.Instance.TradingPeriod);

        return returnValue;
    }

    private int? GetProducedPrice(Combination resultingGood, InventoryEntry[] ingredients,int qty)
    {
        var good1Name= resultingGood.Ingredient1;
        var good2Name= resultingGood.Ingredient2;

        var good1 = ingredients.FirstOrDefault(x=>x.good.GoodName==good1Name);
        var good2 = ingredients.FirstOrDefault(x=>x.good.GoodName==good2Name);

        var good1UnitCost = good1.PriceOfGood;
        var good2UnitCost = good2.PriceOfGood;

        var good1ContributionToCost = good1UnitCost*resultingGood.Ingredient1Needed;
        var good2ContributionToCost = good2UnitCost*resultingGood.Ingredient2Needed;

        return (good1ContributionToCost+good2ContributionToCost)/qty;
    }

    private int GetProducedQuantity(Combination resultingGood, bool hasRecipe, bool isJunk)
    {
        
        if(hasRecipe)
            return resultingGood.ProducesQty;
       
        if(isJunk)
            return resultingGood.ProducesQtyJunk;
        
        return MathHelper.RollD(1,resultingGood.MaxQtyWithoutRecipe);
    }
    #region UI Helpers
     private IEnumerator RenderChoice(InventoryEntry result)
    {
        _productImage.gameObject.SetActive(true);
        _resultText.text = $"You made: {result.quantity} {result.good.GoodName}";

        yield return _waitForSeconds2;

        var rect = _resultText.rectTransform;
        _resultText.text = $"{result.good.GoodName}";
        _resultText.alignment = TextAlignmentOptions.MidlineLeft;
        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            rect.rect.width/2f
        );
        
        _decision.SetActive(true);
    }

    private void ResetTransmogrifierStatusBar()
    {
        var rect=_resultText.rectTransform;
        _resultText.alignment = TextAlignmentOptions.Midline;
        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            rect.rect.width*2f
        );
        _decision.SetActive(false);
        _productImage.gameObject.SetActive(false);
    }
    #endregion
#region ETL
    private void LoadGoodCombinationsFromResources()
    {
        var assetPath = "GoodCombinations";
        var csv = Resources.Load<TextAsset>(assetPath);

        var lines = csv.text.Split("\n");
        foreach (var line in lines.Skip(1))
        {
            if(string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Trim().Split(",");

            var ingredient1 = columns[0].Trim();
            var ingredient2 = columns[1].Trim();
            var product = columns[2].Trim();
            var junk = columns[3].Trim();
            int.TryParse(columns[4].Trim(),out var percentageJunk);
            int.TryParse(columns[5].Trim(), out var recipeDiscoveryChance);
            int.TryParse(columns[6].Trim(),out var ingredient1Needed);
            int.TryParse(columns[7].Trim(),out var ingredient2Needed);
            int.TryParse(columns[8].Trim(),out var producesQty);
            int.TryParse(columns[9].Trim(),out var producesQtyJunk);
            int.TryParse(columns[10].Trim(),out var maxQtyWithoutRecipe);

            if(string.IsNullOrWhiteSpace(product)) continue;

            var key = MakeKey(ingredient1,ingredient2);
            combinations.Add(key,new Combination()
                    {
                        Ingredient1=ingredient1,
                        Ingredient2=ingredient2,
                        Product=product,
                        Junk=junk,
                        PercentageJunk=percentageJunk,
                        RecipeDiscoveryChance=recipeDiscoveryChance,
                        Ingredient1Needed=ingredient1Needed,
                        Ingredient2Needed=ingredient2Needed,
                        ProducesQty=producesQty,
                        ProducesQtyJunk=producesQtyJunk,
                        MaxQtyWithoutRecipe=maxQtyWithoutRecipe
                        });
        }
    }
#endregion
    private string MakeKey(string a, string b)
        => string.CompareOrdinal(a,b) < 0? $"{a}|{b}":$"{b}|{a}";
}

internal class Combination
{
    public string Ingredient1;
    public string Ingredient2;
    public string Product;
    public string Junk;
    public int PercentageJunk;
    public int RecipeDiscoveryChance;	
    public int Ingredient1Needed;
    public int Ingredient2Needed;	
    public int ProducesQty;	
    public int ProducesQtyJunk;	
    public int MaxQtyWithoutRecipe;


}