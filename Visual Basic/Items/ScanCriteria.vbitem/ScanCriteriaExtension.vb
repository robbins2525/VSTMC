'*--------------------------------------------------------------------------------------+
'   $safeitemname$.vb
'
'+--------------------------------------------------------------------------------------*/

Option Explicit On
Option Infer On
Option Strict On

Imports System.Runtime.CompilerServices

#Region "Bentley Namespaces"
Imports BDPN = Bentley.DgnPlatformNET
#End Region

Module ScanCriteriaExtensions

    ''' <summary>
    ''' Scan Criteria Extensions to Bentley.DgnPlatformNET.ScanCriteria.
    ''' <remarks>
    ''' Published on Be Communities MicroStation Programming Forum by Robert Menger
    ''' (http://www.menger-engineering.com/en/).
    ''' Additional code to ensure the Bitmask could accommodate all element types
    ''' was suggested by Robert Hook of Bentley Systems.
    ''' </remarks>
    ''' </summary>

    ''' <summary>
    ''' Given an array of MSElementType values, create a BitMask for the scanner.
    ''' </summary>
    <Extension>
    Public Function AddElementTypes(criteria As BDPN.ScanCriteria, ParamArray types As BDPN.MSElementType()) As BDPN.ScanCriteria
        If criteria Is Nothing Then Throw New ArgumentNullException(NameOf(criteria))
        If types Is Nothing OrElse types.Length = 0 Then Return criteria

        Dim bitMask As BDPN.BitMask = SetElementTypeBitMask(types)
        criteria.SetElementTypeTest(bitMask)
        Return criteria
    End Function

    ''' <summary>
    ''' Scan a DGN model and return a list of elements.
    ''' </summary>
    ''' <param name="criteria">ScanCriteria object.</param>
    ''' <returns>IEnumerable(Element) list of elements.</returns>
    <Extension>
    Public Function Scan(criteria As BDPN.ScanCriteria) As IEnumerable(Of BDPN.Elements.Element)
        If criteria Is Nothing Then Throw New ArgumentNullException(NameOf(criteria))

        Dim result As New List(Of BDPN.Elements.Element)()

        criteria.Scan(
            Function(e, m)
                result.Add(e)
                Return BDPN.StatusInt.Success
            End Function)

        Return result
    End Function

    ''' <summary>
    ''' Get the largest value from an array of numbers.
    ''' </summary>
    ''' <typeparam name="T">Comparable type including enums.</typeparam>
    ''' <param name="numbers">Array of values.</param>
    ''' <returns>Largest value in the array.</returns>
    Public Function Largest(Of T As IComparable)(numbers As T()) As T
        If numbers Is Nothing OrElse numbers.Length = 0 Then
            Throw New ArgumentException("The array must contain at least one value.", NameOf(numbers))
        End If

        Array.Sort(numbers)
        Return numbers(numbers.Length - 1)
    End Function

    ''' <summary>
    ''' Set bits in a BitMask of MSElementType.
    ''' <remarks>Mechanics of resizing a BitMask shown by Robert Hook on Be Communities.</remarks>
    ''' </summary>
    ''' <param name="types">Array of MSElementType values.</param>
    ''' <returns>BitMask</returns>
    Private Function SetElementTypeBitMask(types As BDPN.MSElementType()) As BDPN.BitMask
        If types Is Nothing OrElse types.Length = 0 Then
            Throw New ArgumentException("At least one element type must be provided.", NameOf(types))
        End If

        Dim largestType As BDPN.MSElementType = Largest(types)

        Dim bmSize As UInteger = 1UI + CUInt(largestType)
        bmSize = (bmSize + 7UI) \ 8UI
        bmSize = (bmSize * 16UI) - 15UI

        Dim bitMask As New BDPN.BitMask(False)
        bitMask.EnsureCapacity(bmSize + 1UI)

        For Each elementType As BDPN.MSElementType In types
            bitMask.SetBit(CUInt(elementType) - 1UI, True)
        Next

        Return bitMask
    End Function

End Module