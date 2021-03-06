﻿Imports System.Windows.Media.Animation

Public Class LoadWindow

    Private WithEvents ca As New ColorAnimation(Colors.Green, New Duration(TimeSpan.FromSeconds(1.5)))
    Private WithEvents checker As New Timers.Timer()
    Private isAnimRunning As Boolean = False
    Private nextGoal As Double = -1
    Private progress As Double = 0

    Private lastAnimation As Double = 0
    Private lastColor As Integer = 0

    Public ApplicationReference As Application
    Public Sub New(ByVal app As Application)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ApplicationReference = app
        checker.Interval = 100
        checker.Start()

        fillRectangle.Fill = New SolidColorBrush(Colors.Blue)
        animateColor()
        With app.Info.Version
            versionText.Text = String.Format("v {0}.{1}.{2}", .Major, .Minor, .Build)
        End With
    End Sub

    Private Sub animateColor()
        fillRectangle.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ca)
    End Sub

    Private Sub HandleColorAnimCompleted(sender As Object, e As EventArgs) Handles ca.Completed
        Select Case lastColor Mod 3
            Case Is = 0
                ca.To = Colors.Red
            Case Is = 1
                ca.To = Colors.Blue
            Case Is = 2
                ca.To = Colors.Green
        End Select
        animateColor()
        lastColor += 1
    End Sub

    Private Sub handleCheckTick(sender As Object, e As EventArgs) Handles checker.Elapsed
        If Application.bc.Count > 0 Then
            Dim info As String = String.Empty
            Application.bc.TryDequeue(info)

            If info.Equals("close") Then
                Me.Dispatcher.Invoke(New Action(AddressOf Me.Close))
            Else
                Me.Dispatcher.Invoke(New Action(Of String)(Sub(ByVal infos As String) statusText.Text = infos), info)
            End If
        End If
    End Sub

End Class
