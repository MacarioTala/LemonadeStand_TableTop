# Lemonade Stand: It's an unspecified point in the future. Can you recreate capitalism? Do you even want to?
A 'reverse 4x' where the war room takes a backseat to the boardroom. 

You wake up to a world where modern finance has stopped working. Starting with a Lemonade Stand, you compete with increasingly sophisticated companies in a bid to become the last economic superpower standing. 

Will a liquidity crisis foil your expansion plans? Will a giant monster raze the orchards? 

... Did the giant monster CAUSE the liquidity crisis? 

You'll navigate systemic shocks, liquidity crises, lobby governments, and face the uncomfortable question: 

"What actually makes money work?"

Featuring a modular economics engine that allows exploration of various economic schools of thought, from the gold standard, to Hayekian markets, and watch them succeed or fail in real time. 

## Project Overview

This project simulates a virtual economy where players can create companies, trade goods, and participate in markets. The economic simulation includes features such as:

- Market simulation with supply and demand mechanics
- Company management with inventory systems
- Trading system with various price modifiers
- Multiple economic strategies and algorithms
- Goods with properties like rarity and expiration, as well as the ability to change the state of Markets and other economic participants. 
- A recipe system that allows the creation of new goods
- An event system that introduces an RNG-based state change to the game, such as droughts, monsters, and found technology.

The simulation serves as a foundation for learning economic principles in an interactive environment.

## Requirements

- Unity 2021.3.37f1 or compatible version
- .NET Standard 2.1 support
- Basic understanding of economic principles (for gameplay)

## Setup Instructions

1. Clone this repository
2. Open Unity Hub and add the project
3. Open the project using Unity 2021.3.37f1
4. Let Unity import and compile all assets
5. Open the 'TextBasedLemonadeStand' scene from `Assets/Scenes
6. Hitting play in game view should start up the opening sequence.

## Troubleshooting
# Unresolved references to missing objects:
1. Unity generates .csproj files based on how scenes are set up per box.
2. Delete your CSProj files
3. Reimport assets (Assets\Reimport All)
4. Regenerate project files Mac/Linux: (Unity\Settings\External Tools\Regenerate project files) Win: (Unity\Preferences\External Tools\Regenerate Project Files)

# Unit tests not visible
1. Test runner isn't enabled by default.
2. Enable using (Window\General\Test Runner)
3. Choose EditMode (at the top of the Test Runner window)

# Unit tests still not visible
1. Make sure you don't have compilation errors.
2. Make sure NUnit is installed using the Unity Package Manager (and not NuGet)

# Unit tests visible, but missing references to Moq
1. Moq is not available in Unity Package Manager at the time of this writing.
2. Instead go via dotnetadd : dotnet add Tests.csproj package Moq --version 4.20.72 (or whatever version happens to be in use by the time you read this)
3. Moq, CastleCore, and Eventlog are the dlls you need in order to make Moq work. Put them in a folder called Moq in Assets/Tests/EditMode
4. delete the .meta files. once you rebuild, Unity will recreate them

# Miscellaneous
1. Library is regenerated on build by Burst (Unity's compile engine), so if you get dll corruption, it is safe to delete this.


## Project Structure

The codebase is organized into modular components that handle different aspects of the economic simulation:
In general, everything is coded to an interface, allowing 

- **Actions**: Defines possible actions within the economic system
- **Company**: Contains company management logic and interfaces
- **Demand**: Implements demand strategies and consumption patterns
- **EntityStrategies**: Contains different business strategy implementations
- **ErrorHandling**: Custom error types and handling
- **FixedCosts**: Models fixed costs for businesses
- **Goods**: Models tradable items with properties
- **Inventory**: Manages storage of goods
- **Markets**: Implements market systems and transactions
- **PriceManagers**: Controls pricing algorithms
- **PriceModifiers**: Modifies prices based on various factors
- **Tests**: Contains unit tests for various components
- **The Economy**: Core singleton that ties everything together
- **Trade**: Handles trade orders and transactions
- **TradeProcessors**: Processes and prioritizes trades

## Core Concepts

### The Economy

The central manager that coordinates all economic activities, accessible through a singleton instance. It initializes markets, registers companies, and manages the overall economic state.

```csharp
// Example of accessing the economy
TheEconomy economy = TheEconomy.Instance;
```

### Markets

Places where goods are traded. Markets track supply, demand, and facilitate transactions between entities.

```csharp
// Example of getting the global market
Market globalMarket = TheEconomy.Instance.GetGlobalMarket();
```

### Goods

Items that can be bought, sold, and used in the economy. Goods have properties such as name, price bands, and rarity.

```csharp
// Creating a good
Good lemon = Good.CreateInstance("Lemon", priceBand, RarityEnum.Common);
lemon.ExpiresAfterPeriods = 1; // Lemons expire after one period
```

### Companies

Entities that participate in the economy by buying, selling, and producing goods.

```csharp
// Registering a company
Company myCompany = ScriptableObject.CreateInstance<Company>();
myCompany.Name = "Lemonade Stand Co.";
TheEconomy.Instance.RegisterCompany(myCompany);
```

## Running Tests

The project includes a comprehensive test suite to validate economic behaviors, you can run them on Unity Test Runner or Rider:

On Unity:
1. Open Unity Test Runner (Window > General > Test Runner)
2. Select EditMode tests
3. Click "Run All" to execute all tests

On Rider:
1. Test Explorer:
- Open the Unit Tests tool window (Alt+8 or View > Tool Windows > Unit Tests)
- Unity tests will appear in this window alongside any .NET tests

1. Running Tests:
- Right-click on test classes or methods in code
- Select "Run Unit Tests" or "Debug Unit Tests"
- Use the green "play" icon next to test classes or methods

1. Test Results:
- View results in the Unit Tests tool window
- Failed tests show detailed information about the failure

## Development Guidelines

- Add new features by extending existing interfaces when possible
- Write tests for new economic behaviors
- Keep simulation complexity manageable for educational purposes
- Document economic algorithms with comments and examples

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run all tests to ensure system integrity
5. Submit a pull request
