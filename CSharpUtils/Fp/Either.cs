using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Fp
{
    public interface IEither<TLeft, TRight>
    {
        IEither<TLeft, TResult> Bind<TResult>(Func<TRight, IEither<TLeft, TResult>> f);
        IEither<TLeft, TResult> Map<TResult>(Func<TRight, TResult> f);
        TResult Match<TResult>(Func<TLeft, TResult> whenLeft, Func<TRight, TResult> whenRight);

        bool IsLeft { get; }
        bool IsRight { get; }

        TLeft FromLeft(TLeft aDefault);
        TRight FromRight(TRight aDefault);
    }

    public static class Either
    {
        private static (List<TLeft> Lefts, List<TRight> Rights) Zero<TLeft, TRight>() => 
            (new List<TLeft>(), new List<TRight>());

        public static (List<TLeft> Lefts, List<TRight> Rights) PartitionEither<TLeft, TRight>(IEnumerable<IEither<TLeft, TRight>> eithers) => 
            eithers.Aggregate(Zero<TLeft, TRight>(), (state, either) =>
                either.Match(
                  whenLeft: left => { state.Lefts.Add(left); return state; },
                  whenRight: right => { state.Rights.Add(right); return state; }
            ));

        public static IEnumerable<TRight> Rights<TLeft, TRight>(IEnumerable<IEither<TLeft, TRight>> eithers) =>
            eithers.SelectMany(either => 
                either.Match(
                    whenLeft: _ => Enumerable.Empty<TRight>(),
                    whenRight: right => new [] { right }
            ));

        public static IEnumerable<TLeft> Lefts<TLeft, TRight>(IEnumerable<IEither<TLeft, TRight>> eithers) =>
            eithers.SelectMany(either =>
                either.Match(
                    whenLeft: left => new [] { left },
                    whenRight: _ => Enumerable.Empty<TLeft>()
            ));

        public static IEnumerable<IEither<TLeft, TResult>> Traverse<TLeft, TRight, TResult>(Func<TRight, IEnumerable<TResult>> f, IEither<TLeft, TRight> either) =>
            either
                .Map(f)
                .Match<IEnumerable<IEither<TLeft, TResult>>>(
                    whenLeft: left => new [] { new Left<TLeft, TResult>(left) },
                    whenRight: right => right.Select(r => new Right<TLeft, TResult>(r))
                );

        public static IEnumerable<IEither<TLeft, TRight>> Sequence<TLeft, TRight>(IEither<TLeft, IEnumerable<TRight>> either) =>
            Traverse(Fp.Id, either);
    }

    public sealed class Left<TLeft, TRight> : IEither<TLeft, TRight>, IEquatable<Left<TLeft, TRight>>
    {
        private readonly TLeft _left;

        public Left(TLeft left)
        {
            _left = left;
        }

        public IEither<TLeft, TResult> Bind<TResult>(Func<TRight, IEither<TLeft, TResult>> f) => new Left<TLeft, TResult>(_left);

        public IEither<TLeft, TResult> Map<TResult>(Func<TRight, TResult> f) => new Left<TLeft, TResult>(_left);

        public TResult Match<TResult>(Func<TLeft, TResult> whenLeft, Func<TRight, TResult> _) => whenLeft(_left);

        public bool IsLeft => true;
        public bool IsRight => false;

        public TLeft FromLeft(TLeft _) => _left;
        public TRight FromRight(TRight aDefault) => aDefault;
        
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Left<TLeft, TRight> other && Equals(other);
        
        public bool Equals(Left<TLeft, TRight> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TLeft>.Default.Equals(_left, other._left);
        }

        public override int GetHashCode() => EqualityComparer<TLeft>.Default.GetHashCode(_left);
    }

    public sealed class Right<TLeft, TRight> : IEither<TLeft, TRight>, IEquatable<Right<TLeft, TRight>>
    {
        private readonly TRight _right;

        public Right(TRight right)
        {
            _right = right;
        }

        public IEither<TLeft, TResult> Bind<TResult>(Func<TRight, IEither<TLeft, TResult>> f) => f(_right);

        public IEither<TLeft, TResult> Map<TResult>(Func<TRight, TResult> f) => new Right<TLeft, TResult>(f(_right));

        public TResult Match<TResult>(Func<TLeft, TResult> _, Func<TRight, TResult> whenRight) => whenRight(_right);

        public bool IsLeft => false;
        public bool IsRight => true;

        public TLeft FromLeft(TLeft aDefault) => aDefault;
        public TRight FromRight(TRight _) => _right;

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Right<TLeft, TRight> other && Equals(other);

        public bool Equals(Right<TLeft, TRight> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TRight>.Default.Equals(_right, other._right);
        }

        public override int GetHashCode() => EqualityComparer<TRight>.Default.GetHashCode(_right);
    }
}
