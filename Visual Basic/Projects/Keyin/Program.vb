'*--------------------------------------------------------------------------------------+
'   $safeitemname$.vb
''
'+--------------------------------------------------------------------------------------*/

Imports BM = Bentley.MstnPlatformNET

$AddinAttribute$
Friend NotInheritable Class Program
    Inherits BM.AddIn

    Public Shared ReadOnly Property Instance As Program
        Get
            If _instance Is Nothing Then
                Throw New InvalidOperationException("Add-in instance has not been initialized yet.")
            End If
            Return _instance
        End Get
    End Property

    Public Shared ReadOnly Property TryInstance As Program
        Get
            Return _instance
        End Get
    End Property

    Public Shared Function TryGetInstance(ByRef value As Program) As Boolean
        value = _instance
        Return value IsNot Nothing
    End Function

    Private Shared _instance As Program

    Private Sub New(mdlDesc As IntPtr)
        MyBase.New(mdlDesc)

        If _instance IsNot Nothing AndAlso Not Object.ReferenceEquals(_instance, Me) Then
            Throw New InvalidOperationException("Add-in instance already set.")
        End If

        _instance = Me
    End Sub

    ''' <summary>The AddIn loader creates an instance of a class
    ''' derived from Bentley.MicroStation.AddIn and invokes Run.
    ''' </summary>
    Protected Overrides Function Run(commandLine As String()) As Integer
        ' Get the localized resources

        ' Register event handlers
        AddHandler ReloadEvent, AddressOf OnReloadEvent
        AddHandler UnloadedEvent, AddressOf OnUnloadedEvent

        Return 0
    End Function

    ''' <summary>
    ''' Handles MicroStation UNLOADED event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="eventArgs"></param>
    Private Sub OnReloadEvent(sender As BM.AddIn, eventArgs As ReloadEventArgs) Handles MyBase.ReloadEvent
        'TODO: add specific handling For this Event here

    End Sub

    ''' <summary>
    ''' Handles MicroStation UNLOADED event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="eventArgs"></param>
    Protected Sub OnUnloadedEvent(sender As BM.AddIn, eventArgs As UnloadedEventArgs) Handles MyBase.UnloadedEvent
        'TODO: add specific handling For this Event here

    End Sub

    ''' <summary>
    ''' Handles MDL ONUNLOAD requests when the application Is being unloaded.
    ''' </summary>
    ''' <param name="eventArgs"></param>
    Protected Overrides Sub OnUnloading(eventArgs As UnloadingEventArgs)
        Try
            ' cleanup
        Finally
            MyBase.OnUnloading(eventArgs)
            _instance = Nothing
        End Try
    End Sub
End Class