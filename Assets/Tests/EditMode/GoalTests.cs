using NUnit.Framework;
using UnityEngine;
using System.Linq;
[TestFixture]
public class GoalTests
{
    [Test]
    public void GenerateGoals_creates_double_cash_goal_for_company_with_BasicGrowthStrategy()
    {
        //arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.Initialize("Test Company", CompanyLevelEnum.Beginner, new BasicGrowthStrategy());
        company.companyStrategy.GenerateGoals(company);
        var expected_goal = new Goal("Double Initial Cash",
                                      "Double the initial cash of the company",
                                      null,
                                      (c, g) => g.SetOriginalValue("InitialCash", c.Get_cash())
                                      );
        expected_goal.IsGoalMet = c => c.Get_cash() >= expected_goal.GetOriginalValue<decimal>("InitialCash") * 2;
        //Act
        var actual_goal = company.Goals.Where(g => g.Name == "Double Initial Cash").FirstOrDefault();
        //Assert
        Assert.AreEqual(expected_goal, actual_goal);
    }
    [Test]
    public void GenerateGoals_creates_ten_lemonade_goal_for_company_with_BasicGrowthStrategy()
    {
        //arrange
        var company = ScriptableObject.CreateInstance<Company>();
        company.Initialize("Test Company", CompanyLevelEnum.Beginner, new BasicGrowthStrategy());
        company.companyStrategy.GenerateGoals(company);
        var expected_goal = new Goal("Have 10 Lemonade",
                                      "Have 10 Lemonade in stock",
                                      null,
                                      (c, g) => g.SetOriginalValue("InitialLemonade", 0)
                                      )
        {
            IsGoalMet = c => c.GetInventory().GetInventoryEntriesByGood("Lemonade").Sum(e => e.quantity) >= 10
        };
        //Act
        var actual_goal = company.Goals.Where(g => g.Name == "Have 10 Lemonade").FirstOrDefault();
        //Assert
        Assert.AreEqual(expected_goal, actual_goal);
    }
        
}