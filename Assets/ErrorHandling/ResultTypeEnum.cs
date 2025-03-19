using System;

public enum ResultTypeEnum
{
    Success = 0,
    InsufficientCash=1,
    OrderHasNoBuyer=2,
    OrderHasNoGood=3,
    OrderHasInvalidPrice=4,
    OrderHasInvalidQuantity=5,
    ContextHasNoTrade=6,
    OrderHasNoSeller=7,
    MarketNotSet=8,
    SpreadHasNoBid=9,
    SpreadHasNoAsk=10,
    SpreadHasNoGood=11,
    DuplicateOrder=12,
    SelfTrade=13,
    NoSupplierForGood=14,
    NoMatchingCounterParties=15,
    PartialFill=16,
    OrderHasNoSubmittingCompany=17,
    [Obsolete("Use InsufficientCash Instead.")]//refactor when possible
    InsufficientFunds=18,
    InsufficientGoods=19,
    OrderHasNoActors=20,
    InvalidTransaction=21,
    PrimaryOrderNotSet=22,
    SomeOrdersNotProcessed = 23,
    ElasticityNotFound = 24,
    CompanyNotFound = 25,
}