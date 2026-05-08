'*--------------------------------------------------------------------------------------+
'   $safeitemname$.vb
'
'+--------------------------------------------------------------------------------------*/

Option Explicit On
Option Infer On
Option Strict On

#Region "Bentley Namespaces"
Imports BDPN = Bentley.DgnPlatformNET
#End Region

Friend Class $safeitemname$
    Inherits BDPN.DgnPrimitiveTool

    Public Sub New(toolId As Integer, toolPrompt As Integer)
        MyBase.New(toolId, toolPrompt)
    End Sub

#Region "DgnPrimitiveTool Members"

    Protected Overrides Function OnDataButton(ev As BDPN.DgnButtonEvent) As Boolean
        Return True
    End Function

    Protected Overrides Sub OnRestartTool()
        InstallNewInstance()
    End Sub

    Protected Overrides Function OnResetButton(ev As BDPN.DgnButtonEvent) As Boolean
        ExitTool()
        Return True
    End Function

    Protected Overrides Sub ExitTool()
        MyBase.ExitTool()
    End Sub

    Protected Overrides Sub OnDynamicFrame(ev As BDPN.DgnButtonEvent)
    End Sub

    Protected Overrides Function OnInstall() As Boolean
        Return True
    End Function

    Protected Overrides Sub OnPostInstall()
        MyBase.OnPostInstall()
    End Sub

    Public Shared Sub InstallNewInstance(Optional unparsed As String = Nothing)
        Dim tool As New $safeitemname$(0, 0)
        tool.InstallTool()
    End Sub

#End Region

End Class