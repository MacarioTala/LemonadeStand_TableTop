public class LemonadeStandResultObject
{
    public ResultTypeEnum Result{get;set;}
    public string Message{get;set;}
    
    public static LemonadeStandResultObject Success() =>
        new(){ Result=ResultTypeEnum.Success, Message="Operation Successful"};
    public static LemonadeStandResultObject Failure(ResultTypeEnum resultType, string message) =>
        new() { Result=resultType, Message=message};

    public override string ToString() => Result.ToString();
    public override bool Equals(object other)
    {
        if(other is LemonadeStandResultObject result)
        {
            return Result == result.Result && Message == result.Message;
        }
        return false;   
    }
    public override int GetHashCode() => Result.GetHashCode() + Message.GetHashCode();
}