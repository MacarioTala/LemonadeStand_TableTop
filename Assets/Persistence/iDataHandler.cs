using System;
using System.Collections.Generic;

public interface iDataHandler<T>
{
    void Delete(int id);
    T Load(string path);
    IEnumerable<T> LoadHistorical(Func<T,bool> predicate);
    void Save(T data);
    
}
