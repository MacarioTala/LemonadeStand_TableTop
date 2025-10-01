using System.Collections.Generic;
using UnityEngine;

namespace Sandbox
{
    public class CompanyFactory
    {
        private GameObject companyPrefab;
        private GameObject moneyIndicatorPrefab;
        private readonly List<EconAgent> companies = new();
        private readonly List<GameObject> companyPrefabs = new(); // Track instantiated prefabs
        private List<Good> testGoods;
        private GoodIconMapping iconMapping;
        private Market lemonadeMarket;
        
        // Define the spawn area (slightly smaller than the full playable area)
        private Vector2 spawnAreaMin = new Vector2(-8f, -4f);
        private Vector2 spawnAreaMax = new Vector2(7f, 3f);
        
        // Prefab canvas size in world units
        private float prefabCanvasSize = 5f; // 50 pixels converted to world units (adjust based on your canvas scaler)
        private float spacing = 0.5f; // Additional spacing between prefabs
        private float scaleFactor = 0.5f; // Scale factor for company prefabs
        
        public CompanyFactory(List<Good> goods, GameObject companyPrefab, GoodIconMapping iconMapping, GameObject moneyIndicatorPrefab, Market market)
        {
            testGoods = goods;
            this.companyPrefab = companyPrefab;
            this.iconMapping = iconMapping;
            this.moneyIndicatorPrefab = moneyIndicatorPrefab;
            this.lemonadeMarket = market;
        }
        
        public List<EconAgent> CreateTestCompanies(TheEconomy economy)
        {
            var numberOfCompanies = 4;
            
            // Calculate grid dimensions and positions
            var positions = CalculateGridPositions(numberOfCompanies);
            
            for (int i = 0; i < numberOfCompanies; i++)
            {
                var company = EconAgent.Factory.Create("Company" + i, AgentLevelEnum.Beginner);
                economy.RegisterCompany(company);
                // Associate company with the market
                company.SetMarket(lemonadeMarket);
                
                // Assign LemonadeSellerStrategy to Company0, others get no strategy (will use random actions)
                if (i == 0)
                {
                    var lemonadeStrategy = new LemonadeSellerStrategy();
                    company.SetStrategy(lemonadeStrategy);
                    Debug.Log($"Assigned LemonadeSellerStrategy to {company.Name}");
                }
                
                companies.Add(company);
                

                // Instantiate the company prefab at the calculated grid position
                var position = i < positions.Count ? positions[i] : GetFallbackPosition();
                // spawn them at half the scale
                
                var companyGO = GameObject.Instantiate(companyPrefab, position, Quaternion.identity);
                companyGO.transform.localScale *= 0.5f; // Scale down by half
                companyPrefabs.Add(companyGO); // Store reference to prefab
                companyGO.GetComponent<CompanyView>().Init(company, testGoods, iconMapping, moneyIndicatorPrefab);
            }
            
            foreach (var company in companies)
            {
                foreach (var good in testGoods)
                {
                    company.BuyGood(good, Random.Range(0, 1000), good.GetPrice());
                }
            }
            
            Debug.Log($"Created {numberOfCompanies} test companies in grid formation.");
            return companies;
        }
        
        /// <summary>
        /// Destroys all company prefabs that were created by this factory
        /// </summary>
        public void DestroyAllCompanyPrefabs()
        {
            foreach (var prefab in companyPrefabs)
            {
                if (prefab != null)
                {
                    GameObject.Destroy(prefab);
                }
            }
            companyPrefabs.Clear();
        }
        public List<EconAgent> GetCompanies() => companies;
        
        private List<Vector3> CalculateGridPositions(int numberOfCompanies)
        {
            var positions = new List<Vector3>();
            
            // Calculate available area
            float availableWidth = spawnAreaMax.x - spawnAreaMin.x;
            float availableHeight = spawnAreaMax.y - spawnAreaMin.y;
            
            // Calculate how many companies can fit in each dimension, accounting for scale
            float totalPrefabWidth = prefabCanvasSize * scaleFactor + spacing;
            float totalPrefabHeight = prefabCanvasSize * scaleFactor + spacing;
            
            int maxColumns = Mathf.FloorToInt(availableWidth / totalPrefabWidth);
            int maxRows = Mathf.FloorToInt(availableHeight / totalPrefabHeight);
            
            // Ensure we have at least 1 column and row
            maxColumns = Mathf.Max(1, maxColumns);
            maxRows = Mathf.Max(1, maxRows);
            
            // Calculate optimal grid dimensions for the number of companies
            int columns = Mathf.Min(numberOfCompanies, maxColumns);
            int rows = Mathf.CeilToInt((float)numberOfCompanies / columns);
            
            // If we exceed max rows, redistribute
            if (rows > maxRows)
            {
                rows = maxRows;
                columns = Mathf.CeilToInt((float)numberOfCompanies / rows);
            }
            
            // Calculate starting position (top-left), accounting for scale
            float startX = spawnAreaMin.x + (prefabCanvasSize * scaleFactor / 2f); // Offset by half scaled canvas size to center the prefab
            float startY = spawnAreaMax.y - (prefabCanvasSize * scaleFactor / 2f); // Start from top
            
            // Generate positions
            for (int i = 0; i < numberOfCompanies; i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                float x = startX + (col * totalPrefabWidth);
                float y = startY - (row * totalPrefabHeight);
                
                // Ensure positions stay within bounds, accounting for scale
                x = Mathf.Clamp(x, spawnAreaMin.x + (prefabCanvasSize * scaleFactor/2f), spawnAreaMax.x - (prefabCanvasSize * scaleFactor/2f));
                y = Mathf.Clamp(y, spawnAreaMin.y + (prefabCanvasSize * scaleFactor/2f), spawnAreaMax.y - (prefabCanvasSize * scaleFactor/2f));
                
                positions.Add(new Vector3(x, y, 0f));
            }
            
            return positions;
        }
        
        // Fallback position if grid calculation fails
        private Vector3 GetFallbackPosition()
        {
            float x = Random.Range(spawnAreaMin.x + (prefabCanvasSize * scaleFactor/2f), spawnAreaMax.x - (prefabCanvasSize * scaleFactor/2f));
            float y = Random.Range(spawnAreaMin.y + (prefabCanvasSize * scaleFactor/2f), spawnAreaMax.y - (prefabCanvasSize * scaleFactor/2f));
            return new Vector3(x, y, 0f);
        }
    }
}
