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
    IReadOnlyDictionary<string,Combination> combinations;
    public event Action<InventoryEntry> OnSaleRequested;
    public event Action<InventoryEntry> OnStoreRequested;

    private void Awake()
    {
        //Product Window
        _productImage = transform.Find("Product").GetComponentInChildren<Image>();
        _productImage.enabled=false;

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

        combinations = GameRoot.Instance.GetCombinations();
        _button.onClick.AddListener(Transmogrify);
    }

    private void ProductChoice(ProductChoiceEnum choice,InventoryEntry entry)
    {
        switch (choice)
        {
            case ProductChoiceEnum.SellIt:
                OnSaleRequested?.Invoke(entry);
                ResetTransmogrifierStatusBar();
                break;
            case ProductChoiceEnum.StoreIt:
                OnStoreRequested?.Invoke(entry);
                ResetTransmogrifierStatusBar();
                break;
            case ProductChoiceEnum.ChuckIt:
                _resultText.text += " Chucked!";
                ResetTransmogrifierStatusBar();
                break;
            default:
                _resultText.text = "How did you even do this?";
                ResetTransmogrifierStatusBar();
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
        var key = ETLHelper.MakeKey(ingredients[0].good.GoodName, ingredients[1].good.GoodName);

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

    #region UI Helpers
     private IEnumerator RenderChoice(InventoryEntry result)
    {
        _productImage.enabled=true;
        _productImage.sprite=result.good.GoodSprite;
        _resultText.text = $"You made: {result.quantity} {result.good.GoodName}";

        yield return _waitForSeconds2;

        var rect = _resultText.rectTransform;
        _resultText.text = $"{result.good.GoodName}";
        _resultText.alignment = TextAlignmentOptions.MidlineLeft;
        rect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            rect.rect.width/2f
        );

        //Listeners
        _sellIt.onClick.RemoveAllListeners();
        _storeIt.onClick.RemoveAllListeners();
        _chuckIt.onClick.RemoveAllListeners();
        _sellIt.onClick.AddListener(()=>ProductChoice(ProductChoiceEnum.SellIt,result));
        _storeIt.onClick.AddListener(()=>ProductChoice(ProductChoiceEnum.StoreIt,result));
        _chuckIt.onClick.AddListener(()=>ProductChoice(ProductChoiceEnum.ChuckIt,result));
        
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
        _productImage.enabled=false;
        _resultText.text=string.Empty;
    }
    #endregion

    #region TransMogrification Helpers
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
    #endregion
#region ETL
    
#endregion
}
public enum ProductChoiceEnum
{
    SellIt,
    StoreIt,
    ChuckIt
}