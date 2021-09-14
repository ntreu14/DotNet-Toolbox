using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Fp
{
  public abstract class Monoid<T>
  {
    public abstract T Empty { get; }
    public abstract T Append(T first, T second);

    protected T CheckForNullsOrAppend(T first, T second, Func<T> thunk) =>
      (first, second) switch
      {
        (null, null) => Empty,
        (null, _)    => second,
        (_, null)    => first,
        _            => thunk()
      };

    public virtual T Concat(IEnumerable<T> xs) => xs.Aggregate(Empty, Append);
  }

  public class StringMonoid : Monoid<string>
  {
    public override string Empty => "";

    public override string Append(string first, string second) =>
      CheckForNullsOrAppend(first, second, () => $"{first}{second}");
  }

  public class ListMonoid<T> : Monoid<List<T>>
  {
    public override List<T> Empty => new List<T>();

    public override List<T> Append(List<T> first, List<T> second) =>
      CheckForNullsOrAppend(first, second, () => first.Concat(second).ToList());
  }
}
