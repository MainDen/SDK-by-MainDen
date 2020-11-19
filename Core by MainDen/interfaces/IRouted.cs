// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Enums;

namespace MainDen.Collections
{
    /// <summary>
    /// Represents a methods of a custom directed enumerator.
    /// </summary>
    public interface IRouted
    {
        /// <summary>
        /// Enumerator extension route type.
        /// </summary>
        RouteType RouteType { get; }

        /// <summary>
        /// Enumerator extension route mode.
        /// </summary>
        RouteMode RouteMode { get; }

        /// <summary>
        /// The index of the current item.
        /// </summary>
        int? CurrentIndex { get; }

        /// <summary>
        /// The index of the begin item.
        /// </summary>
        int? BeginIndex { get; }

        /// <summary>
        /// The index of the end item.
        /// </summary>
        int? EndIndex { get; }

        /// <summary>
        /// The index of the next item.
        /// </summary>
        int? NextIndex { get; }

        /// <summary>
        /// The current item.
        /// </summary>
        object Current { get; }

        /// <summary>
        /// The begin item.
        /// </summary>
        object Begin { get; }

        /// <summary>
        /// The end item.
        /// </summary>
        object End { get; }

        /// <summary>
        /// The next item.
        /// </summary>
        object Next { get; }

        /// <summary>
        /// The number of items in the remainder.
        /// </summary>
        int RemainderCount { get; }

        /// <summary>
        /// Moves the custom directed enumerator to the next item in the collection.
        /// </summary>
        void MoveNext();

        /// <summary>
        /// Reverses the direction of movement of the custom directional enumerator if the current route type supports reversing.
        /// </summary>
        /// <remarks>
        /// For route types that do not support reversing, use the <see cref="SetRouteMode(RouteMode)"/>.
        /// </remarks>
        void ReverseDirection();

        /// <summary>
        /// Sets the <see cref="RouteType"/>.
        /// </summary>
        /// <param name="routeType"></param>
        void SetRouteType(RouteType routeType);

        /// <summary>
        /// Sets the <see cref="RouteMode"/>.
        /// </summary>
        /// <param name="routeMode"></param>
        void SetRouteMode(RouteMode routeMode);

        /// <summary>
        /// Sets the <see cref="CurrentIndex"/>.
        /// </summary>
        /// <param name="index"></param>
        void SetCurrentIndex(int? index);
        
        /// <summary>
        /// Sets initial values.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Updates the values for the current state.
        /// </summary>
        void Update();

        /// <summary>
        /// Updates the values for the current state with new <see cref="CurrentIndex"/>.
        /// </summary>
        /// <param name="current"></param>
        void Update(int? current);

        /// <summary>
        /// Updates the values for the current state where <see cref="CurrentIndex"/> determined by the rule:
        /// <c>
        /// <para><see langword="if"/> (<paramref name="offset"/> &gt; 0 &amp;&amp; <paramref name="index"/> &lt;= <see cref="CurrentIndex"/>)</para>
        /// <para><see langword="    "/><see cref="CurrentIndex"/> += <paramref name="offset"/>;</para>
        /// <para><see langword="else if"/> (<paramref name="offset"/> &lt; 0)</para>
        /// <para><see langword="    if"/> (<paramref name="index"/> &lt;= <see cref="CurrentIndex"/> + <paramref name="offset"/>)</para>
        /// <para><see langword="        "/><see cref="CurrentIndex"/> += <paramref name="offset"/>;</para>
        /// <para><see langword="    else if"/> (<paramref name="index"/> &lt;= <see cref="CurrentIndex"/>)</para>
        /// <para><see langword="        "/><see cref="CurrentIndex"/> = <see langword="null"/>;</para>
        /// </c>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        void Update(int index, int offset);
    }
}