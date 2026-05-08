/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#region System Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

#region Bentley Namespaces
using BDPN = Bentley.DgnPlatformNET;
#endregion

namespace $rootnamespace$
{
    /// <summary>
    /// Extension methods for <see cref="BDPN.ScanCriteria"/>.
    /// </summary>
    public static class $safeitemname$
    {
        /// <summary>
        /// Adds an element type filter to the scan criteria.
        /// </summary>
        /// <param name="criteria">The scan criteria to modify.</param>
        /// <param name="types">The element types to include.</param>
        /// <returns>The same <paramref name="criteria"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="criteria"/> or <paramref name="types"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when no element types are provided.</exception>
        public static BDPN.ScanCriteria AddElementTypes(
            this BDPN.ScanCriteria criteria,
            params BDPN.MSElementType[] types)
{
    if (criteria == null)
        throw new ArgumentNullException(nameof(criteria));

    if (types == null)
        throw new ArgumentNullException(nameof(types));

    if (types.Length == 0)
        throw new ArgumentException("At least one element type must be provided.", nameof(types));

    var bitMask = CreateElementTypeBitMask(types);
    criteria.SetElementTypeTest(bitMask);
    return criteria;
}

/// <summary>
/// Scans a DGN model and returns all matching elements.
/// </summary>
/// <param name="criteria">The scan criteria.</param>
/// <returns>A materialized list of matching elements.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="criteria"/> is null.</exception>
public static IReadOnlyList<BDPN.Elements.Element> ScanElements(this BDPN.ScanCriteria criteria)
{
    if (criteria == null)
        throw new ArgumentNullException(nameof(criteria));

    var result = new List<BDPN.Elements.Element>();

    criteria.Scan((element, model) =>
    {
        result.Add(element);
        return BDPN.StatusInt.Success;
    });

    return result;
}

/// <summary>
/// Creates a bit mask for the supplied MicroStation element types.
/// </summary>
private static BDPN.BitMask CreateElementTypeBitMask(IEnumerable<BDPN.MSElementType> types)
{
    var typeArray = types as BDPN.MSElementType[] ?? types.ToArray();

    if (typeArray.Length == 0)
        throw new ArgumentException("At least one element type must be provided.", nameof(types));

    var largestType = typeArray.Max();
    uint bitCount = (uint)largestType;
    uint byteCount = (bitCount + 1 + 7) / 8;
    uint capacity = (byteCount * 16) - 15;

    var bitMask = new BDPN.BitMask(false);

    bitMask.EnsureCapacity(capacity + 1);

    foreach (var type in typeArray.Distinct())
    {
        bitMask.SetBit((uint)type - 1, true);
    }

    return bitMask;
}
}
}