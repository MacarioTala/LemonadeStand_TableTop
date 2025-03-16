public class LemonadeStandResultObject
{
    public ResultTypeEnum Result{get;set;}
    public string Message{get;set;}

    public object ExtraData{get;set;}
    
    public static LemonadeStandResultObject Success(object extraData=null) =>
        new(){ Result=ResultTypeEnum.Success, Message="Operation Successful", ExtraData=extraData};
    public static LemonadeStandResultObject Failure(ResultTypeEnum resultType, string message) =>
        new() { Result=resultType, Message=message};

    public override string ToString() => Result.ToString();
    public override bool Equals(object other)
    {
        if(other is LemonadeStandResultObject result)
        {
            return Result == result.Result;
        }
        return false;   
    }

    public static bool operator == (LemonadeStandResultObject left, LemonadeStandResultObject right) 
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator != (LemonadeStandResultObject left, LemonadeStandResultObject right) 
    {
        return !(left == right);
    }

    public override int GetHashCode() => Result.GetHashCode() + Message.GetHashCode();
}