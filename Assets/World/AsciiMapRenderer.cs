using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class AsciiMapRenderer:MonoBehaviour
{
    [SerializeField]private TMP_Text _TMP_Output;

    public void Hello() => _TMP_Output.text = "Hello";
    public void RenderCountry(Country country)
    {
        var neighbourhoods = country.GetNeighbourhoods();

        if(neighbourhoods.Count==0)
        {
            _TMP_Output.text = "Empty country";
            return;
        }

        var builder = new StringBuilder();
        var visited = new HashSet<Neighbourhood>();

        RecursivelyRenderNodes(
            neighbourhoods[0], //start from first node
            null,
            "",
            true,
            visited,
            builder
        );
       

        _TMP_Output.text = builder.ToString();
    }

    private void RecursivelyRenderNodes(
        Neighbourhood current,
        Neighbourhood parent,
        string connector,
        bool isLastConnectionInNode,
        HashSet<Neighbourhood> visited,
        StringBuilder builder
    )
    {
        visited.Add(current);
        builder.Append(connector);

        if(parent != null)
            builder.Append(isLastConnectionInNode?" └─" : "├─");
        
        builder.AppendLine(current.Name);

        var children = new List<Neighbourhood>();

        foreach(var neighbour in current.Neighbours)
        {
            if(!visited.Contains(neighbour))
                children.Add(neighbour);
        }
        
        for (var i=0;i<children.Count;i++)
        {
            var childIsLast = i == children.Count -1;
            var childConnector = connector + 
            (
                parent == null?"" : 
                isLastConnectionInNode ?" ":  
                " |"
            );

            RecursivelyRenderNodes(
                children[i],
                current,
                childConnector,
                childIsLast,
                visited,
                builder
            );
        }
    }

    public void GetLocalDeliveryCostBetweenNeighbourhoods(Country country)
    {
        var builder = new StringBuilder();
        var neighbourhoods = country.GetNeighbourhoods();
        for(var i=0;i<neighbourhoods.Count;i++)
        {
            for(var j=i+1;j<neighbourhoods.Count; j++)
            {
                var from = neighbourhoods[i];
                var to = neighbourhoods[j];

                builder.AppendLine(
                    $"{from.Name} to {to.Name}: " +
                    $"{from.GetLocalDeliveryCostFrom(to,DeliverySizeEnum.small)}"
                );
            }
        }

        _TMP_Output.text+=builder.ToString();
    }

}