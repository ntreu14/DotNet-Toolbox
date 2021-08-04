using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Fp
{
    public interface IOption<T>
    {
        IOption<TResult> Bind<TResult>(Func<T, IOption<TResult>> f);
        IOption<TResult> Map<TResult>(Func<T, TResult> f);
        TResult Match<TResult>(Func<T, TResult> whenSome, Func<TResult> whenNone);
        
        T Or(T aDefault);
        T Or(Func<T> thunk);
        IOption<T> OrElse(IOption<T> ifNone);

        bool IsSome { get; }
        bool IsNone { get; }

        List<T> ToList();
    }

    public static class Option
    {
        public static IOption<T> FromNullable<T>(T? value) where T : struct =>
            value.HasValue ? Some.Of(value.Value) : new None<T>();

        public static IOption<T> FromMaybeNull<T>(T value) where T : class =>
            value != null ? Some.Of(value) : new None<T>();
        
        public static IOption<TElem> TryFind<TElem>(IEnumerable<TElem> values, Func<TElem, bool> predicate) =>
            values.Any(predicate) ? Some.Of(values.First(predicate)) : new None<TElem>();

        public static IEnumerable<TResult> Choose<T, TResult>(IEnumerable<T> xs, Func<T, IOption<TResult>> chooser) =>
            xs.SelectMany(x => chooser(x).ToList());

        public delegate bool ParserDelegate<in TValue, TResult>(TValue str, out TResult result);

        public static IOption<TResult> TryParse<TValue, TResult>(TValue str, ParserDelegate<TValue, TResult> parser) =>
            parser(str, out var perhapsResult) ? Some.Of(perhapsResult) : new None<TResult>();

        public static IOption<TElem> TryKey<TKey, TElem>(TKey key, IDictionary<TKey, TElem> dict) =>
          TryParse<TKey, TElem>(key, dict.TryGetValue);  

        public static IOption<int> TryParseInt(string str) => TryParse<string, int>(str, int.TryParse);

        public static IOption<DateTime> TryParseDateTime(string str) => TryParse<string, DateTime>(str, DateTime.TryParse);
    }

    public static class Some
    {
        public static IOption<T> Of<T>(T value) => new Some<T>(value);
    }

    public sealed class Some<T> : IOption<T>, IEquatable<Some<T>>
    {
        private readonly T _value;

        public Some(T value)
        {
            _value = value;
        }

        public IOption<TResult> Bind<TResult>(Func<T, IOption<TResult>> f) => f(_value);

        public IOption<TResult> Map<TResult>(Func<T, TResult> f) => new Some<TResult>(f(_value));

        public TResult Match<TResult>(Func<T, TResult> whenSome, Func<TResult> _) => whenSome(_value);

        public T Or(T _) => _value;
        public T Or(Func<T> _) => _value;
        public IOption<T> OrElse(IOption<T> _) => new Some<T>(_value);

        public bool IsSome => true;
        public bool IsNone => false;

        public List<T> ToList() => new List<T> { _value };

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Some<T> other && Equals(other);

        public bool Equals(Some<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_value);
    }

    public sealed class None<T> : IOption<T>, IEquatable<None<T>>
    {
        public IOption<TResult> Bind<TResult>(Func<T, IOption<TResult>> _) => new None<TResult>();

        public IOption<TResult> Map<TResult>(Func<T, TResult> _) => new None<TResult>();

        public TResult Match<TResult>(Func<T, TResult> _, Func<TResult> whenNone) => whenNone();

        public T Or(T aDefault) => aDefault;
        public T Or(Func<T> thunk) => thunk();
        public IOption<T> OrElse(IOption<T> ifNone) => ifNone;

        public bool IsSome => false;
        public bool IsNone => true;

        public List<T> ToList() => new List<T>();

        public bool Equals(None<T> _) => true;

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is None<T> other && Equals(other);

        public override int GetHashCode() => 0;
    }
}
