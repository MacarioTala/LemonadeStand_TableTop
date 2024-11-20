public delegate void Action(ActionContext context);
public class AllowedAction
{
    public string Name { get; set; }
    public string Description { get; set; } 
    public Action ActionToExecute { get; set; }
    public AllowedAction(string name, string description, Action action)
    {
        Name = name;
        Description = description;
        ActionToExecute = action;
    }
}