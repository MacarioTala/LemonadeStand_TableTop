using System;
using System.Collections.Generic;

/// <summary>
/// If you are here, know that this is the event bus for system events, not gameplay events.
/// This class publishes events that subscribers can subscribe to so that the publisher 
///     neither knows nor cares who its subscribers are, or even that they have subscriptions.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, Dictionary<int,Delegate>> _handlers = new();

    private readonly Dictionary<Type,object> _stickyEvent = new();

    private readonly Queue<object> _queue = new();

    private int _nextId = 1;

    public bool QueuedDelivery{get;set;} = false;

    public SubscriptionToken Subscribe<T>(Action<T> handler, bool replaySticky = true)
    {
        var type = typeof(T);
        if(!_handlers.TryGetValue(type, out var map))
        {
            map = new Dictionary<int, Delegate>();
            _handlers[type] = map;
        }

        var id = _nextId++;
        map[id] = handler ?? throw new ArgumentNullException(nameof(handler));

        //Sticky events
        if(replaySticky && _stickyEvent.TryGetValue(type, out var last))
        {
            try
            {
                handler((T)last);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }

        return new SubscriptionToken(this,type, id);
    }

    internal void Unsubscribe(SubscriptionToken token)
    {
        if(!token.IsValid) return;

        if(_handlers.TryGetValue(token.EventType, out var map))
        {
            map.Remove(token.Id);
            if(map.Count == 0) _handlers.Remove(token.EventType);
        }

        token.Invalidate();
    }
    public void Publish<T> (T evt, bool makeSticky=false)
    {
        if(makeSticky)
            _stickyEvent[typeof(T)] = evt;
        
        if(QueuedDelivery)
        {
            _queue.Enqueue(evt!);
            return;
        }

        Dispatch(evt!);
    }

    public void DrainQueue(int maxPerFrame = 100)
    {
        var count = 0;
        while(_queue.Count>0 && count < maxPerFrame )
        {
            Dispatch(_queue.Dequeue());
            count++;
        }
    }

    public bool TryGetSticky<T> (out T value)
    {
        if(_stickyEvent.TryGetValue(typeof(T),out var obj))
        {
            value =(T)obj;
            return true;
        }
        value = default;
        return false;
    }




    private void Dispatch(object evt)
    {
        var type = evt.GetType();

        if(!_handlers.TryGetValue(type, out var map)) return;

        //Guard against unsub during dispatch
        var snapshot = new List<Delegate>(map.Values);
        foreach(var subscriberPair in snapshot)
        {
            try
            {
                subscriberPair.DynamicInvoke(evt);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            
        }
    }
}

public readonly struct SubscriptionToken : IDisposable
{
    private readonly EventBus _bus;
    public readonly Type EventType;
    public readonly int Id;
    private readonly bool _valid;

    internal SubscriptionToken(EventBus bus, Type eventType, int id)
    {
        _bus = bus;
        EventType = eventType;
        Id=id;
        _valid = true;
    }

    public bool IsValid => _valid && _bus != null && Id!=0;

public void Dispose()
    {
        _bus?.Unsubscribe(this);
    }

    internal void Invalidate()
    {
     //Intentional Nop. 
    }
}